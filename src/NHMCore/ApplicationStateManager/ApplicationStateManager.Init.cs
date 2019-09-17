// SHARED
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Mining.Plugins;
using NHMCore.Stats;
using NHMCore.Utils;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NHM.DeviceDetection;
using NHM.DeviceMonitoring;
using static NHMCore.Translations;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        private static bool isInitFinished = false;

        public static bool FailedRamCheck { get; internal set; }

        private class LoaderConverter : IStartupLoader
        {
            public IProgress<(string, int)> PrimaryProgress { get; }
            public IProgress<(string, int)> SecondaryProgress { get; }
            public string PrimaryTitle { get; set; }
            public string SecondaryTitle { get; set; }
            public bool SecondaryVisible { get; set; }

            public LoaderConverter(IProgress<(string, int)> prog, IProgress<(string, int)> prog2)
            {
                PrimaryProgress = prog;
                SecondaryProgress = prog2;
            }
        }

        public static async Task InitializeManagersAndMiners(IStartupLoader loader)
        {
            try
            {
                var allSteps = 15;
                var currentStep = 0;
                var nextProgPerc = new Func<int>(() =>
                {
                    ++currentStep;
                    var perc = (int)(((double)currentStep / allSteps) * 100);
                    if (perc > 100) return 100;
                    return perc;
                });
                // STEP
                // Checking System Memory
                loader.PrimaryProgress.Report((Tr("Checking System Specs"), nextProgPerc()));
                await Task.Run(() => SystemSpecs.QueryWin32_OperatingSystemDataAndLog());

                // TODO extract in function
                #region Device Detection

                // STEP +3
                Func<DeviceDetectionStep, string> detectionStepMessage = (DeviceDetectionStep step) =>
                {
                    switch (step)
                    {
                        case DeviceDetectionStep.CPU:
                            return Tr("Checking CPU Info");
                        case DeviceDetectionStep.NVIDIA_CUDA:
                            return Tr("Querying CUDA devices");
                        case DeviceDetectionStep.AMD_OpenCL:
                            return Tr("Checking AMD OpenCL GPUs");
                        default: //DeviceDetectionStep.WMIWMIVideoControllers
                            return Tr("Checking Windows Video Controllers");
                    }
                };
                var devDetectionProgress = new Progress<DeviceDetectionStep>(step => {
                    var msg = detectionStepMessage(step);
                    loader.PrimaryProgress.Report((msg, nextProgPerc()));
                });
                await DeviceDetection.DetectDevices(devDetectionProgress);
                // add devices
                var detectionResult = DeviceDetection.DetectionResult;
                var index = 0;
                var cpuCount = 0;
                var cudaCount = 0;
                var amdCount = 0;
                foreach (var cDev in DeviceDetection.GetDetectedDevices())
                {
                    var nameCount = "";
                    if (cDev.DeviceType == DeviceType.CPU)
                    {
                        cpuCount++;
                        nameCount = $"CPU#{cpuCount}";
                    }
                    if (cDev.DeviceType == DeviceType.AMD)
                    {
                        amdCount++;
                        nameCount = $"AMD#{amdCount}";
                    }
                    if (cDev.DeviceType == DeviceType.NVIDIA)
                    {
                        cudaCount++;
                        nameCount = $"GPU#{cudaCount}";
                    }
                    AvailableDevices.AddDevice(new ComputeDevice(cDev, index++, nameCount));
                }
                AvailableDevices.UncheckCpuIfGpu();
                FailedRamCheck = SystemSpecs.CheckRam(AvailableDevices.AvailGpus, AvailableDevices.AvailNvidiaGpuRam, AvailableDevices.AvailAmdGpuRam);
                // no compatible devices? exit
                if (AvailableDevices.Devices.Count == 0)
                {
                    var result = MessageBox.Show(Tr("No supported devices are found. Select the OK button for help or cancel to continue."),
                        Tr("No Supported Devices"),
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.OK)
                    {
                        Process.Start(Links.NhmNoDevHelp);
                    }
                    Application.Exit();
                    return;
                }

                // STEP
                DeviceMonitorManager.DisableDevicePowerModeSettings = ConfigManager.GeneralConfig.DisableDevicePowerModeSettings;
                loader.PrimaryProgress.Report((Tr("Initializing device monitoring"), nextProgPerc()));
                var monitors = await DeviceMonitorManager.GetDeviceMonitors(AvailableDevices.Devices.Select(d => d.BaseDevice), detectionResult.IsDCHDriver);
                foreach (var monitor in monitors)
                {
                    var dev = AvailableDevices.GetDeviceWithUuid(monitor.UUID);
                    dev.SetDeviceMonitor(monitor);
                }
                // now init device settings
                ConfigManager.InitDeviceSettings();
                #endregion Device Detection

                // STEP
                // load plugins
                loader.PrimaryProgress.Report((Tr("Loading miner plugins..."), nextProgPerc()));
                // Plugin Loading
                MinerPluginsManager.LoadMinerPlugins();
                // commit again benchmarks after loading plugins
                ConfigManager.CommitBenchmarks();
                /////////////////////////////////////////////
                /////// from here on we have our devices and Miners initialized
                UpdateDevicesStatesAndStartDeviceRefreshTimer();

                // STEP
                // connect to nhmws
                loader.PrimaryProgress.Report((Tr("Connecting to nhmws..."), nextProgPerc()));
                // Init ws connection
                NiceHashStats.StartConnection(Nhmws.NhmSocketAddress);

                // STEP
                // disable windows error reporting
                loader.PrimaryProgress.Report((Tr("Setting Windows error reporting..."), nextProgPerc()));
                Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);

                // STEP
                // Nvidia p0
                loader.PrimaryProgress.Report((Tr("Changing all supported NVIDIA GPUs to P0 state..."), nextProgPerc()));
                if (ConfigManager.GeneralConfig.NVIDIAP0State && AvailableDevices.HasNvidia)
                {
                    Helpers.SetNvidiaP0State();
                }

                // STEP
                // Downloading integrated plugins bins, TODO put this in some internals settings
                var hasMissingMinerBins = MinerPluginsManager.GetMissingMiners().Count > 0;
                if (hasMissingMinerBins)
                {
                    loader.SecondaryTitle = Tr("Downloading Miner Binaries");
                    loader.SecondaryVisible = true;

                    loader.PrimaryProgress.Report((Tr("Downloading Miner Binaries..."), nextProgPerc()));
                    await MinerPluginsManager.DownloadMissingMinersBins(loader.SecondaryProgress, ExitApplication.Token);
                    //await MinersDownloader.MinersDownloadManager.DownloadAndExtractOpenSourceMinersWithMyDownloaderAsync(progressDownload, ExitApplication.Token);
                    loader.SecondaryVisible = false;
                    if (ExitApplication.IsCancellationRequested) return;
                }

                // STEP
                // Update miner plugin binaries
                var hasPluginMinerUpdate = MinerPluginsManager.HasMinerUpdates();
                if (hasPluginMinerUpdate)
                {
                    loader.SecondaryTitle = Tr("Updating Miner Binaries");
                    loader.SecondaryVisible = true;

                    loader.PrimaryProgress.Report((Tr("Updating Miner Binaries..."), nextProgPerc()));
                    await MinerPluginsManager.UpdateMinersBins(loader.SecondaryProgress, ExitApplication.Token);
                    loader.SecondaryVisible = false;
                    if (ExitApplication.IsCancellationRequested) return;
                }


                // re-check after download we should have all miner files
                var missingMinerBins = MinerPluginsManager.GetMissingMiners().Count > 0;
                if (missingMinerBins)
                {
                    var result = MessageBox.Show(Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. {0} might not work properly without missing files. Click Yes to reinitialize {0} to try to fix this issue.", NHMProductInfo.Name),
                        Tr("Warning!"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        RestartProgram();
                        return;
                    }
                }

                // STEP
                // VC_REDIST check
                loader.PrimaryProgress.Report((Tr("Checking VC_REDIST..."), nextProgPerc()));
                VC_REDIST_x64_2015_DEPENDENCY_PLUGIN.Instance.InstallVcRedist();

                // STEP
                if (FirewallRules.RunFirewallRulesOnStartup)
                {
                    loader.PrimaryProgress.Report((Tr("Checking Firewall Rules..."), nextProgPerc()));
                    if (FirewallRules.IsFirewallRulesOutdated())
                    {
                        // requires UAC
                        // TODO show message box
                        FirewallRules.UpdateFirewallRules();
                    }
                }
                else
                {
                    loader.PrimaryProgress.Report((Tr("Skipping Firewall Rules..."), nextProgPerc()));
                }

                // STEP
                // Cross reference plugin indexes
                loader.PrimaryProgress.Report((Tr("Cross referencing miner device IDs..."), nextProgPerc()));
                // Detected devices cross reference with miner indexes
                await MinerPluginsManager.DevicesCrossReferenceIDsWithMinerIndexes();
            }
            catch (Exception e)
            {
                Logger.Error("ApplicationStateManager.Init", $"Exception: {e.Message}");
            }
            finally
            {
                isInitFinished = true;
                NiceHashStats.NotifyStateChangedTask();
            }
        }

        // make these non modal
        //public static void ShowQueryWarnings()
        //{
        //    if (query.FailedRamCheck)
        //    {
        //        MessageBox.Show(Tr("{0} recommends increasing virtual memory size so that all algorithms would work fine.", NHMProductInfo.Name),
        //            Tr("Warning!"),
        //            MessageBoxButtons.OK);
        //    }

        //    if (query.FailedVidControllerStatus)
        //    {
        //        var msg = Tr("We have detected a Video Controller that is not working properly. {0} will not be able to use this Video Controller for mining. We advise you to restart your computer, or reinstall your Video Controller drivers.", NHMProductInfo.Name);
        //        msg += '\n' + query.FailedVidControllerInfo;
        //        MessageBox.Show(msg,
        //            Tr("Warning! Video Controller not operating correctly"),
        //            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //}
    }
}

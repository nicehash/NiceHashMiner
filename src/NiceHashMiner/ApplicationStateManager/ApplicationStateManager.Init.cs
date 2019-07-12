// SHARED
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Forms.Components;
using NiceHashMiner.Miners.IntegratedPlugins;
using NiceHashMiner.Plugin;
using NiceHashMiner.Stats;
using NiceHashMiner.Utils;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NHM.DeviceDetection;

using static NiceHashMiner.Translations;
using NHM.DeviceMonitoring;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        private static bool isInitFinished = false;

        public static bool FailedRamCheck { get; internal set; }

        public static async Task InitializeManagersAndMiners(StartupLoadingControl loadingControl, IProgress<(string loadMessageText, int prog)> progress, IProgress<(string loadMessageText, int prog)> progressDownload)
        {
            try
            {
                var allSteps = 14;
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
                progress?.Report((Tr("Checking System Specs"), nextProgPerc()));
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
                    progress?.Report((msg, nextProgPerc()));
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
#if TESTNET || TESTNETDEV || PRODUCTION_NEW
                DeviceMonitorManager.DisableDevicePowerModeSettings = ConfigManager.GeneralConfig.DisableDevicePowerModeSettings;
#else
                DeviceMonitorManager.DisableDevicePowerModeSettings = true;
                ConfigManager.GeneralConfig.DisableDevicePowerModeSettings = true;
#endif
                progress?.Report((Tr("Initializing device monitoring"), nextProgPerc()));
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
                progress?.Report((Tr("Loading miner plugins..."), nextProgPerc()));
                // Plugin Loading
                MinerPluginsManager.LoadMinerPlugins();
                // commit again benchmarks after loading plugins
                ConfigManager.CommitBenchmarks();
                /////////////////////////////////////////////
                /////// from here on we have our devices and Miners initialized
                UpdateDevicesStatesAndStartDeviceRefreshTimer();

                // STEP
                // connect to nhmws
                progress?.Report((Tr("Connecting to nhmws..."), nextProgPerc()));
                // Init ws connection
                NiceHashStats.StartConnection(Nhmws.NhmSocketAddress);

                // STEP
                // disable windows error reporting
                progress?.Report((Tr("Setting Windows error reporting..."), nextProgPerc()));
                Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);

                // STEP
                // Nvidia p0
                progress?.Report((Tr("Changing all supported NVIDIA GPUs to P0 state..."), nextProgPerc()));
                if (ConfigManager.GeneralConfig.NVIDIAP0State && AvailableDevices.HasNvidia)
                {
                    Helpers.SetNvidiaP0State();
                }

                // STEP
                // Downloading integrated plugins bins, TODO put this in some internals settings
                var hasMissingMinerBins = MinerPluginsManager.GetMissingMiners().Count > 0;
                if (hasMissingMinerBins)
                {
                    loadingControl.LoadTitleTextSecond = Tr("Downloading Miner Binaries");
                    loadingControl.ShowSecondProgressBar = true;

                    progress?.Report((Tr("Downloading Miner Binaries..."), nextProgPerc()));
                    await MinerPluginsManager.DownloadMissingIntegratedMinersBins(progressDownload, ExitApplication.Token);
                    //await MinersDownloader.MinersDownloadManager.DownloadAndExtractOpenSourceMinersWithMyDownloaderAsync(progressDownload, ExitApplication.Token);
                    loadingControl.ShowSecondProgressBar = false;
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
                progress?.Report((Tr("Checking VC_REDIST..."), nextProgPerc()));
                VC_REDIST_x64_2015_DEPENDENCY_PLUGIN.Instance.InstallVcRedist();

                // STEP
                if (FirewallRules.RunFirewallRulesOnStartup)
                {
                    progress?.Report((Tr("Checking Firewall Rules..."), nextProgPerc()));
                    if (FirewallRules.IsFirewallRulesOutdated())
                    {
                        // requires UAC
                        // TODO show message box
                        FirewallRules.UpdateFirewallRules();
                    }
                }
                else
                {
                    progress?.Report((Tr("Skipping Firewall Rules..."), nextProgPerc()));
                }

                // STEP
                // Cross reference plugin indexes
                progress?.Report((Tr("Cross referencing miner device IDs..."), nextProgPerc()));
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
                NiceHashStats.StateChanged();
#if !(TESTNET || TESTNETDEV || PRODUCTION_NEW)
                ResetNiceHashStatsCredentials();
#endif
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

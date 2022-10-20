using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.DeviceDetection;
using NHM.DeviceMonitoring;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Mining.Plugins;
using NHMCore.Nhmws;
using NHMCore.Notifications;
using NHMCore.Schedules;
using NHMCore.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
using static NHMCore.Translations;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        private static bool isInitFinished = false;

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
                Launcher.SetIsUpdated(Environment.GetCommandLineArgs().Contains("-updated"));
                Launcher.SetIsUpdatedFailed(Environment.GetCommandLineArgs().Contains("-updateFailed"));

                var allSteps = 14;
                var currentStep = 0;
                int nextProgPerc()
                {
                    ++currentStep;
                    var perc = (int)(((double)currentStep / allSteps) * 100);
                    if (perc > 100) return 100;
                    return perc;
                };
                // STEP
                // Checking System Memory
                loader.PrimaryProgress?.Report((Tr("Checking System Specs"), nextProgPerc()));
                await Task.Run(() => SystemSpecs.QueryWin32_OperatingSystemDataAndLog());
                await WindowsUptimeCheck.DelayUptime();

                // TODO extract in function
                #region Device Detection

                // STEP +3
                string detectionStepMessage(DeviceDetectionStep step)
                {
                    return step switch
                    {
                        DeviceDetectionStep.CPU => Tr("Checking CPU Info"),
                        DeviceDetectionStep.NVIDIA_CUDA => Tr("Querying CUDA devices"),
                        DeviceDetectionStep.AMD_OpenCL => Tr("Checking AMD OpenCL GPUs"),
                        _ => Tr("Checking Windows Video Controllers"), //DeviceDetectionStep.WMIWMIVideoControllers
                    };
                };
                var devDetectionProgress = new Progress<DeviceDetectionStep>(step =>
                {
                    var msg = detectionStepMessage(step);
                    loader.PrimaryProgress?.Report((msg, nextProgPerc()));
                });
                await DeviceDetection.DetectDevices(devDetectionProgress);
                if(DeviceDetection.DetectionResult.CUDADevices.Any(dev => dev.IsLHR) && !Helpers.IsElevated && CUDADevice.INSTALLED_NVIDIA_DRIVERS < new Version(522, 25))
                {
                    AvailableNotifications.CreateLHRPresentAdminRunRequired();
                }

                if (!DeviceMonitorManager.IsMotherboardCompatible() && Helpers.IsElevated)
                {
                    AvailableNotifications.CreateMotherboardNotCompatible();
                }
                OutsideProcessMonitor.Init(ExitApplication.Token);
                // add devices
                string getDeviceNameCount(DeviceType deviceType, int index) => 
                    deviceType switch
                    {
                        DeviceType.CPU => $"CPU#{index}",
                        DeviceType.AMD => $"AMD#{index}",
                        DeviceType.NVIDIA => $"GPU#{index}",
                        _ => $"UNKNOWN#{index}",
                    };

                ComputeDevice newComputeDevice(BaseDevice d, int i) => new ComputeDevice(d, getDeviceNameCount(d.DeviceType, i + 1));
                var detectionResult = DeviceDetection.DetectionResult;
                var groupedComputeDevices = DeviceDetection.GetDetectedDevices()
                    .GroupBy(dev => dev.DeviceType)
                    .SelectMany(group => group.Select(newComputeDevice));
                foreach (var cDev in groupedComputeDevices) AvailableDevices.AddDevice(cDev);

                AvailableDevices.UncheckCpuIfGpu();

                var ramCheckOK = SystemSpecs.CheckRam(AvailableDevices.AvailGpus, AvailableDevices.AvailNvidiaGpuRam, AvailableDevices.AvailAmdGpuRam);
                if (!ramCheckOK)
                {
                    AvailableNotifications.CreateIncreaseVirtualMemoryInfo();
                }
                if (AvailableDevices.HasNvidia && (DeviceDetection.DetectionResult.IsNvidiaNVMLLoadedError || DeviceDetection.DetectionResult.IsNvidiaNVMLInitializedError))
                {
                    AvailableNotifications.CreateFailedNVMLLoadInitInfo();
                }
                // no compatible devices? exit
                if (AvailableDevices.Devices.Count == 0)
                {
                    NoDeviceAction?.Invoke();
                    return;
                }
#warning CPU monitoring detection not fully functional
                //// no compatible CPU
                //if (!DeviceMonitorManager.IsMotherboardCompatible())
                //{
                //    AvailableNotifications.CreateMotherboardNotCompatible();
                //}
                //else if(!Helpers.IsElevated) // MOBO is supported but we lack admin privs
                //{
                //    AvailableNotifications.CreateAdminRunRequired();
                //}



                // STEP
                loader.PrimaryProgress?.Report((Tr("Initializing device monitoring"), nextProgPerc()));
                var monitors = await DeviceMonitorManager.GetDeviceMonitors(AvailableDevices.Devices.Select(d => d.BaseDevice));
                foreach (var monitor in monitors)
                {
                    var dev = AvailableDevices.GetDeviceWithUuid(monitor.UUID);
                    dev.SetDeviceMonitor(monitor);
                }
                // now init device settings
                ConfigManager.InitDeviceSettings();

                if (!Helpers.IsElevated && !GlobalDeviceSettings.Instance.DisableDevicePowerModeSettings && AvailableDevices.HasNvidia)
                {
                    AvailableNotifications.CreateDeviceMonitoringNvidiaElevateInfo();
                }
                //// TODO add check and only show if not enabled
                //if (AvailableDevices.HasCpu)
                //{
                //    AvailableNotifications.CreateEnableLargePagesInfo();
                //}
                //// TODO add check and only show if not enabled
                //if (AvailableDevices.HasAmd)
                //{
                //    if (detectionResult.AMDDevices.Any(amd => amd.InfSection.ToLower().Contains("polaris")))
                //    {
                //        AvailableNotifications.CreateEnableComputeModeAMDInfo();
                //    }
                //}
                #endregion Device Detection
                // STEP
                // load plugins
                loader.PrimaryProgress?.Report((Tr("Loading miner plugins..."), nextProgPerc()));
                // Plugin Loading
                await MinerPluginsManager.LoadAndInitMinerPlugins();
                MinerPluginsManager.StartLoops(ExitApplication.Token);
                // commit again benchmarks after loading plugins
                ConfigManager.CommitBenchmarks();
                /////////////////////////////////////////////
                /////// from here on we have our devices and Miners initialized
                MiningState.Instance.CalculateDevicesStateChange();

                // STEP
                // connect to nhmws
                loader.PrimaryProgress?.Report((Tr("Connecting to nhmws..."), nextProgPerc()));
                // Init ws connection
                var (btc, worker, group) = CredentialsSettings.Instance.GetCredentials();
                NHWebSocket.SetCredentials(btc, worker, group);
                NHWebSocket.StartLoop(ExitApplication.Token);


                // STEP
                // disable windows error reporting
                loader.PrimaryProgress?.Report((Tr("Setting Windows error reporting..."), nextProgPerc()));
                Helpers.DisableWindowsErrorReporting(WarningSettings.Instance.DisableWindowsErrorReporting);

                // STEP
                // Update miner plugin binaries
                loader.SecondaryTitle = Tr("Updating Miner Binaries");
                loader.SecondaryVisible = true;
                loader.PrimaryProgress?.Report((Tr("Updating Miner Binaries..."), nextProgPerc()));
                await MinerPluginsManager.UpdateMinersBins(loader.SecondaryProgress, ExitApplication.Token);
                loader.SecondaryVisible = false;
                if (ExitApplication.IsCancellationRequested) return;

                // STEP
                loader.SecondaryTitle = Tr("Downloading Miner Binaries");
                loader.SecondaryVisible = true;
                loader.PrimaryProgress?.Report((Tr("Downloading Miner Binaries..."), nextProgPerc()));
                await MinerPluginsManager.DownloadMissingMinersBins(loader.SecondaryProgress, ExitApplication.Token);
                loader.SecondaryVisible = false;
                if (ExitApplication.IsCancellationRequested) return;

                //var shouldAutoIncreaseVRAM = Registry.CurrentUser.GetValue(@"Software\" + APP_GUID.GUID + @"\AutoIncreaseVRAM", false);
                //if (shouldAutoIncreaseVRAM == null)
                //{
                //    AvailableNotifications.CreateIncreaseVirtualMemoryInfo();
                //} else
                //{
                //    if ((bool)shouldAutoIncreaseVRAM == true)
                //    {
                //        var vramSum = AvailableDevices.AvailNvidiaGpuRam + AvailableDevices.AvailAmdGpuRam;
                //    }
                //}

                // fire up mining manager loop
                var username = CredentialValidators.ValidateBitcoinAddress(btc) ? CreateUsername(btc, RigID()) : DemoUser.BTC;
                MiningManager.StartLoops(ExitApplication.Token, username);

                // STEP
                // VC_REDIST check
                loader.PrimaryProgress?.Report((Tr("Checking VC_REDIST..."), nextProgPerc()));
                await VC_REDIST_x64_2015_2019_Manager.Instance.InitVCRedist(loader.SecondaryProgress, ExitApplication.Token);
                // STEP
                // Cross reference plugin indexes 
                loader.PrimaryProgress?.Report((Tr("Cross referencing miner device IDs..."), nextProgPerc()));
                // Detected devices cross reference with miner indexes
                await MinerPluginsManager.DevicesCrossReferenceIDsWithMinerIndexes(loader);

                if (AvailableDevices.HasGpuToPause)
                {
                    var deviceToPauseUuid = AvailableDevices.Devices.FirstOrDefault(dev => dev.PauseMiningWhenGamingMode && dev.DeviceType != DeviceType.CPU).Uuid;
                    MiningSettings.Instance.DeviceIndex = AvailableDevices.GetDeviceIndexFromUuid(deviceToPauseUuid);
                }
                else if (MiningSettings.Instance.DeviceToPauseUuid != "")
                {
                    MiningSettings.Instance.DeviceIndex = AvailableDevices.GetDeviceIndexFromUuid(MiningSettings.Instance.DeviceToPauseUuid);
                    AvailableDevices.GPUs.FirstOrDefault(dev => dev.Uuid == MiningSettings.Instance.DeviceToPauseUuid).PauseMiningWhenGamingMode = true;
                }
                else if (AvailableDevices.HasGpu)
                {
                    MiningSettings.Instance.DeviceIndex = 0;
                    AvailableDevices.GPUs.FirstOrDefault().PauseMiningWhenGamingMode = true;
                }
                GPUProfileManager.Instance.Init();
                if (GPUProfileManager.Instance.SystemContainsSupportedDevicesNotSystemElevated)
                {
                    if (MiscSettings.Instance.UseOptimizationProfiles) AvailableNotifications.CreateOptimizationProfileElevateInfo();
                    else AvailableNotifications.CreateOptimizationProfileNotEnabledInfo();
                }

                SchedulesManager.Instance.Init();
            }
            catch (Exception e)
            {
                Logger.Error("ApplicationStateManager.Init", $"Exception: {e.Message}");
            }
            finally
            {
                isInitFinished = true;
                NHWebSocket.NotifyStateChanged();

                // start update checker
                // updater loops after we finish
                UpdateHelpers.StartLoops(ExitApplication.Token);
                // restore last mining states
                await RestoreMiningState();
            }
        }
    }
}

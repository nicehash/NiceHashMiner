using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHMCore.Mining;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using static NHMCore.Translations;

namespace NHMCore.Notifications
{
    public static class AvailableNotifications
    {
        public static void CreateDeviceMonitoringNvidiaElevateInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.MonitoringNvidiaElevate, Tr("NVIDIA TDP Settings Insufficient Permissions"), Tr("Disabled NVIDIA power mode settings due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
            notification.Action = AvailableActions.ActionRunAsAdmin();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.MonitoringNvidiaElevate);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateOptimizationProfileElevateInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.OptimizationProfilesElevate, Tr("Optimization profiles Insufficient Permissions"), Tr("Can't run optimization profiles due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
            notification.Action = AvailableActions.ActionRunAsAdmin();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.OptimizationProfilesElevate);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateOptimizationProfileNotEnabledInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.OptimizationWithProfilesDisabled, Tr("Optimization profiles not enabled"), Tr("Optimization profiles are not enabled. Enable for optimization of some GPUs for a bigger hash rate. Run NiceHash Miner as an Administrator and enable Optimization profiles in advanced settings."));
            notification.Action = AvailableActions.ActionRunAsAdmin();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.OptimizationWithProfilesDisabled);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateConnectionLostInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.ConnectionLost, Tr("Check internet connection"), Tr("NiceHash Miner requires internet connection to run. Please ensure that you are connected to the internet before running NiceHash Miner."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.ConnectionLost);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoEnabledDeviceInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.NoEnabledDevice, Tr("No enabled devices"), Tr("NiceHash Miner cannot start mining. Make sure you have at least one enabled device that has at least one enabled and benchmarked algorithm."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NoEnabledDevice);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateDemoMiningInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.DemoMining, Tr("Demo mode mining"), Tr("You have not entered a mining address. NiceHash Miner will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer.\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!"));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.DemoMining);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoSmaInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.NoSma, Tr("Unable to get profitability data"), Tr("Unable to get NiceHash profitability data. If you are connected to internet, try again later."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NoSma);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Not used anymore, we might add it back in the future")]
        public static void CreateNoDeviceSelectedBenchmarkInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.NoDeviceSelectedBenchmark, Tr("No device selected to benchmark"), Tr("No device has been selected, there is nothing to benchmark."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NoDeviceSelectedBenchmark);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Not used anymore, we might add it back in the future")]
        public static void CreateNothingToBenchmarkInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NothingToBenchmark, Tr("Nothing to benchmark"), Tr("Current benchmark settings are already executed. There is nothing to do."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NothingToBenchmark);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoSupportedDevicesInfo()
        {
            var notification = new Notification(NotificationsType.Fatal, NotificationsGroup.NoSupportedDevices, Tr("No Supported Devices"), Tr("No supported devices are found."));
            notification.Action = AvailableActions.ActionNHMNoDevHelp();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NoSupportedDevices);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMissingGPUsInfo()
        {
            var notification = new Notification(
                NotificationsType.Warning,
                NotificationsGroup.MissingGPUs,
                Tr("Missing GPUs"),
                Tr("There are missing GPUs from inital NiceHash Miner startup. This is usually caused by driver crashes and usually requires system restart to recover."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.MissingGPUs);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNhmUpdateInfoDownload(bool isInstallerVersion)
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NhmUpdate, Tr("NiceHash Miner Update"), Tr("New version of NiceHash Miner is available."));
            if (Configs.UpdateSettings.Instance.AutoUpdateNiceHashMiner)
            {
                notification.Action = AvailableActions.ActionDownloadUpdater(isInstallerVersion, notification);
            }

            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NhmUpdate);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNhmUpdateInfoUpdate()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NhmUpdate, Tr("NiceHash Miner Update"), Tr("New version of NiceHash Miner is available."));
            if (Configs.UpdateSettings.Instance.AutoUpdateNiceHashMiner)
            {
                notification.Action = AvailableActions.ActionStartUpdater();
            }

            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NhmUpdate);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNhmUpdateAttemptFail()
        {
            var notificationIfUnsuccessfull = new Notification(NotificationsType.Warning, NotificationsGroup.NhmUpdateFailed, Tr("NiceHash Miner Update Failed"), Tr("Update procedure failed please install manually. Please make sure that the file is accessible and that your anti-virus is not blocking the application. NiceHash Miner might not work properly without missing files. Please check the following blog post: {0}", Links.AVHelp));
            notificationIfUnsuccessfull.Action = AvailableActions.ActionVisitReleasePage();
            notificationIfUnsuccessfull.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NhmUpdateFailed);
            NotificationsManager.Instance.AddNotificationToList(notificationIfUnsuccessfull);
            var notification = NotificationsManager.Instance.Notifications.FirstOrDefault(n => n.Group == NotificationsGroup.NhmUpdate);
            if (notification != null) NotificationsManager.Instance.RemoveNotificationFromList(notification);
        }

        public static void CreateNhmWasUpdatedInfo(bool success)
        {
            var sentence = success ? "was updated" : "was not updated";
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NhmWasUpdated, Tr("NiceHash Miner was updated"), Tr($"NiceHash Miner {sentence} to the latest version."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NhmWasUpdated);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreatePluginUpdateInfo(string pluginName, bool success)
        {
            var sentence = success ? "was installed" : "was not installed";
            var content = Tr("New version of {0} {1}.", pluginName, Tr(sentence));
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.PluginUpdate, Tr("Miner Plugin Update"), content);
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.PluginUpdate);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMissingMinerBinsInfo(string pluginName)
        {
            EventManager.Instance.AddEventMissingFiles(true);
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.MissingMinerBins, Tr("Missing miner binaries"), Tr("Some of the {0} binaries are missing from the installation folder. Please make sure that the files are accessible and that your anti-virus is not blocking the application.", pluginName));
            notification.Action = AvailableActions.ActionVisitAVHelp();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.MissingMinerBins);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEnableLargePagesInfo()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.LargePages, Tr("Enable large pages for randomx"), Tr("Would you like to enable large pages when mining with RandomX(CPU)?"));
            notification.Action = AvailableActions.ActionLargePagesHelp();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.LargePages);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateIncreaseVirtualMemoryInfo()
        {
            EventManager.Instance.AddEventVirtualMemInsufficient(true);
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.VirtualMemory, Tr("Increase virtual memory"), Tr("NiceHash Miner recommends increasing virtual memory size so that all algorithms would work fine. Would you like to increase virtual memory?"));
            notification.Action = AvailableActions.ActionVisitMemoryHelp();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.VirtualMemory);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedBenchmarksInfo(ComputeDevice device, string algo, string plugin)
        {
            EventManager.Instance.AddEventBenchmarkFailed(plugin, algo, device.Name, device.B64Uuid, true);
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.FailedBenchmarks, Tr("Failed benchmarks"), Tr("Some benchmarks for {0} failed to execute. Check benchmark tab for more info.", device.Name));
            notification.Action = AvailableActions.ActionFailedBenchmarksHelp();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.FailedBenchmarks);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNotProfitableInfo(bool shouldClear)
        {
            if (!shouldClear)
            {
                var notification = new Notification(NotificationsType.Warning, NotificationsGroup.Profit, Tr("Mining not profitable"), Tr("Currently mining is not profitable. Mining will be resumed once it will be profitable again."));
                notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.Profit);
                NotificationsManager.Instance.AddNotificationToList(notification);
            }
        }

        public static void CreateNoAvailableAlgorithmsInfo(int deviceId, string deviceName)
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.NoAvailableAlgorithms, Tr("No available algorithms"), Tr("There are no available algorithms to mine with GPU #{0} {1}. Please check you rig stability and stability of installed plugins.", deviceId.ToString(), deviceName));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NoAvailableAlgorithms);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateLogUploadResultInfo(bool success, string uuid)
        {
            //clean previous
            var logUploadNotifications = NotificationsManager.Instance.Notifications.Where(notif => notif.Group == NotificationsGroup.LogArchiveUpload).FirstOrDefault();
            if (logUploadNotifications != null) logUploadNotifications.RemoveNotification();

            var sentence = success ? Tr("was uploaded.") : Tr("was not uploaded. Please contact our support team for help.");
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.LogArchiveUpload, Tr("Log archive upload result"), Tr("The log archive with the following ID: {0}", uuid) + " " + sentence);
            notification.Action = AvailableActions.ActionCopyToClipBoard(uuid);
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.LogArchiveUpload);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedNVMLLoadInitInfo()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.NVMLLoadInitFail, Tr("Failed NVML Load/Initialization"), Tr("NVML was not initialized. Try to reinstall drivers. Also you could try to restart Windows."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NVMLLoadInitFail);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedDownloadWrongHashBinary(string pluginName)
        {
            var content = Tr("The downloaded {0} checksum does not meet our security verification. Please make sure that you are downloading the source from a trustworthy source.", pluginName);
            var notification = new Notification(NotificationsType.Error, pluginName, NotificationsGroup.WrongChecksumBinary, Tr("Checksum validation failed"), content);
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.WrongChecksumBinary);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedDownloadWrongHashDll(string pluginName)
        {
            var content = Tr("The used {0} plugin .dll checksum does not meet our security verification. Please make sure that you are using an official .dll.", pluginName);
            var notification = new Notification(NotificationsType.Error, pluginName, NotificationsGroup.WrongChecksumDll, Tr("Checksum validation failed dll"), content);
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.WrongChecksumDll);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateRestartedMinerInfo(DateTime dateTime, string minerName)
        {
            var content = Tr("Miner \"{0}\" was restarted at {1}", minerName, dateTime.ToString("HH:mm:ss MM/dd/yyyy"));
            var notification = new Notification(NotificationsType.Info, minerName, NotificationsGroup.MinerRestart, Tr("Miner restarted"), Tr(content));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.MinerRestart);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNullChecksumError(string pluginName)
        {
            var content = Tr("Unable to download file for {0}, check your antivirus.", pluginName);
            var notification = new Notification(NotificationsType.Error, pluginName, NotificationsGroup.NullChecksum, Tr("Checksum validation null"), content);
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NullChecksum);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateOutdatedDriverWarningForPlugin(string pluginName, string pluginUUID, List<(DriverVersionLimitType outDatedType, BaseDevice dev, (DriverVersionCheckType checkReturnCode, Version minVersion) driverCheckReturn)> listOfOldDrivers)
        {
            string name = Tr("Detected older driver versions") + " (" + pluginName + ")";
            string content = Tr("Older driver versions have been detected on this system, and they may cause problems with {0}. Please update them.", pluginName) + "\n";

            var criticals = listOfOldDrivers.Where(dev => dev.outDatedType == DriverVersionLimitType.MinRequired);
            var recommends = listOfOldDrivers.Where(dev => dev.outDatedType == DriverVersionLimitType.MinRecommended && !criticals.Any(dev1 => dev1.Item2 == dev.Item2));

            if (recommends.Any())
            {
                content += Tr("Lower than recommended") + ":\n";
                var nvidias = recommends.Where(dev => dev.Item2.DeviceType == DeviceType.NVIDIA);
                var amds = recommends.Where(dev => dev.Item2.DeviceType == DeviceType.AMD);
                if (nvidias.Any()) content += "\tNvidia: at least " + nvidias.FirstOrDefault().driverCheckReturn.minVersion + "\n";
                if (amds.Any())
                {
                    content += "\tAMD: (adrenalin)\n";
                    foreach (var amd in amds) content += "\t\t" + amd.Item2.Name + ": at least " + amd.driverCheckReturn.minVersion + "\n";
                }
            }
            if (criticals.Any())
            {
                content += Tr("Lower than required") + ":\n";
                var nvidias = criticals.Where(dev => dev.Item2.DeviceType == DeviceType.NVIDIA);
                var amds = criticals.Where(dev => dev.Item2.DeviceType == DeviceType.AMD);
                if (nvidias.Any()) content += "\tNvidia: at least " + nvidias.FirstOrDefault().driverCheckReturn.minVersion + "\n";
                if (amds.Any())
                {
                    content += "\tAMD: (adrenalin)\n";
                    foreach (var amd in amds) content += "\t\t" + amd.Item2.Name + ": at least " + amd.driverCheckReturn.minVersion + "\n";
                }
            }
            var notification = new Notification(NotificationsType.Warning, pluginName, NotificationsGroup.DriverVersionProblem, Tr(name), Tr(content));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.DriverVersionProblem);
            NotificationsManager.Instance.AddNotificationToList(notification);
            Logger.Warn(pluginName, content);
        }

        public static void CreateADLVersionWarning(AMDDevice amdDev)
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.DriverVersionProblem, Tr("ADL driver version retrieval warning ({0})", amdDev.ADLReturnCode), Tr("Driver string could not be correctly retrieved from the system - version may be incorrect (\"{0}\")", amdDev.RawDriverVersion));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.DriverVersionProblem);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateADLVersionError(AMDDevice amdDev)
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.DriverVersionProblem, Tr("ADL driver version retrieval failed ({0})", amdDev.ADLReturnCode), Tr("ADL failed to retrieve the driver version. Please update your AMD drivers."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.DriverVersionProblem);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateAdminRunRequired()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.AdminRunRequired, Tr("NiceHash Miner can't obtain CPU information"), Tr("Start NiceHash Miner with administrator rights."));
            notification.Action = AvailableActions.ActionRunAsAdmin();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.AdminRunRequired);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMotherboardNotCompatible()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.MotherboardNotCompatible, Tr("NiceHash Miner can’t monitor CPU"), Tr("Your motherboard is not reporting fan speed, temperature, or the load of the CPU. Most likely your CPU is not compatible with the NHM CPU monitoring tool."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.MotherboardNotCompatible);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateLHRPresentAdminRunRequired()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.RequireAdminForLHR, Tr("LHR Insufficient Permissions"), Tr("At least one LHR GPU was detected on your rig. To achieve full LHR unlock you need to run NMH as Administrator."));
            notification.Action = AvailableActions.ActionRunAsAdmin();
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.RequireAdminForLHR);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoPowerInfo(string pluginName, string algorithmName, string deviceName)
        {
            var content = Tr("Try reinstalling the GPU drivers or run NHM as administrator.\n\n{0}/{1} power usage for {2} is missing.\n", pluginName, algorithmName, deviceName);
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NoPowerInfo, Tr("Some benchmarks failed to save power usage"), content);
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.DriverVersionProblem);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoOptimalDrivers(Version v)
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.NoOptimalDrivers, Tr("NVIDIA drivers used are not optimal for mining"), Tr($"NVIDIA drivers version {v} are known to affect mining stability negatively."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.NoOptimalDrivers);
            notification.Action = AvailableActions.ActionNoOptimalDrivers();
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNotAdminForRigManagement()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.RigManagementElevate, Tr("NHM needs administrator privileges for rig management"), Tr($"If you want to use rig manager for OC/Fan/command settings, you must run NHM as an administrator"));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.RigManagementElevate);
            notification.Action = AvailableActions.ActionRunAsAdmin();
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMemClockRangeGetFailed(int busID, string gpuName)
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.MemClockRangeGetFail, Tr("NHM failed to retrieve memory clock range"), Tr($"NHM failed to retrieve memory clock range. Default range will be used on [{busID}]:{gpuName}."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.MemClockRangeGetFail);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateCoreClockRangeGetFailed(int busID, string gpuName)
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.CoreClockRangeGetFail, Tr("NHM failed to retrieve core clock range"), Tr($"NHM failed to retrieve core clock range. Default range will be used on [{busID}]:{gpuName}."));
            notification.NotificationUUID = Enum.GetName(typeof(NotificationsGroup), NotificationsGroup.CoreClockRangeGetFail);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }
    }
}

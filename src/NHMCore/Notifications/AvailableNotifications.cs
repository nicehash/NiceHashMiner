using NHMCore.Mining;
using NHMCore.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static NHMCore.Translations;

namespace NHMCore.Notifications
{
    public static class AvailableNotifications
    {
        public static void CreateDeviceMonitoringNvidiaElevateInfo()
        {
            var notification = new Notification(NotificationsType.Info, Tr("NVIDIA TDP Settings Insufficient Permissions"), Tr("Disabled NVIDIA power mode settings due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Run As Administrator",
                Action = () => { RunAsAdmin.SelfElevate(); }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEthlargementElevateInfo()
        {
            var notification = new Notification(NotificationsType.Info, Tr("Ethlargement-Pill Settings Insufficient Permissions"), Tr("Run Ethlargement settings due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Run As Administrator",
                Action = () => { RunAsAdmin.SelfElevate(); }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateConnectionLostInfo()
        {
            var notification = new Notification(NotificationsType.Error, Tr("Check internet connection"), Tr("NiceHash Miner requires internet connection to run. Please ensure that you are connected to the internet before running NiceHash Miner."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoEnabledDeviceInfo()
        {
            var notification = new Notification(NotificationsType.Error, Tr("No enabled devices"), Tr("NiceHash Miner cannot start mining. Make sure you have at least one enabled device that has at least one enabled and benchmarked algorithm."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateDemoMiningInfo()
        {
            var notification = new Notification(NotificationsType.Info, Tr("Demo mode mining"), Tr("You have not entered a bitcoin address. NiceHash Miner will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer.\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!"));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoSmaInfo()
        {
            var notification = new Notification(NotificationsType.Error, Tr("Unable to get profitability data"), Tr("Unable to get NiceHash profitability data. If you are connected to internet, try again later."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Not used anymore, we might add it back in the future")]
        public static void CreateNoDeviceSelectedBenchmarkInfo()
        {
            var notification = new Notification(NotificationsType.Error, Tr("No device selected to benchmark"), Tr("No device has been selected, there is nothing to benchmark."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Not used anymore, we might add it back in the future")]
        public static void CreateNothingToBenchmarkInfo()
        {
            var notification = new Notification(NotificationsType.Info, Tr("Nothing to benchmark"), Tr("Current benchmark settings are already executed. There is nothing to do."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoSupportedDevicesInfo()
        {
            var notification = new Notification(NotificationsType.Fatal, Tr("No Supported Devices"), Tr("No supported devices are found."));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Help",
                Action = () => {Process.Start(Links.NhmNoDevHelp); }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMissingMinersInfo()
        {
            var notification = new Notification(NotificationsType.Error, Tr("Missing miner files"), Tr($"There are missing files from last Miners Initialization. Please make sure that the file is accessible and that your anti-virus is not blocking the application. NiceHash Miner might not work properly without missing files. Please check the following blog post: {Links.AVHelp}"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Restart NiceHash Miner",
                Action = () => { ApplicationStateManager.RestartProgram(); }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Not used anymore, we might add it back in the future")]
        public static void CreateFailedVideoControllerInfo()
        {
            var notification = new Notification(NotificationsType.Info, Tr("Video Controller not operating correctly"), Tr("We have detected a Video Controller that is not working properly. NiceHash Miner will not be able to use this Video Controller for mining. We advise you to restart your computer, or reinstall your Video Controller drivers."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Enabling WMI is mandatory and is checked in the 'ApplicationStateManager.Program.SystemRequirementsEnsured()'")]
        public static void CreateIsWmiEnabledInfo()
        {
            var notification = new Notification(NotificationsType.Fatal, Tr("Windows Management Instrumentation Error"), Tr("NiceHash Miner cannot run needed components. It seems that your system has Windows Management Instrumentation service Disabled. In order for NiceHash Miner to work properly Windows Management Instrumentation service needs to be Enabled. This service is needed to detect RAM usage and Avaliable Video controler information. Enable Windows Management Instrumentation service manually and start NiceHash Miner."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Mandatory is checked in the 'ApplicationStateManager.Program.SystemRequirementsEnsured()'")]
        public static void CreateIsNet45Info()
        {
            var notification = new Notification(NotificationsType.Fatal, Tr(".Net Framework Error"), Tr("NiceHash Miner requires .NET Framework 4.5 or higher to work properly. Please install Microsoft .NET Framework 4.5."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Mandatory is checked in the 'ApplicationStateManager.Program.SystemRequirementsEnsured()'")]
        public static void CreateIs64BitOSInfo()
        {
            var notification = new Notification(NotificationsType.Fatal, Tr("Operating System Error"), Tr("NiceHash Miner supports only x64 platforms. You will not be able to use NiceHash Miner with x86."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNhmUpdateInfo()
        {
            var notification = new Notification(NotificationsType.Info, Tr("NiceHash Miner Update"), Tr("New version of NiceHash Miner is available."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreatePluginUpdateInfo(string pluginName, bool success)
        {
            var sentence = "was installed";
            if (!success) sentence = "was not installed";      
            var notification = new Notification(NotificationsType.Info, Tr("Miner Plugin Update"), Tr($"New version of {pluginName} " +sentence));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMissingMinerBinsInfo(string pluginName)
        {
            var notification = new Notification(NotificationsType.Info, Tr("Missing miner binaries"), Tr($"Some of the {pluginName} binaries are missing from the installation folder. Please make sure that the files are accessible and that your anti-virus is not blocking the application."));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Help",
                Action = () => { Process.Start(Links.AVHelp); }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateWarningNVIDIADCHInfo()
        {
            var notification = new Notification(NotificationsType.Warning, Tr("Nvidia DCH drivers detected"), Tr("Detected drivers are not recommended for mining with NiceHash Miner. Please change them for optimal performance."));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Help",
                Action = () => { Process.Start(Links.NvidiaDriversHelp); }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateAddWindowsDefenderExceptionInfo()
        {
            var notificationIfUnsuccessfull = new Notification(NotificationsType.Warning, Tr("Add Windows Defender Exception Failed"), Tr("Adding exception to Windows Defender failed. Please check the help page."));
            notificationIfUnsuccessfull.Actions.Add(new NotificationAction
            {
                Info = "Help",
                Action = () => { Process.Start(Links.AddWDExclusionHelp_PRODUCTION); }
            });

            var notification = new Notification(NotificationsType.Info, Tr("Add Windows Defender Exception"), Tr("Would you like to add NiceHash Miner root folder to the Windows Defender exceptions?"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Add exception",
                Action = () => {
                    var ok = WindowsDefender.AddException();
                    NotificationsManager.Instance.RemoveNotificationFromList(notification);
                    if (!ok)
                    {
                        NotificationsManager.Instance.AddNotificationToList(notificationIfUnsuccessfull);
                    }
                }
            });
            notification.NotificationUUID = "WindowsDefenderNotification";
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEnableComputeModeAMDInfo()
        {
            // TODO check/fix this functionality
            //var notification = new Notification(NotificationsType.Warning, Tr("Switch compute/graphic mode"), Tr("Would you like to switch between compute and graphic mode?"));
            //notification.Actions.Add(new NotificationAction
            //{
            //    Info = "Switch modes",
            //    Action = () => { AmdModeSwitcher.SwitchAmdComputeMode(); }
            //});
            var notification = new Notification(NotificationsType.Warning, Tr("Switch compute/graphic mode"), Tr("Would you like to switch between compute and graphic mode for optimized profit?"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Help",
                Action = () => { Process.Start(Links.AMDComputeModeHelp_PRODUCTION); }
            });
            notification.NotificationUUID = "AMDModeSwitchNotification";
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEnableLargePagesInfo()
        {
            var notification = new Notification(NotificationsType.Warning, Tr("Enable large pages for randomx"), Tr("Would you like to enable large pages when mining with RandomX(CPU)?"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Help",
                Action = () => { Process.Start(Links.LargePagesHelp); }
            });
            notification.NotificationUUID = "LargePagesNotification";
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateIncreaseVirtualMemoryInfo()
        {
            var notification = new Notification(NotificationsType.Warning, Tr("Increase virtual memory"), Tr("NiceHash Miner recommends increasing virtual memory size so that all algorithms would work fine. Would you like to increase virtual memory?"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Help",
                Action = () => { Process.Start(Links.VirtualMemoryHelp); }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedBenchmarksInfo(ComputeDevice device)
        {
            var notification = new Notification(NotificationsType.Info, Tr("Failed benchmarks"), Tr($"Some benchmarks for {device.Name} failed to execute. Check benchmark tab for more info."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateUnavailablePrimaryMarketLocationInfo()
        {
            //clear "market notifications"
            var marketNotifications = NotificationsManager.Instance.Notifications.Where(notif => notif.Group == NotificationsGroup.Market);
            foreach (var marketNotif in marketNotifications)
            {
                NotificationsManager.Instance.RemoveNotificationFromList(marketNotif);
            }

            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.Market, Tr("Primary mining location unavailable"), Tr($"Primary mining location is unavailable. Switching to fallback location."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateUnavailableAllMarketsLocationInfo()
        {
            //clear "market notifications"
            var marketNotifications = NotificationsManager.Instance.Notifications.Where(notif => notif.Group == NotificationsGroup.Market);
            foreach (var marketNotif in marketNotifications)
            {
                NotificationsManager.Instance.RemoveNotificationFromList(marketNotif);
            }

            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.Market, Tr("All mining locations unavailable"), Tr($"All mining locations are unavailable. Mining will be stopped."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNotProfitableInfo(bool shouldClear)
        {
            // clear old state
            var profitNotifications = NotificationsManager.Instance.Notifications.Where(notif => notif.Group == NotificationsGroup.Profit);
            foreach (var profitNotif in profitNotifications)
            {
                NotificationsManager.Instance.RemoveNotificationFromList(profitNotif);
            }

            if (!shouldClear)
            {
                var notification = new Notification(NotificationsType.Warning, NotificationsGroup.Profit, Tr("Mining not profitable"), Tr($"Currently mining is not profitable. Mining will be stopped."));
                NotificationsManager.Instance.AddNotificationToList(notification);
            }
        }

        public static void CreateNVMLFallbackFailInfo()
        {
            var notification = new Notification(NotificationsType.Error, Tr("Failed NVML fallback"), Tr("NiceHash Miner has detected that DCH drivers are installed and NVML fallback method has failed. Please fix your non-DCH driver install."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateOpenClFallbackInfo()
        {
            var notification = new Notification(NotificationsType.Error, Tr("Fallback of OpenCL"), Tr("Please check if AMD drivers are installed properly. If they are please remove Intel video driver."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }
    }
}

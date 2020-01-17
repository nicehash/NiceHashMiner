using NHMCore.Mining;
using NHMCore.Utils;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using static NHMCore.Translations;

namespace NHMCore.Notifications
{
    public class AvailableNotifications
    {
        public static void CreateDeviceMonitoringNvidiaElevateInfo()
        {
            var notification = new Notification(NotificationsType.Info, Tr("NVIDIA TDP Settings Insufficient Permissions"), Tr("Disabled NVIDIA power mode settings due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Run As Administrator",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Click yes if you wish to run {0} as Administrator.", NHMProductInfo.Name),
                        Tr("Run as Administrator"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                        RunAsAdmin.SelfElevate();
                }
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
                Info = "Get No Supported Devices Help",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Select the OK button for help.", NHMProductInfo.Name),
                        Tr("Help"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                        Process.Start(Links.NhmNoDevHelp);
                }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMissingMinersInfo()
        {
            var notification = new Notification(NotificationsType.Error, Tr("Missing miner files"), Tr($"There are missing files from last Miners Initialization. Please make sure that the file is accessible and that your anti-virus is not blocking the application. NiceHash Miner might not work properly without missing files. Please check the following blog post: {Links.AVHelp}"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Missing miner files help",
                Action = () =>
                {
                    var dialogResult = MessageBox.Show(Tr("Click OK to reinitialize NiceHash Miner to try to fix this issue.", NHMProductInfo.Name),
                        Tr("Reinitialize NiceHash Miner"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                        ApplicationStateManager.RestartProgram();
                }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedRamCheckInfo()
        {
            var notification = new Notification(NotificationsType.Info, Tr("Ram warning"), Tr("NiceHash Miner recommends increasing virtual memory size so that all algorithms would work fine."));
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
                Info = "Add folder to Windows Defender Help",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Select the OK button for redirect to how-to page."),
                        Tr("Redirect help"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                        Process.Start(Links.AVHelp);
                }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateWarningNVIDIADCHInfo()
        {
            var notification = new Notification(NotificationsType.Warning, Tr("Nvidia DCH drivers detected"), Tr("Detected drivers are not recommended for mining with NiceHash Miner. Please change them for optimal performance."));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Nvidia DCH drivers detected Help",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Select the OK button for redirect to drivers download page."),
                        Tr("Redirect help"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                        Process.Start(Links.NvidiaDriversHelp);
                }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateAddWindowsDefenderExceptionInfo()
        {
            var notification = new Notification(NotificationsType.Warning, Tr("Add Windows Defender Exception"), Tr("Would you like to add NiceHash Miner root folder to the Windows Defender exceptions?"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Add exception for NiceHash Miner",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Select the OK button to add exception."),
                        Tr("Add exception"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                    {
                        WindowsDefender.AddException();
                        NotificationsManager.Instance.RemoveNotificationFromList(notification);
                    }
                }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEnableComputeModeAMDInfo()
        {
            var notification = new Notification(NotificationsType.Warning, Tr("Swith compute/graphic mode"), Tr("Would you like to switch between compute and graphic mode?"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Enable AMD mode switcher",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Select the OK button to switch between modes."),
                        Tr("Switch modes"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                    {
                        AmdModeSwitcher.SwitchAmdComputeMode();
                    }
                }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEnableLargePagesInfo()
        {
            var notification = new Notification(NotificationsType.Warning, Tr("Enable large pages for randomx"), Tr("Would you like to enable large pages when mining with RandomX(CPU)?"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Enable large pages",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Select the OK button to be redirected to help page for optimized mining of RandomX."),
                        Tr("Enable large pages"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                    {
                        Process.Start(Links.LargePagesHelp);
                    }
                }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        // TODO finish CreateIncreaseVirtualMemoryInfo
        public static void CreateIncreaseVirtualMemoryInfo()
        {
            var notification = new Notification(NotificationsType.Warning, Tr("Increase virtual memory"), Tr("Would you like to automatically increase virtual memory?"));
            notification.Actions.Add(new NotificationAction
            {
                Info = "Increase virtual memory",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Select the OK button to automatically increase virtual memory."),
                        Tr("Increase virtual memory"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                    {
                        //TODO auto increase vram
                        //var key = Registry.CurrentUser.OpenSubKey(@"Software\" + APP_GUID.GUID, true);
                        //key.SetValue("AutoIncreaseVRAM", true); //set key to "true"
                    }
                }
            });
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedBenchmarksInfo(ComputeDevice device)
        {
            var notification = new Notification(NotificationsType.Info, Tr("Failed benchmarks"), Tr($"Some benchmarks for {device.Name} failed to execute. Check benchmark tab for more info."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

    }
}

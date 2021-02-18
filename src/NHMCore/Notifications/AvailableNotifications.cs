using NHM.Common;
using NHMCore.Mining;
using NHMCore.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using static NHMCore.Translations;

namespace NHMCore.Notifications
{
    public static class AvailableNotifications
    {
        public static void CreateDeviceMonitoringNvidiaElevateInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.MonitoringNvidiaElevate, Tr("NVIDIA TDP Settings Insufficient Permissions"), Tr("Disabled NVIDIA power mode settings due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
            notification.Action = new NotificationAction
            {
                Info = Tr("Run As Administrator"),
                Action = () => { RunAsAdmin.SelfElevate(); }
            };
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEthlargementElevateInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.EthlargementElevate, Tr("Ethlargement-Pill Settings Insufficient Permissions"), Tr("Can't run Ethlargement-Pill due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
            notification.Action = new NotificationAction
            {
                Info = Tr("Run As Administrator"),
                Action = () => { RunAsAdmin.SelfElevate(); }
            };
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEthlargementNotEnabledInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.EthlargementNotEnabled, Tr("EthLargement-Pill not enabled"), Tr("EthLargement-Pill is not enabled. is not running. Enable it for 50% higher hashrates. Run NiceHash Miner as an Administrator and enable Run Ethlargement in advanced settings."));
            notification.Action = new NotificationAction
            {
                Info = Tr("Run As Administrator"),
                Action = () => { RunAsAdmin.SelfElevate(); }
            };
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateConnectionLostInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.ConnectionLost, Tr("Check internet connection"), Tr("NiceHash Miner requires internet connection to run. Please ensure that you are connected to the internet before running NiceHash Miner."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoEnabledDeviceInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.NoEnabledDevice, Tr("No enabled devices"), Tr("NiceHash Miner cannot start mining. Make sure you have at least one enabled device that has at least one enabled and benchmarked algorithm."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateDemoMiningInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.DemoMining, Tr("Demo mode mining"), Tr("You have not entered a bitcoin address. NiceHash Miner will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer.\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!"));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoSmaInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.NoSma, Tr("Unable to get profitability data"), Tr("Unable to get NiceHash profitability data. If you are connected to internet, try again later."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Not used anymore, we might add it back in the future")]
        public static void CreateNoDeviceSelectedBenchmarkInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.NoDeviceSelectedBenchmark, Tr("No device selected to benchmark"), Tr("No device has been selected, there is nothing to benchmark."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        [Obsolete("Not used anymore, we might add it back in the future")]
        public static void CreateNothingToBenchmarkInfo()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NothingToBenchmark, Tr("Nothing to benchmark"), Tr("Current benchmark settings are already executed. There is nothing to do."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoSupportedDevicesInfo()
        {
            var notification = new Notification(NotificationsType.Fatal, NotificationsGroup.NoSupportedDevices, Tr("No Supported Devices"), Tr("No supported devices are found."));
            notification.Action = new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Process.Start(Links.NhmNoDevHelp); }
            };
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMissingMinersInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.MissingMiners, Tr("Missing miner files"), Tr("There are missing files from last Miners Initialization. Please make sure that the file is accessible and that your anti-virus is not blocking the application. NiceHash Miner might not work properly without missing files. Please check the following blog post: {0}", Links.AVHelp));
            notification.Action = new NotificationAction
            {
                Info = Tr("Restart NiceHash Miner"),
                Action = () => { _ = ApplicationStateManager.RestartProgram(); }
            };
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMissingGPUsInfo()
        {
            var notification = new Notification(
                NotificationsType.Warning,
                NotificationsGroup.MissingGPUs,
                Tr("Missing GPUs"),
                Tr("There are missing GPUs from inital NiceHash Miner startup. This is usually caused by driver crashes and usually requires system restart to recover."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNhmUpdateInfoDownload(bool isInstallerVersion)
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NhmUpdate, Tr("NiceHash Miner Update"), Tr("New version of NiceHash Miner is available."));
            if (!Configs.UpdateSettings.Instance.AutoUpdateNiceHashMiner)
            {
                notification.Action = new NotificationAction
                {
                    Info = Tr("Download updater"),
                    Action = () =>
                    {
                        ApplicationStateManager.App.Dispatcher.Invoke(async () =>
                        {
                            var ok = await UpdateHelpers.StartDownloadingUpdater(isInstallerVersion);
                            if (!ok)
                            {
                                CreateNhmUpdateAttemptFail();
                            }
                            else
                            {
                                NotificationsManager.Instance.RemoveNotificationFromList(notification);
                                CreateNhmUpdateInfoUpdate();
                            }
                        });
                    },
                    IsSingleShotAction = true,
                    BindProgress = true,
                };
            }

            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNhmUpdateInfoUpdate()
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NhmUpdate, Tr("NiceHash Miner Update"), Tr("New version of NiceHash Miner is available."));
            if (!Configs.UpdateSettings.Instance.AutoUpdateNiceHashMiner)
            {
                notification.Action = new NotificationAction
                {
                    Info = Tr("Start updater"),
                    Action = () =>
                    {
                        ApplicationStateManager.App.Dispatcher.Invoke(async () =>
                        {
                            var ok = await UpdateHelpers.StartUpdateProcess();
                            if (!ok) CreateNhmUpdateAttemptFail();
                        });
                    },
                    IsSingleShotAction = true,
                };
            }

            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNhmUpdateAttemptFail()
        {
            var notificationIfUnsuccessfull = new Notification(NotificationsType.Warning, NotificationsGroup.NhmUpdateFailed, Tr("NiceHash Miner Update Failed"), Tr("Update procedure failed please install manually. Please make sure that the file is accessible and that your anti-virus is not blocking the application. NiceHash Miner might not work properly without missing files. Please check the following blog post: {0}", Links.AVHelp));
            notificationIfUnsuccessfull.Action = new NotificationAction
            {
                Info = Tr("Visit release Page"),
                Action = () => { Process.Start(Links.VisitReleasesUrl); }
            };
            NotificationsManager.Instance.AddNotificationToList(notificationIfUnsuccessfull);
            var notification = NotificationsManager.Instance.Notifications.FirstOrDefault(n => n.Group == NotificationsGroup.NhmUpdate);
            if (notification != null) NotificationsManager.Instance.RemoveNotificationFromList(notification);
        }

        public static void CreateNhmWasUpdatedInfo(bool success)
        {
            var sentence = "was updated";
            if (!success) sentence = "was not updated";

            var notification = new Notification(NotificationsType.Info, NotificationsGroup.NhmWasUpdated, Tr("NiceHash Miner was Updated"), Tr($"NiceHash Miner {sentence} to the latest version."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreatePluginUpdateInfo(string pluginName, bool success)
        {
            var sentence = "was installed";
            if (!success) sentence = "was not installed";

            var content = Tr("New version of {0} {1}.\n", pluginName, Tr(sentence));

            try
            {
                var pluginNotification = NotificationsManager.Instance.Notifications.Where(notif => notif.Group == NotificationsGroup.PluginUpdate).FirstOrDefault();
                if (pluginNotification != null)
                {
                    if (pluginNotification.NotificationNew == true)
                    {
                        //check if the same sentence was already written to notification
                        var newSentence = Tr("New version of {0} {1}.\n", pluginName, Tr(sentence));
                        if (pluginNotification.NotificationContent.Contains(newSentence))
                        {
                            return;
                        }

                        //add new content to prev content
                        content = pluginNotification.NotificationContent + newSentence;
                    }
                }
                //clean previous notification
                NotificationsManager.Instance.Notifications.Remove(pluginNotification);
            }
            catch (Exception ex)
            {
                Logger.Error("Notifications", ex.Message);
            }

            var notification = new Notification(NotificationsType.Info, NotificationsGroup.PluginUpdate, Tr("Miner Plugin Update"), content);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateMissingMinerBinsInfo(string pluginName)
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.MissingMinerBins, Tr("Missing miner binaries"), Tr("Some of the {0} binaries are missing from the installation folder. Please make sure that the files are accessible and that your anti-virus is not blocking the application.", pluginName));
            notification.Action = new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Process.Start(Links.AVHelp); }
            };
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEnableComputeModeAMDInfo()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.ComputeModeAMD, Tr("Switch compute/graphic mode"), Tr("Would you like to switch between compute and graphic mode for optimized profit?"));
            notification.Action = new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Process.Start(Links.AMDComputeModeHelp); }
            };
            notification.NotificationUUID = "AMDModeSwitchNotification";
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateEnableLargePagesInfo()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.LargePages, Tr("Enable large pages for randomx"), Tr("Would you like to enable large pages when mining with RandomX(CPU)?"));
            notification.Action = new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Process.Start(Links.LargePagesHelp); }
            };
            notification.NotificationUUID = "LargePagesNotification";
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateIncreaseVirtualMemoryInfo()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.VirtualMemory, Tr("Increase virtual memory"), Tr("NiceHash Miner recommends increasing virtual memory size so that all algorithms would work fine. Would you like to increase virtual memory?"));
            notification.Action = new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Process.Start(Links.VirtualMemoryHelp); }
            };
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedBenchmarksInfo(ComputeDevice device)
        {
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.FailedBenchmarks, Tr("Failed benchmarks"), Tr("Some benchmarks for {0} failed to execute. Check benchmark tab for more info.", device.Name));
            notification.Action = new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Process.Start(Links.FailedBenchmarkHelp); }
            };
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

            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.Market, Tr("Primary mining location unavailable"), Tr("Primary mining location is unavailable. Switching to fallback location."));
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

            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.Market, Tr("All mining locations unavailable"), Tr("All mining locations are unavailable. Mining will be stopped."));
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
                var notification = new Notification(NotificationsType.Warning, NotificationsGroup.Profit, Tr("Mining not profitable"), Tr("Currently mining is not profitable. Mining will be stopped."));
                NotificationsManager.Instance.AddNotificationToList(notification);
            }
        }

        public static void CreateOpenClFallbackInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.OpenClFallback, Tr("Fallback of OpenCL"), Tr("Please check if AMD drivers are installed properly. If they are please remove Intel video driver."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateNoAvailableAlgorithmsInfo()
        {
            var notification = new Notification(NotificationsType.Error, NotificationsGroup.NoAvailableAlgorithms, Tr("No available algorithms"), Tr("There are no available algorithms to mine. Please check you rig stability and stability of installed plugins."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateLogUploadResultInfo(bool success, string uuid)
        {
            //clean previous
            var logUploadNotifications = NotificationsManager.Instance.Notifications.Where(notif => notif.Group == NotificationsGroup.LogArchiveUpload).FirstOrDefault();
            if (logUploadNotifications != null) logUploadNotifications.RemoveNotification();

            var sentence = Tr("was uploaded.");
            if (!success) sentence = Tr("was not uploaded. Please contact our support team for help.");
            var notification = new Notification(NotificationsType.Info, NotificationsGroup.LogArchiveUpload, Tr("Log archive upload result"), Tr("The log archive with the following ID: {0}", uuid) + " " + sentence);
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedNVMLInitInfo()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.NVMLInitFail, Tr("Failed NVML Initialization"), Tr("NVML was not initialized. Try to reinstall drivers - recommended standard drivers over DCH. Also you could try to restart Windows."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateFailedNVMLLoadInfo()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.NVMLLoadFail, Tr("Failed NVML Load"), Tr("NVML was not loaded. Try to reinstall drivers - recommended standard drivers over DCH. Also you could try to restart Windows."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }

        public static void CreateWarningNVIDIADCHInfo()
        {
            var notification = new Notification(NotificationsType.Warning, NotificationsGroup.NvidiaDCH, Tr("Nvidia DCH drivers detected"), Tr("Detected drivers are not recommended for mining with NiceHash Miner. Please change them for optimal performance."));
            NotificationsManager.Instance.AddNotificationToList(notification);
        }
    }
}

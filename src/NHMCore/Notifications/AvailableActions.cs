using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static NHMCore.Translations;

namespace NHMCore.Notifications
{
    public enum ActionID
    {
        ActionNONE = -1,
        ActionRunAsAdmin,
        ActionNHMNoDevHelp,
        ActionDownloadUpdater,
        ActionStartUpdater,
        ActionVisitReleasePage,
        ActionVisitAVHelp,
        ActionLargePagesHelp,
        ActionVisitMemoryHelp,
        ActionFailedBenchmarksHelp,
        ActionCopyToClipBoard,
        ActionNoOptimalDrivers,
    }


    public static class AvailableActions
    {
        public static NotificationAction ToAction(ActionID id, Notification notif = null, bool isInstallerVersion = false, string uuid = "") => id switch
        {
            ActionID.ActionRunAsAdmin => ActionRunAsAdmin(),
            ActionID.ActionNHMNoDevHelp => ActionNHMNoDevHelp(),
            ActionID.ActionDownloadUpdater => ActionDownloadUpdater(isInstallerVersion, notif),
            ActionID.ActionStartUpdater => ActionStartUpdater(),
            ActionID.ActionVisitReleasePage => ActionVisitReleasePage(),
            ActionID.ActionVisitAVHelp => ActionVisitAVHelp(),
            ActionID.ActionLargePagesHelp => ActionLargePagesHelp(),
            ActionID.ActionVisitMemoryHelp => ActionVisitMemoryHelp(),
            ActionID.ActionFailedBenchmarksHelp => ActionFailedBenchmarksHelp(),
            ActionID.ActionCopyToClipBoard => ActionCopyToClipBoard(uuid),
            ActionID.ActionNoOptimalDrivers => ActionNoOptimalDrivers(),
            _ => new NotificationAction()
        };
        public static NotificationAction ActionRunAsAdmin()
        {
            return new NotificationAction
            {
                Info = Tr("Run As Administrator"),
                Action = () => { RunAsAdmin.SelfElevate(); },
                ActionID = ActionID.ActionRunAsAdmin
            };
        }
        public static NotificationAction ActionNHMNoDevHelp()
        {
            return new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Helpers.VisitUrlLink(Links.NhmNoDevHelp); },
                ActionID = ActionID.ActionNHMNoDevHelp
            };
        }
        public static NotificationAction ActionDownloadUpdater(bool isInstallerVersion, Notification notification)
        {
            return new NotificationAction
            {
                Info = Tr("Download updater"),
                Action = () =>
                {
                    ApplicationStateManager.App.Dispatcher.Invoke(async () =>
                    {
                        var ok = await UpdateHelpers.StartDownloadingUpdater(isInstallerVersion);
                        if (!ok)
                        {
                            AvailableNotifications.CreateNhmUpdateAttemptFail();
                        }
                        else
                        {
                            NotificationsManager.Instance.RemoveNotificationFromList(notification);
                            AvailableNotifications.CreateNhmUpdateInfoUpdate();
                        }
                    });
                },
                ActionID = ActionID.ActionDownloadUpdater,
                IsSingleShotAction = true,
                BindProgress = true,
            };
        }
        public static NotificationAction ActionStartUpdater()
        {
            return new NotificationAction
            {
                Info = Tr("Start updater"),
                Action = () =>
                {
                    ApplicationStateManager.App.Dispatcher.Invoke(async () =>
                    {
                        var ok = await UpdateHelpers.StartUpdateProcess();
                        if (!ok) AvailableNotifications.CreateNhmUpdateAttemptFail();
                    });
                },
                ActionID = ActionID.ActionStartUpdater,
                IsSingleShotAction = true,
            };
        }
        public static NotificationAction ActionVisitReleasePage()
        {
            return new NotificationAction
            {
                Info = Tr("Visit release Page"),
                Action = () => { Helpers.VisitUrlLink(Links.VisitReleasesUrl); },
                ActionID = ActionID.ActionVisitReleasePage
            };
        }
        public static NotificationAction ActionVisitAVHelp()
        {
            return new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Helpers.VisitUrlLink(Links.AVHelp); },
                ActionID = ActionID.ActionVisitAVHelp
            };
        }
        public static NotificationAction ActionLargePagesHelp()
        {
            return new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Helpers.VisitUrlLink(Links.LargePagesHelp); },
                ActionID = ActionID.ActionLargePagesHelp
            };
        }
        public static NotificationAction ActionVisitMemoryHelp()
        {
            return new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Helpers.VisitUrlLink(Links.VirtualMemoryHelp); },
                ActionID = ActionID.ActionVisitMemoryHelp
            };
        }
        public static NotificationAction ActionFailedBenchmarksHelp()
        {
            return new NotificationAction
            {
                Info = Tr("Help"),
                Action = () => { Helpers.VisitUrlLink(Links.FailedBenchmarkHelp); },
                ActionID = ActionID.ActionFailedBenchmarksHelp
            };
        }
        public static NotificationAction ActionCopyToClipBoard(string uuid)
        {
            return new NotificationAction
            {
                Info = Tr("Copy to clipboard"),
                Action = () => { Clipboard.SetText(uuid); },
                ActionID = ActionID.ActionCopyToClipBoard,
                Extra = uuid
            };
        }
        public static NotificationAction ActionNoOptimalDrivers()
        {
            return new NotificationAction
            {
                Info = Tr("Show more"),
                Action = () => { Helpers.VisitUrlLink(Links.NoOptimalDrivers); },
                ActionID = ActionID.ActionNoOptimalDrivers
            };
        }
    }
}

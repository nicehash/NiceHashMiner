using NHM.Common;
using NHMCore.Utils;
using System.Collections.Generic;
using System.Windows.Forms;

using static NHMCore.Translations;

namespace NHMCore.Notifications
{
    public class NotificationsManager : NotifyChangedBase
    {
        public static NotificationsManager Instance { get; } = new NotificationsManager();
        private static readonly object _lock = new object();

        private NotificationsManager()
        {}


        private readonly List<Notification> _notifications = new List<Notification>();

        // TODO must not modify Notifications outside manager
        public List<Notification> Notifications
        {
            get
            {
                lock (_lock)
                {
                    return _notifications;
                }
            }
        }


        //public List<Notification> Notifications { }
        public void AddNotificationToList(Notification notification)
        {
            lock (_lock)
            {
                _notifications.Add(notification);
            }
            OnPropertyChanged(nameof(Notifications));
        }

        public bool RemoveNotificationFromList(Notification notification)
        {
            var ok = false;
            lock (_lock)
            {
                ok = _notifications.Remove(notification);
            }
            OnPropertyChanged(nameof(Notifications));
            return ok;
        }

        // TODO this is here temporary 
        #region Notifications creation methods

        public void CreateDeviceMonitoringNvidiaElevateInfo()
        {
            var notification = new Notification(Translations.Tr("NVIDIA TDP Settings Insufficient Priviledges"), Translations.Tr("Disabled NVIDIA power mode settings due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
            notification.Actions.Add(new NotificationAction {
                Info = "Run As Administrator",
                Action = () => {
                    var dialogResult = MessageBox.Show(Tr("Click yes if you wish to run {0} as Administrator.", NHMProductInfo.Name),
                        Tr("Run as Administrator"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                        RunAsAdmin.SelfElevate();
                }
            });
            AddNotificationToList(notification);
        }

        #endregion Notifications creation methods

    }
}

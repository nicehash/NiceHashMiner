using NHM.Common;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Notifications
{
    public class NotificationsManager : NotifyChangedBase
    {
        public static NotificationsManager Instance { get; } = new NotificationsManager();
        private static readonly object _lock = new object();

        private NotificationsManager()
        { }

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

        public void AddNotificationToList(Notification notification)
        {
            Configs.MiscSettings.Instance.ShowNotifications.TryGetValue(notification.NotificationUUID, out var shouldNotAdd);
            if (shouldNotAdd) return;

            //only have 1 notification of same type
            var groupNotifications = _notifications.Where(notif => notif.Group == notification.Group).ToList();
            if (groupNotifications.Count != 0) return;

            lock (_lock)
            {
                notification.NotificationNew = true;
                _notifications.Insert(0, notification);
                notification.PropertyChanged += Notification_PropertyChanged;
            }
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(NotificationNewCount));
        }

        private void Notification_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(Notification.NotificationNew) == e.PropertyName)
            {
                OnPropertyChanged(nameof(NotificationNewCount));
            }
        }

        public bool RemoveNotificationFromList(Notification notification)
        {
            var ok = false;
            lock (_lock)
            {
                ok = _notifications.Remove(notification);
                notification.PropertyChanged -= Notification_PropertyChanged;
            }
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(NotificationNewCount));
            return ok;
        }

        private int _notificationNewCount { get; set; }
        public int NotificationNewCount
        {
            get => Instance.Notifications.Where(notif => notif.NotificationNew == true).Count();
            set
            {
                _notificationNewCount = value;
                OnPropertyChanged(nameof(NotificationNewCount));
            }
        }
    }
}

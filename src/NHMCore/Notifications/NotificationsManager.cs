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

        public bool RemoveNotificationFromList(string notificationName)
        {
            var ok = false;
            lock (_lock)
            {
                var removedNotification = _notifications.Where(notification => notification.Name == notificationName).FirstOrDefault();
                ok = _notifications.Remove(removedNotification);
            }
            OnPropertyChanged(nameof(Notifications));
            return ok;
        }
    }
}

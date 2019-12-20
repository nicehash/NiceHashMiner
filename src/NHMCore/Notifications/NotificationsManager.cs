using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

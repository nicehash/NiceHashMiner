using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Notifications
{
    public class Notification : NotifyChangedBase
    {
        public NotificationsType NotificationsType { get; } = NotificationsType.Info;

        public Notification(string name, string content)
        {
            NotificationName = name;
            NotificationContent = content;
        }
             
        private string _notificationName { get; set; }
        public string NotificationName
        {
            get => _notificationName;
            set
            {
                _notificationName = value;
                OnPropertyChanged(nameof(NotificationName));
            }
        }

        private string _notificationContent { get; set; }
        public string NotificationContent
        {
            get => _notificationContent;
            set
            {
                _notificationContent = value;
                OnPropertyChanged(nameof(NotificationContent));
            }
        }
    }
}

using NHM.Common;
using System.Collections.Generic;

namespace NHMCore.Notifications
{
    public class Notification : NotifyChangedBase
    {
        public NotificationsType Type { get; } = NotificationsType.Info;

        public Notification(string name, string content)
        {
            Name = name;
            NotificationContent = content;
        }

        public Notification(NotificationsType type, string name, string content)
        {
            Type = type;
            Name = name;
            NotificationContent = content;
        }

        public List<INotificationBaseAction> Actions { get; } = new List<INotificationBaseAction>();

        private string _notificationName { get; set; }
        public string Name
        {
            get => _notificationName;
            set
            {
                _notificationName = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool _notificationNew { get; set; }
        public bool NotificationNew
        {
            get => _notificationNew;
            set
            {
                _notificationNew = value;
                OnPropertyChanged(nameof(NotificationNew));
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

        public void RemoveNotification()
        {
            NotificationsManager.Instance.RemoveNotificationFromList(this);
        }
    }
}

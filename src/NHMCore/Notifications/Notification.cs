using NHM.Common;
using System;

namespace NHMCore.Notifications
{
    public class Notification : NotifyChangedBase
    {
        public NotificationsType Type { get; } = NotificationsType.Info;
        //public NotificationsGroup Group { get; } = NotificationsGroup.Misc;
        public string Group { get; } = NotificationsGroup.Misc.ToString();

        public Notification(string name, string content)
        {
            Name = name;
            NotificationContent = content;
            NotificationEpochTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            //NotificationTime = DateTime.Now.ToString("dd/MM/y hh:mm tt");
        }

        public Notification(NotificationsType type, string name, string content)
        {
            Type = type;
            Name = name;
            NotificationContent = content;
            NotificationEpochTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            //NotificationTime = DateTime.Now.ToString("dd/MM/y hh:mm tt");
        }

        public Notification(NotificationsType type, NotificationsGroup group, string name, string content)
        {
            Type = type;
            Group = group.ToString();
            Name = name;
            NotificationContent = content;
            NotificationEpochTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            //NotificationTime = DateTime.Now.ToString("dd/MM/y hh:mm tt");
        }

        public Notification(NotificationsType type, NotificationsGroup group, string name, string content, string url)
        {
            Type = type;
            Group = group.ToString();
            Name = name;
            NotificationContent = content;
            NotificationUrl = url;
            NotificationEpochTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            //NotificationTime = DateTime.Now.ToString("dd/MM/y hh:mm tt");
        }

        public Notification(NotificationsType type, string dynamicGroup, string name, string content)
        {
            Type = type;
            Group = dynamicGroup;
            Name = name;
            NotificationContent = content;
            NotificationEpochTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public NotificationAction Action { get; internal set; } = null;

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

        private string _notificationUUID { get; set; } = "";
        public string NotificationUUID
        {
            get => _notificationUUID;
            set
            {
                _notificationUUID = value;
                OnPropertyChanged(nameof(NotificationUUID));
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

        //private string _notificationTime { get; set; }
        //public string NotificationTime
        //{
        //    get => _notificationTime;
        //    set
        //    {
        //        _notificationTime = value;
        //        OnPropertyChanged(nameof(NotificationTime));
        //    }
        //}

        private int _notificationEpochTime { get; set; }
        public int NotificationEpochTime
        {
            get => _notificationEpochTime;
            set
            {
                _notificationEpochTime = value;
                OnPropertyChanged(nameof(NotificationEpochTime));
            }
        }

        private string _notificationTime { get; set; }
        public string NotificationTime
        {
            get => _notificationTime;
            set
            {
                _notificationTime = value;
                OnPropertyChanged(nameof(NotificationTime));
            }
        }

        public void UpdateNotificationTimeString()
        {
            var returnTime = "";
            DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeSeconds(NotificationEpochTime);
            DateTime dateTimeOfNotification = dateTimeOffSet.UtcDateTime;
            var now = DateTime.UtcNow;
            var secondsDiff = (now - dateTimeOfNotification).TotalSeconds;
            if (secondsDiff < 60) returnTime = "Under a minute ago";
            else if (secondsDiff < 120) returnTime = "A minute ago";
            else if (secondsDiff < 3600) returnTime = Math.Floor(secondsDiff / 60) + " minutes ago";
            else if (secondsDiff < 86400) returnTime = Math.Floor(secondsDiff / 60 / 60) + " hours ago";
            else if (secondsDiff < 172800) returnTime = "Yesterday at " + dateTimeOfNotification.TimeOfDay;
            else if (secondsDiff < 604800) returnTime = dateTimeOfNotification.DayOfWeek + " at " + dateTimeOfNotification.TimeOfDay;
            else returnTime = dateTimeOfNotification.Date + " at " + dateTimeOfNotification.TimeOfDay;
            NotificationTime = returnTime;
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

        private string _notificationUrl { get; set; }
        public string NotificationUrl
        {
            get => _notificationUrl;
            set
            {
                _notificationUrl = value;
                OnPropertyChanged(nameof(NotificationUrl));
            }
        }

        public void RemoveNotification()
        {
            NotificationsManager.Instance.RemoveNotificationFromList(this);
        }
    }
}

using NHM.Common;
using System;
using static NHMCore.Translations;

namespace NHMCore.Notifications
{
    public class Notification : NotifyChangedBase
    {
        public NotificationsType Type { get; } = NotificationsType.Info;
        public NotificationsGroup Group { get; } = NotificationsGroup.Misc;

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
            Group = group;
            Name = name;
            NotificationContent = content;
            NotificationEpochTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            //NotificationTime = DateTime.Now.ToString("dd/MM/y hh:mm tt");
        }

        public Notification(NotificationsType type, NotificationsGroup group, string name, string content, string url)
        {
            Type = type;
            Group = group;
            Name = name;
            NotificationContent = content;
            NotificationUrl = url;
            NotificationEpochTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            //NotificationTime = DateTime.Now.ToString("dd/MM/y hh:mm tt");
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

        private bool _isVisible { get; set; } = false;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

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
            if (secondsDiff < 60) returnTime = Tr("Under a minute ago");
            else if (secondsDiff < 120) returnTime = Tr("A minute ago");
            else if (secondsDiff < 3600) returnTime = Tr("{0} minutes ago", Math.Floor(secondsDiff / 60));
            else if (secondsDiff < 7200) returnTime = Tr("1 hour ago");
            else if (secondsDiff < 86400) returnTime = Tr("{0} hours ago", Math.Floor(secondsDiff / 60 / 60));
            else if (secondsDiff < 172800) returnTime = Tr("Yesterday at {0}", dateTimeOfNotification.TimeOfDay);
            else if (secondsDiff < 604800) returnTime = Tr("{0} at {1}", dateTimeOfNotification.DayOfWeek, dateTimeOfNotification.TimeOfDay);
            else returnTime = Tr("{0} at {1}", dateTimeOfNotification.Date.ToString("dd-MM-yyyy"), dateTimeOfNotification.TimeOfDay);
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

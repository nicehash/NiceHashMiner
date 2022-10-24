using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Configs;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using Windows.UI.Notifications;

namespace NHMCore.Notifications
{
    public class NotificationsManager : NotifyChangedBase
    {
        public static NotificationsManager Instance { get; } = new NotificationsManager();
        private static readonly object _lock = new object();
        private string TAG = "NotificationManager";
        private readonly string _notificationFile = Paths.RootPath("logs/notifications.json");
        private readonly int _notificationQuota = 15;
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
        public List<Notification> LatestNotifications
        {
            get
            {
                var ret = new List<Notification>();
                var ordered = Notifications.OrderByDescending(notif => notif.NotificationEpochTime);
                foreach(NotificationsGroup group in Enum.GetValues(typeof(NotificationsGroup)))
                {
                    var found = Notifications.FirstOrDefault(notif => notif.Group == group);
                    if (found != null) ret.Add(found);
                }
                return ret;
            }
        }

        public void AddNotificationToList(Notification notification, bool shouldWrite = true)
        {
            Configs.MiscSettings.Instance.ShowNotifications.TryGetValue(notification.NotificationUUID, out var shouldNotAdd);
            if (shouldNotAdd) return;
            if (_notifications.Any(notif => notif.NumericUID == notification.NumericUID)) return;
            lock (_lock)
            {
                notification.NotificationNew = shouldWrite;
                if (shouldWrite) _notifications.Insert(0, notification);
                else _notifications.Add(notification);
                notification.PropertyChanged += Notification_PropertyChanged;
            }
            CheckNotificationQuotaForGroupAndDeleteExcess(notification.Group, notification.Domain);
            SearchForNewestNotificationOfTypeAndSetHistory(notification);
            if (shouldWrite) WriteNotifications();
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(NotificationNewCount));
            OnPropertyChanged(nameof(LatestNotifications));
        }
        private void CheckNotificationQuotaForGroupAndDeleteExcess(NotificationsGroup group, string domain)
        {
            var groupNotifications = Notifications
                .Where(notif => notif.Group == group)
                .Where(notif => notif.Domain == domain)
                .ToList();
            if (groupNotifications.Count < _notificationQuota) return;
            var numToDelete = (int)Math.Floor(0.4 * groupNotifications.Count);
            lock (_lock)
            {
                var toDelete = Notifications.TakeLast(numToDelete);
                foreach (var notif in toDelete)
                {
                    notif.PropertyChanged -= Notification_PropertyChanged;
                    _notifications.Remove(notif);
                }
            }
            WriteNotifications();
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(NotificationNewCount));
            OnPropertyChanged(nameof(LatestNotifications));
        }
        private void Notification_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(Notification.NotificationNew) == e.PropertyName)
            {
                OnPropertyChanged(nameof(NotificationNewCount));
            }
        }
        private void ReManageNotificationProperties(Notification notification)
        {
            CheckNotificationQuotaForGroupAndDeleteExcess(notification.Group, notification.Domain);
            SearchForNewestNotificationOfTypeAndSetHistory(notification);
            WriteNotifications();
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(NotificationNewCount));
            OnPropertyChanged(nameof(LatestNotifications));
        }

        public bool RemoveNotificationFromList(Notification notification)
        {
            var ok = false;
            lock (_lock)
            {
                ok = _notifications.Remove(notification);
                notification.PropertyChanged -= Notification_PropertyChanged;
            }
            ReManageNotificationProperties(notification);
            return ok;
        }
        public bool RemoveAllNotificationsOfThisType(Notification notification)
        {
            var ok = false;
            lock (_lock)
            {
                var sameTypeNotifications = _notifications.Where(notif => notif.Group == notification.Group);
                foreach (var notif in sameTypeNotifications)
                {
                    notif.PropertyChanged -= Notification_PropertyChanged;
                }
                _notifications.RemoveAll(notif => notif.Group == notification.Group);
            }
            ReManageNotificationProperties(notification);
            return ok;
        }
        public bool ClearAllNotifications()
        {
            var ok = false;
            lock (_lock)
            {
                Notifications.ForEach(notif => notif.PropertyChanged -= Notification_PropertyChanged);
                _notifications.Clear();
            }
            WriteNotifications();
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(NotificationNewCount));
            OnPropertyChanged(nameof(LatestNotifications));
            return ok;
        }
        private void SearchForNewestNotificationOfTypeAndSetHistory(Notification notification)
        {
            var newest = Notifications
                .Where(notif => notif.Group == notification.Group)
                .OrderByDescending(notif => notif.NotificationEpochTime)
                .FirstOrDefault();
            if (newest == null) return;
            var olderNotifications = Notifications
                .Where(notif => notif.Group == notification.Group)
                .Where(notif => notif != newest)
                .OrderByDescending(notif => notif.NotificationEpochTime);
            foreach (var notif in olderNotifications) notif.NotificationNew = false;
            newest.OlderNotificationsOfSameType = olderNotifications.ToList();
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(LatestNotifications));
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
        private (bool ok, List<NotificationRecord> list) ReadNotifications()
        {
            List<NotificationRecord> recordList = new();
            try
            {
                using StreamReader reader = new(_notificationFile);
                var text = reader.ReadToEnd();
                var existingRecord = JsonConvert.DeserializeObject<List<NotificationRecord>>(text);
                if (existingRecord != null) recordList = existingRecord;
            }
            catch (Exception e)
            {
                Logger.Warn(TAG, e.Message);
                return (false, null);
            }
            return (true, recordList);
        }
        private void WriteNotifications()
        {
            var recordList = new List<NotificationRecord>();
            Notifications.ForEach(item => recordList.Add(NotificationRecord.NotificationToRecord(item)));
            lock (_lock)
            {
                using StreamWriter writer = new(_notificationFile);
                try
                {
                    var textToWrite = JsonConvert.SerializeObject(recordList, Formatting.Indented);
                    writer.Write(textToWrite);
                }
                catch (Exception e)
                {
                    Logger.Warn(TAG, e.Message);
                }
            }
        }
        public void ReadLoggedNotifications()
        {
            var res = ReadNotifications();
            if (!res.ok) return;
            foreach(var log in res.list)
            {
                AddNotificationToList(NotificationRecord.NotificationFromRecord(log), false);
            }
        }
    }
}

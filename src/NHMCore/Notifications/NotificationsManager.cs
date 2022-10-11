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
        private Random _random = new Random();
        private string TAG = "NotificationManager";
        private readonly string _notificationFile = "notifications.json";
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

        public void AddNotificationToList(Notification notification, bool shouldWrite = true)
        {
            Configs.MiscSettings.Instance.ShowNotifications.TryGetValue(notification.NotificationUUID, out var shouldNotAdd);
            if (shouldNotAdd) return;
            if (_notifications.Any(notif => notif.NumericUID == notification.NumericUID)) return;
            lock (_lock)
            {
                notification.NotificationNew = true;
                _notifications.Insert(0, notification);
                notification.PropertyChanged += Notification_PropertyChanged;
            }
            CheckNotificationQuotaForEachGroupAndDeleteExcess(notification.Group, notification.Domain);
            if (shouldWrite) WriteNotifications();
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(NotificationNewCount));
        }
        private void CheckNotificationQuotaForEachGroupAndDeleteExcess(NotificationsGroup group, string domain)
        {
            var groupNotifications = _notifications.Where(notif => notif.Group == group)
                                        .Where(notif => notif.Domain == domain).ToList();
            if (groupNotifications.Count < _notificationQuota) return;
            var numToDelete = (int)Math.Floor(0.4 * groupNotifications.Count);
            int count = 0;
            foreach(var notification in groupNotifications)
            {
                if (count > numToDelete) break;
                RemoveNotificationFromList(notification);
                count++;
            }
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
            CheckNotificationQuotaForEachGroupAndDeleteExcess(notification.Group, notification.Domain);
            WriteNotifications();
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
        (bool ok, List<NotificationRecord> list) ReadNotifications()
        {
            var path = Paths.AppRootPath(_notificationFile);
            List<NotificationRecord> recordList = new();
            try
            {
                using StreamReader reader = new(path);
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
        public void WriteNotifications()
        {
            var path = Paths.AppRootPath(_notificationFile);
            var recordList = new List<NotificationRecord>();
            _notifications.ForEach(item => recordList.Add(NotificationRecord.NotificationToRecord(item)));
            lock (_lock)
            {
                using StreamWriter writer = new(path);
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

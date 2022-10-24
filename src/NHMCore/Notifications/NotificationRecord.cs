using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Notifications
{
    public record NotificationRecord
    {
        public long NumericUID { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public NotificationsType Type { get; set; } = NotificationsType.Info;
        public NotificationsGroup Group { get; set; } = NotificationsGroup.Misc;
        public string Domain { get; set; } = string.Empty;
        public string NotificationUrl { get; set; } = string.Empty;
        public string NotificationContent { get; set; } = string.Empty;
        public string NotificationTime { get; set; } = string.Empty;
        public int NotificationEpochTime { get; set; } = -1;
        public bool IsVisible { get; set; } = true; //?
        public bool NotificationNew { get; set; } = true; //?
        public string NotificationUUID { get; set; } = string.Empty;
        public ActionID Action { get; set; }
        public string Action_Extra = string.Empty;


        public static NotificationRecord NotificationToRecord(Notification notification)
        {
            return new NotificationRecord()
            {
                NumericUID = notification.NumericUID,
                Name = notification.Name,
                Type = notification.Type,
                Group = notification.Group,
                Domain = notification.Domain,
                NotificationUrl = notification.NotificationUrl,
                NotificationContent = notification.NotificationContent,
                NotificationTime = notification.NotificationTime,
                NotificationEpochTime = notification.NotificationEpochTime,
                NotificationNew = notification.NotificationNew,
                NotificationUUID = notification.NotificationUUID,
                IsVisible = notification.IsVisible,
                Action = notification.Action?.ActionID ?? ActionID.ActionNONE,
                Action_Extra = notification.Action?.Extra ?? string.Empty,
            };
        }
        public static Notification NotificationFromRecord(NotificationRecord notificationRecord)
        {
            var notif = new Notification(notificationRecord.Type, notificationRecord.Domain, notificationRecord.Group, notificationRecord.Name, notificationRecord.NotificationContent)
            {
                NotificationUrl = notificationRecord.NotificationUrl,
                NotificationTime = notificationRecord.NotificationTime,
                NotificationEpochTime = notificationRecord.NotificationEpochTime,
                NotificationNew = notificationRecord.NotificationNew,
                NotificationUUID = notificationRecord.NotificationUUID,
                IsVisible = notificationRecord.IsVisible,
                NumericUID = notificationRecord.NumericUID,
            };
            if (notificationRecord.Action == ActionID.ActionNONE) notif.Action = null;
            else
            {
                notif.Action = AvailableActions.ToAction(notificationRecord.Action);
                notif.Action.Extra = notificationRecord.Action_Extra;
            }
            return notif;
        }
    }
}

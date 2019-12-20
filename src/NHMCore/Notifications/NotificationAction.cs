using System;

namespace NHMCore.Notifications
{
    public class NotificationAction : INotificationBaseAction
    {
        public string Info { get; internal set; }
        public Action Action { get; internal set; }
    }
}

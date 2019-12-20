using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.ApplicationState
{
    public class NotificationState : NotifyChangedBase
    {
        public static NotificationState Instance { get; } = new NotificationState();

        private NotificationState()
        {
            NotificationList = new List<NotificationState>();
        }

        private NotificationState(string name, string content)
        {
            NotificationName = name;
            NotificationContent = content;
        }

        public List<NotificationState> NotificationList { get; private set; }

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

        public void AddNotificationToList(NotificationState helpNotification)
        {
            NotificationList.Add(helpNotification);
        }
    }
}

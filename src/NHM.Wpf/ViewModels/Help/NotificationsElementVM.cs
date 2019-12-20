using NHM.Common;
using NHMCore.ApplicationState;
using NHMCore.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels.Help
{
    public class NotificationsElementVM : NotifyChangedBase
    {
        private Notification _notification;

        public string NotificationName => _notification.NotificationName;
        public string NotificationContent => _notification.NotificationContent;

        public NotificationsElementVM(Notification notification)
        {
            _notification = notification;
            OnPropertyChanged(nameof(NotificationName));
            OnPropertyChanged(nameof(NotificationContent));
        }
    }
}

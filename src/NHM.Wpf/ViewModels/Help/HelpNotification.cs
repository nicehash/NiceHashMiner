using NHMCore.ApplicationState;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels
{
    public class HelpNotification
    {
        public NotificationState Notification => NotificationState.Instance;

        public HelpNotification(string name, string content)
        {
            Notification.NotificationName = name;
            Notification.NotificationContent = content;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels
{
    public class HelpNotification
    {
        public static IEnumerable<HelpNotification> HelpNotificationsList { get; private set; }

        public string notificationName { get; private set; }
        public string notificationContent { get; private set; }

        public HelpNotification()
        {
        }

        public HelpNotification(string name, string content)
        {
            notificationName = name;
            notificationContent = content;
        }

        public void AddNotificationToList(HelpNotification helpNotification)
        {
            HelpNotificationsList.Prepend(helpNotification);
        }
    }
}

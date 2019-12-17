using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Notifications
{
    public class NotificationsManager : NotifyChangedBase
    {
        public static NotificationsManager Instance { get; } = new NotificationsManager();

        private NotificationsManager()
        {}
        // TODO add notifications list
    }
}

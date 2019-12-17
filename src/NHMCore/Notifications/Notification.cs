using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Notifications
{
    public abstract class Notification
    {
        public abstract NotificationsType NotificationsType { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Notifications
{
    public record NotificationRecordList
    {
        public List<NotificationRecord> List { get; set; } = new List<NotificationRecord>();
    }
}

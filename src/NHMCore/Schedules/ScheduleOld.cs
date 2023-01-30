using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Schedules
{
    public class ScheduleOld
    {
        public string From { get; set; } = "";

        public string To { get; set; } = "";

        public Dictionary<string, bool> Days { get; set; } = new()
        {
            ["Monday"] = false,
            ["Tuesday"] = false,
            ["Wednesday"] = false,
            ["Thursday"] = false,
            ["Friday"] = false,
            ["Saturday"] = false,
            ["Sunday"] = false
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Schedules
{
    public class Schedule
    {
        public string From = "";

        public string To = "";

        public Dictionary<string, bool> Days = new()
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

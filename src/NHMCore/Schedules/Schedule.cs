using NHM.Common;
using System.Collections.Generic;

namespace NHMCore.Schedules
{
    public class Schedule : NotifyChangedBase
    {
        public string From { get; set; } = "";

        public string To { get; set; } = "";

        public Dictionary<string, bool> DaysFrom { get; set; } = new()
        {
            ["Monday"] = false,
            ["Tuesday"] = false,
            ["Wednesday"] = false,
            ["Thursday"] = false,
            ["Friday"] = false,
            ["Saturday"] = false,
            ["Sunday"] = false
        };

        public Dictionary<string, bool> DaysTo { get; set; } = new()
        {
            ["Monday"] = false,
            ["Tuesday"] = false,
            ["Wednesday"] = false,
            ["Thursday"] = false,
            ["Friday"] = false,
            ["Saturday"] = false,
            ["Sunday"] = false
        };

        public Schedule SetValues(ScheduleOld scheduleOld)
        {
            var scheduleNew = new Schedule()
            {
                From = scheduleOld.From,
                To = scheduleOld.To,
                DaysFrom = scheduleOld.Days,
                DaysTo = scheduleOld.Days,
            };
            return scheduleNew;
        }
    }
}

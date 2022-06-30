using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Schedules
{
    public class SchedulesManager : NotifyChangedBase
    {
        public static SchedulesManager Instance { get; } = new SchedulesManager();
        private static readonly object _lock = new object();

        private SchedulesManager()
        { }

        private readonly List<Schedule> _schedules = new List<Schedule>();

        public List<Schedule> Schedules
        {
            get
            {
                lock (_lock)
                {
                    return _schedules;
                }
            }
        }

        public void AddScheduleToList()
        {
            _schedules.Add(new Schedule());
            OnPropertyChanged(nameof(Schedules));
        }
    }
}

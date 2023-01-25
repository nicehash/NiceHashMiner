using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHM.Common;
using NHMCore.Configs;
using NHMCore.Nhmws.V4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        private ObservableCollection<Schedule> _schedules = new ObservableCollection<Schedule>();

        public ObservableCollection<Schedule> Schedules
        {
            get
            {
                lock (_lock)
                {
                    return _schedules;
                }
            }
            set
            {
                lock (_lock)
                {
                    _schedules = value;
                }
            }
        }

        public void Init()
        {
            if (File.Exists(Paths.ConfigsPath("Schedule.json")))
            {
                var schedules = JsonConvert.DeserializeObject<ObservableCollection<ScheduleOld>>(File.ReadAllText(Paths.ConfigsPath("Schedule.json")));
                if (schedules != null && schedules.Any(s => s.Days.Any(d => d.Value == true)))
                {
                    foreach (var schedule in schedules)
                    {
                        _schedules.Add(new Schedule().SetValues(schedule));
                    }
                }
                else _schedules = JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(File.ReadAllText(Paths.ConfigsPath("Schedule.json")));
                OnPropertyChanged(nameof(Schedules));
                ConfigManager.ScheduleConfigFileCommit();
            }
        }

        public void AddScheduleToList(Schedule schedule)
        {
            Schedules.Add(schedule);
            OnPropertyChanged(nameof(Schedules));
            ConfigManager.ScheduleConfigFileCommit();
            Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
        }

        public void DeleteScheduleFromList(Schedule schedule)
        {
            Schedules.Remove(schedule);
            OnPropertyChanged(nameof(Schedules));
            ConfigManager.ScheduleConfigFileCommit();
            Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
        }

        public void ClearScheduleList()
        {
            Schedules.Clear();
            OnPropertyChanged(nameof(Schedules));
            ConfigManager.ScheduleConfigFileCommit();
            Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
        }

        public string ScheduleToJSON()
        {
            var schedules = new SchedulesWS4();
            schedules.enabled = MiningSettings.Instance.UseScheduler;
            foreach (var slot in Schedules)
            {
                var from = TimeSpan.Parse(slot.From);
                var to = TimeSpan.Parse(slot.To);
                var days = new List<int>();
                int counter = 1;
                foreach (var (_, enabled) in slot.DaysFrom)
                {
                    if (enabled) days.Add(counter % 7);
                    counter++;
                }
                //if (days.Count == 7) days.Clear();//if empty it assumes all days are selected
                days.Sort();
                var newSlot = new List<object> {
                    from.Hours,
                    from.Minutes,
                    to.Hours,
                    to.Minutes,
                    days,
                };
                schedules.slots.Add(newSlot);
            }
            var ret = JsonConvert.SerializeObject(schedules);
            return ret;
        }

        public (bool enabled, List<Schedule> schedules) ScheduleFromJSON(string schedule)
        {
            List<Schedule> ret = new List<Schedule>();
            var schedules = JsonConvert.DeserializeObject<SchedulesWS4>(schedule);
            if (schedules == null || schedules.slots == null || schedules.slots.Count <= 0) return (false, null);
            foreach (var slot in schedules.slots)
            {
                if (slot.Count != 5) continue;
                if (slot[0] is long fromH &&
                    slot[1] is long fromM &&
                    slot[2] is long toH &&
                    slot[3] is long toM &&
                    slot[4] is JArray days)
                {
                    var daysConverted = days.ToObject<List<int>>();
                    if (daysConverted.Count == 0) daysConverted.AddRange(new List<int> { 0, 1, 2, 3, 4, 5, 6 });
                    var formattedFromH = fromH <= 9 ? $"0{fromH}" : fromH.ToString();
                    var formattedFromM = fromM <= 9 ? $"0{fromM}" : fromM.ToString();
                    var formattedToH = toH <= 9 ? $"0{toH}" : toH.ToString();
                    var formattedToM = toM <= 9 ? $"0{toM}" : toM.ToString();
                    ret.Add(ScheduleToLocalFormat($"{formattedFromH}:{formattedFromM}", $"{formattedToH}:{formattedToM}", daysConverted));
                }
            }
            return (schedules.enabled, ret);
        }

        public Schedule ScheduleToLocalFormat(string schedulerFrom, string schedulerTo, List<int> days)
        {
            bool cboxMon = days.Contains(1);
            bool cboxTue = days.Contains(2);
            bool cboxWed = days.Contains(3);
            bool cboxThu = days.Contains(4);
            bool cboxFri = days.Contains(5);
            bool cboxSat = days.Contains(6);
            bool cboxSun = days.Contains(0);

            var isNextDay = Convert.ToDateTime(schedulerTo) < Convert.ToDateTime(schedulerFrom);
            if (isNextDay)
            {
                var schedule = new Schedule()
                {
                    From = schedulerFrom,
                    To = schedulerTo,
                    DaysFrom = new Dictionary<string, bool>()
                    {
                        ["Monday"] = cboxMon,
                        ["Tuesday"] = cboxTue,
                        ["Wednesday"] = cboxWed,
                        ["Thursday"] = cboxThu,
                        ["Friday"] = cboxFri,
                        ["Saturday"] = cboxSat,
                        ["Sunday"] = cboxSun,
                    },
                    DaysTo = new Dictionary<string, bool>()
                    {
                        ["Monday"] = cboxSun,
                        ["Tuesday"] = cboxMon,
                        ["Wednesday"] = cboxTue,
                        ["Thursday"] = cboxWed,
                        ["Friday"] = cboxThu,
                        ["Saturday"] = cboxFri,
                        ["Sunday"] = cboxSat,
                    }
                };
                return schedule;
            }
            else
            {
                var schedule = new Schedule()
                {
                    From = schedulerFrom,
                    To = schedulerTo,
                    DaysFrom = new Dictionary<string, bool>()
                    {
                        ["Monday"] = cboxMon,
                        ["Tuesday"] = cboxTue,
                        ["Wednesday"] = cboxWed,
                        ["Thursday"] = cboxThu,
                        ["Friday"] = cboxFri,
                        ["Saturday"] = cboxSat,
                        ["Sunday"] = cboxSun,
                    },
                    DaysTo = new Dictionary<string, bool>()
                    {
                        ["Monday"] = cboxMon,
                        ["Tuesday"] = cboxTue,
                        ["Wednesday"] = cboxWed,
                        ["Thursday"] = cboxThu,
                        ["Friday"] = cboxFri,
                        ["Saturday"] = cboxSat,
                        ["Sunday"] = cboxSun,
                    }
                };
                return schedule;
            }
        }
    }
}

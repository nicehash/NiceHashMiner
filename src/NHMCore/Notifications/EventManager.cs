using Newtonsoft.Json;
using NHM.Common;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Notifications
{
    public static class EventManager
    {
        private static string TAG = "EventManager";
        private static readonly object _lock = new object();
        private static readonly object _lock2 = new object();
        private static readonly string _eventFile = Paths.RootPath("logs","events.json");
        private static List<Event> _events = new List<Event>();
        private static readonly int _eventQuota = 20;
        private static bool _init = false;
        public static event EventHandler<string> EventAdded;
        public static event EventHandler EventsLoaded;

        public class Event
        {
            public int ID;
            public int? DeviceID;
            public DateTime DateTime;
            public string Content;
        }
        public static void Init()
        {
            if(_init ) return;
            try
            {
                using StreamReader reader = new(_eventFile);
                var text = reader.ReadToEnd();
                var existingRecord = JsonConvert.DeserializeObject<List<Event>>(text);
                if (existingRecord != null) Events = existingRecord;
                EventsLoaded?.Invoke(null, null);
            }
            catch (Exception e)
            {
                Logger.Warn(TAG, e.Message);
            }
            _init = true;
        }
        public static List<Event> Events
        {
            get
            {
                lock (_lock)
                {
                    return _events;
                }
            }
            set
            {
                lock (_lock)
                {
                    _events = value;
                }
            }
        }
        public static void AddEvent(EventType type, string content = "")
        {
            if(!_init) return;
            lock (_lock2)
            {
                if(!ApplicationStateManager.isInitFinished && 
                    (type == EventType.DeviceEnabled || 
                    type == EventType.DeviceDisabled ||
                    type == EventType.AlgoEnabled || 
                    type == EventType.AlgoDisabled))
                {
                    return;
                }
                var now = DateTime.Now;
                var eventText = GetEventText(type, content);
                Events.Add(new Event() {ID = (int)type, DateTime = now, Content = eventText });
                if(Events.Count >= _eventQuota) Events.RemoveAt(0);
                var events = JsonConvert.SerializeObject(Events, Formatting.Indented);
                using StreamWriter w = File.CreateText(_eventFile);
                w.Write(events);
                Logger.Warn(TAG, $"Event occurred {eventText}");
                EventAdded?.Invoke(null, $"{String.Format("{0:G}", now)} - {eventText}");
                //todo send
                //todo onpropertyChanged
            }
        }

        private static string GetEventText(EventType type, string content = "")
        {
            string ret = type switch
            {
                EventType.Unknown => "",
                EventType.RigStarted => $"Rig started mining.",
                EventType.RigStopped => $"Rig stopped mining.",
                EventType.DeviceEnabled => $"GPU {content} enabled.",
                EventType.DeviceDisabled => $"GPU {content} disabled.",
                EventType.RigRestart => $"Rebooting this rig.",
                EventType.PluginFailiure => $"{content} failed to run successfully",
                EventType.MissingFiles => $"Missing files. Check your antivirus software",
                EventType.VirtualMemory => $"Virtual memory is low. Increase it",
                EventType.GeneralConfigErr => $"Configuration error. Reinstall is suggested",
                EventType.DriverCrash => $"GPU drivers crashed. Lower OC settings or reinstall the drivers",
                EventType.DeviceOverheat => $"GPU(s) ({content}) are overheating.",
                EventType.MissingDev => $"Missing devices ({content})",
                EventType.AlgoSwitch => $"Algo switch: ({content})",
                EventType.AlgoEnabled => $"Algorithm enabled: {content}",
                EventType.AlgoDisabled => $"Algorithm disabled: {content}",
                EventType.TestOverClockApplied => $"Test overclock applied on device {content}",
                EventType.TestOverClockFailed => $"Test overclock failed on device {content}",
                EventType.BundleApplied => $"Bundle {content} applied.",
                EventType.BenchmarkFailed => $"Benchmark combination {content} has failed",
                _ => ""
            };
            return ret;
        }
    }
}

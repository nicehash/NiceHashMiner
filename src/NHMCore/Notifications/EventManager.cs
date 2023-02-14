using Newtonsoft.Json;
using NHM.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Notifications
{
    internal class EventManager
    {
        public static EventManager Instance { get; } = new EventManager();
        private string TAG = "EventManager";
        private static readonly object _lock = new object();
        private static readonly object _lock2 = new object();
        private readonly string _eventFile = Paths.RootPath("logs","events.json");
        private List<Event> _events = new List<Event>();
        public class Event
        {
            public int ID;
            public int? DeviceID;
            public DateTime DateTime;
            public string Content;
        }
        private EventManager()
        {
            try
            {
                using StreamReader reader = new(_eventFile);
                var text = reader.ReadToEnd();
                var existingRecord = JsonConvert.DeserializeObject<List<Event>>(text);
                if (existingRecord != null) Events = existingRecord;
            }
            catch (Exception e)
            {
                Logger.Warn(TAG, e.Message);
            }
        }
        public List<Event> Events
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
        public void AddEvent(EventType type, string content = "")
        {
            lock (_lock2)
            {
                var now = DateTime.Now;
                Events.Add(new Event() {ID = (int)type, DateTime = now, Content = content });
                var events = JsonConvert.SerializeObject(Events);
                using StreamWriter w = File.CreateText(_eventFile);
                w.Write(events);
            }
        }
        public enum EventType
        {
            Unknown = 0,
            RigStarted = 1,
            RigStopped = 2, 
            DeviceEnabled = 3,
            DeviceDisabled = 4,
            RigRestart = 5,
            Unknown1 = 6,
            PluginFailiure = 7,
            MissingFiles = 8,
            VirtualMemory = 9,
            GeneralConfigErr = 10,
            Unknown2 = 11,
            DriverCrash = 12,
            DeviceOverheat = 13,
            MissingDev = 14,
            AlgoSwitch = 15,
            AlgoEnabled = 16,
            AlgoDisabled = 17,
            TestOverClockApplied = 18,
            TestOverClockFailed = 19,
            BundleApplied = 20,
            Unknown3 = 21,
            BenchmarkFailed = 22,
        }
    }
}

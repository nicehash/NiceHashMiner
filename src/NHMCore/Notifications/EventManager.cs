using Newtonsoft.Json;
using NHM.Common;
using NHMCore.Configs;
using NHMCore.Nhmws.V4;
using NHMCore.Switching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NHMCore.Notifications
{
    public class EventManager
    {
        public static EventManager Instance { get; } = new EventManager();

        private string TAG = "EventManager";
        private readonly object _lock = new object();
        private readonly string _eventFile = Paths.RootPath("logs", "events.json");
        private List<Event> _events = new List<Event>();
        private readonly int _eventQuota = 20;
        private bool _init = false;
        public event EventHandler<string> EventAdded;
        public event EventHandler EventsLoaded;

        public class Event
        {
            public int ID;
            public string? DeviceID;
            public DateTime DateTime;
            public string Content;
        }
        public void Init()
        {
            if (_init) return;
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
        public void AddEvent(EventType type, string content = "", string deviceID = null, bool send = true)
        {
            if (!_init) return;
            if (!ApplicationStateManager.isInitFinished &&
                (type == EventType.DeviceEnabled ||
                type == EventType.DeviceDisabled ||
                type == EventType.AlgoEnabled ||
                type == EventType.AlgoDisabled))
            {
                return;
            }
            var now = DateTime.Now;
            var eventText = GetEventText(type, content);
            Events.Add(new Event() { ID = (int)type, DateTime = now, Content = eventText, DeviceID = deviceID});
            if (Events.Count >= _eventQuota) Events.RemoveAt(0);
            var events = JsonConvert.SerializeObject(Events, Formatting.Indented);
            using StreamWriter w = File.CreateText(_eventFile);
            w.Write(events);
            Logger.Warn(TAG, $"Event occurred: {eventText}");
            EventAdded?.Invoke(null, $"{String.Format("{0:G}", now)} - {eventText}");
#if NHMWS4
            if (send) NHWebSocketV4.SendEvent(type, deviceID, content);
#endif
            //todo onpropertyChanged
        }

        private string GetEventText(EventType type, string content = "")
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
                EventType.MissingDev => $"Missing devices ({content})",
                EventType.AlgoSwitch => $"Algo switch: ({content})",
                EventType.AlgoEnabled => $"Algorithm enabled: {content}",
                EventType.AlgoDisabled => $"Algorithm disabled: {content}",
                EventType.TestOverClockApplied => $"Test overclock applied on device {content}",
                EventType.TestOverClockFailed => $"Test overclock failed on device {content}",
                EventType.BundleApplied => $"Bundle {content} applied.",
                EventType.BenchmarkFailed => $"Benchmark combination {content} has failed",
                EventType.BenchmarkStarted => $"Benchmark started for {content}",
                _ => ""
            };
            return ret;
        }
    }
}

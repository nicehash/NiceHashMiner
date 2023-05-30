﻿using Newtonsoft.Json;
using NHM.Common;
using NHMCore.Configs;
using NHMCore.Nhmws.V4;
using NHMCore.Switching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static NHM.MinerPluginToolkitV1.CommandLine.MinerConfigManager;
using static NHMCore.Utils.GPUProfileManager;

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
        private long GetUnixNow()
        {
            DateTimeOffset timeNow = new DateTimeOffset(DateTime.UtcNow);
            var unixTime = timeNow.ToUnixTimeSeconds();
            return unixTime;
        }
        public void AddEventUnknown(bool send = true)
        {
            var type = EventType.Unknown;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventRigStarted(bool send = true)
        {
            var type = EventType.RigStarted;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventRigStopped(bool send = true)
        {
            var type = EventType.RigStopped;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventDevEnabled(bool send = true, string devName = "", string devID = "")
        {
            var type = EventType.DeviceEnabled;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { GpuName = devName });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content, DeviceID = devID };
            AddEvent(type, ev, send);
        }
        public void AddEventDevDisabled(bool send = true, string devName = "", string devID = "")
        {
            var type = EventType.DeviceDisabled;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { GpuName = devName });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content, DeviceID = devID };
            AddEvent(type, ev, send);
        }
        public void AddEventRigRestart(bool send = true)
        {
            var type = EventType.RigRestart;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventPluginFail(bool send = true, string pluginName = "")
        {
            var type = EventType.PluginFailiure;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker, PluginName = pluginName });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventMissingFiles(bool send = true)
        {
            var type = EventType.MissingFiles;
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow() };
            AddEvent(type, ev, send);
        }
        public void AddEventVirtualMemInsufficient(bool send = true)
        {
            var type = EventType.VirtualMemory;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventGeneralCfg(bool send = true)
        {
            var type = EventType.GeneralConfigErr;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventDriverCrash(bool send = true)
        {
            var type = EventType.DriverCrash;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventDeviceOverheating(bool send = true, string gpuName = "", string devID = "")
        {
            var type = EventType.DeviceOverheat;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker, GpuName = gpuName });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content, DeviceID = devID };
            AddEvent(type, ev, send);
        }
        public void AddEventMissingDevice(bool send = true)
        {
            var type = EventType.MissingDevice;
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow() };
            AddEvent(type, ev, send);
        }
        public void AddEventSwitch(bool send = true, string oldAlgo = "", string newAlgo = "")
        {
            var type = EventType.AutoSwitch;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { AlgoNameOld = oldAlgo, AlgoNameNew = newAlgo });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventAlgoEnabled(bool send = true, string algo = "")
        {
            var type = EventType.AlgoEnabled;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { AlgoName = algo, RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventAlgoDisabled(bool send = true, string algo = "")
        {
            var type = EventType.AlgoDisabled;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { AlgoName = algo, RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventTestOCApplied(bool send = true, string devName = "", string devID = "")
        {
            var type = EventType.TestOCApplied;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { GpuName = devName, RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content, DeviceID = devID };
            AddEvent(type, ev, send);
        }
        public void AddEventTestOCFailed(bool send = true, string devName = "", string devID = "")
        {
            var type = EventType.TestOCFailed;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { GpuName = devName, RigName = worker });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventBundleApplied(bool send = true, string bundle = "")
        {
            var type = EventType.BundleApplied;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker, BundleName = bundle });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content };
            AddEvent(type, ev, send);
        }
        public void AddEventBenchmarkFailed(bool send = true, string plugin = "", string algo = "", string gpu = "", string devID = "")
        {
            var type = EventType.BenchmarkFailed;
            var worker = CredentialsSettings.Instance.GetCredentials().worker;
            var content = JsonConvert.SerializeObject(new NhmwsEventContent { RigName = worker, GpuName = gpu, AlgoName = algo, PluginName = plugin });
            var ev = new NhmwsEvent() { EventID = (int)type, Time = GetUnixNow(), Message = content, DeviceID = devID  };
            AddEvent(type, ev, send);
        }

        private void AddEvent(EventType type, NhmwsEvent ev, bool send = true)
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
            var eventText = GetEventText(type, ev);
            Events.Add(new Event() { ID = (int)type, DateTime = now, Content = eventText});
            if (Events.Count >= _eventQuota) Events.RemoveAt(0);
            var events = JsonConvert.SerializeObject(Events, Formatting.Indented);
            try
            {
                using StreamWriter w = File.CreateText(_eventFile);
                w.Write(events);
            }
            catch(Exception ex)
            {
                Logger.Warn(TAG, $"{ex}");
            }
            Logger.Warn(TAG, $"Event occurred: {eventText}");
            EventAdded?.Invoke(null, $"{String.Format("{0:G}", now)} - {eventText}");
#if NHMWS4
            if (send)
            {
                NHWebSocketV4.SendEvent(type, ev);
            }
#endif
        }

        private string GetEventText(EventType type, NhmwsEvent ev)
        {
            try
            {
                var additional = new NhmwsEventContent();
                if (ev.Message != null)
                {
                    additional = JsonConvert.DeserializeObject<NhmwsEventContent>(ev.Message);
                }
                string ret = type switch
                {
                    EventType.Unknown => "",
                    EventType.RigStarted => $"Rig started mining.",
                    EventType.RigStopped => $"Rig stopped mining.",
                    EventType.DeviceEnabled => $"GPU {additional.GpuName} enabled.",
                    EventType.DeviceDisabled => $"GPU {additional.GpuName} disabled.",
                    EventType.RigRestart => $"Rebooting this rig.",
                    EventType.PluginFailiure => $"{additional.PluginName} failed to run successfully",
                    EventType.MissingFiles => $"Missing files. Check your antivirus software",
                    EventType.VirtualMemory => $"Virtual memory is low. Increase it",
                    EventType.GeneralConfigErr => $"Configuration error. Reinstall is suggested",
                    EventType.DriverCrash => $"GPU drivers crashed. Lower OC settings or reinstall the drivers",
                    EventType.MissingDevice => $"Missing some devices!",
                    EventType.AutoSwitch => $"Algo switch: ({additional.AlgoNameNew})",
                    EventType.AlgoEnabled => $"Algorithm enabled: {additional.AlgoName}",
                    EventType.AlgoDisabled => $"Algorithm disabled: {additional.AlgoName}",
                    EventType.TestOCApplied => $"Test overclock applied on device {additional.GpuName}",
                    EventType.TestOCFailed => $"Test overclock failed on device {additional.GpuName}",
                    EventType.BundleApplied => $"Bundle {additional.BundleName} applied.",
                    EventType.BenchmarkFailed => $"Benchmark combination {additional.PluginName}/{additional.AlgoName} has failed",
                    _ => ""
                };
                return ret;
            }
            catch(Exception ex)
            {
                Logger.Error(TAG, "Event text retrieval error");
            }
            return "Generic event occurred";
        }
    }
}

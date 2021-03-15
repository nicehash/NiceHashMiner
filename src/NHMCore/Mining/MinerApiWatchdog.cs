using System;
using System.Collections.Generic;

namespace NHMCore.Mining
{
    static class MinerApiWatchdog
    {
        static object _lock = new object();
        static private Dictionary<string, DateTime> _groupKeyLastApiTimestamps = new Dictionary<string, DateTime>();
        static private Dictionary<string, TimeSpan> _groupKeyMaxTimeout = new Dictionary<string, TimeSpan>();

        private static bool _enabled = true;
        public static bool Enabled
        {
            get
            {
                lock (_lock) return _enabled;
            }
            set
            {
                lock (_lock) _enabled = value;
            }
        }

        public static void AddGroup(string groupKey, TimeSpan maxTimeout, DateTime addTime)
        {
            lock (_lock)
            {
                _groupKeyMaxTimeout[groupKey] = maxTimeout;
                _groupKeyLastApiTimestamps[groupKey] = addTime;
            }
        }

        public static void RemoveGroup(string groupKey)
        {
            lock (_lock)
            {
                _groupKeyMaxTimeout.Remove(groupKey);
                _groupKeyLastApiTimestamps.Remove(groupKey);
            }
        }

        public static void UpdateApiTimestamp(string groupKey, DateTime updateTime)
        {
            lock (_lock)
            {
                _groupKeyLastApiTimestamps[groupKey] = updateTime;
            }
        }

        public static List<string> GetTimedoutGroups(DateTime currentTime)
        {
            lock (_lock)
            {
                if (!_enabled) return null;

                var timedoutGroups = new List<string>();
                foreach (var kvp in _groupKeyMaxTimeout)
                {
                    var groupKey = kvp.Key;
                    var maxTimeout = kvp.Value;
                    var lastTimeStamp = _groupKeyLastApiTimestamps[groupKey];
                    var elapsed = currentTime - lastTimeStamp;
                    if (elapsed >= maxTimeout)
                    {
                        timedoutGroups.Add(groupKey);
                    }
                }
                return timedoutGroups;
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _groupKeyLastApiTimestamps.Clear();
                _groupKeyMaxTimeout.Clear();
            }
        }
    }
}

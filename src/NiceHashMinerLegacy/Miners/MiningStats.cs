using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinerPlugin;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Stats
{
    // MiningStats are the new APIData rates groups...
    // keep this here for now
    public static class MiningStats
    {
        private static object _lock = new object();

        // key is joined device MinerUUID-UUIDs, sorted uuids
        private static Dictionary<string, ApiData> _apiDataGroups = new Dictionary<string, ApiData>();

        public static void UpdateGroup(ApiData data, string minerUUID)
        {

            if (data == null) return;
            if (data.AlgorithmSpeedsPerDevice == null || data.AlgorithmSpeedsPerDevice.Count == 0) return;
            var sortedUUIDs = data.AlgorithmSpeedsPerDevice.Select(speedInfo => speedInfo.Key).OrderBy(uuid => uuid).ToList();
            var uuidsKeys = string.Join(",", sortedUUIDs);
            

            var key = $"{minerUUID}-{uuidsKeys}";
            
            // update groups
            lock (_lock)
            {
                if (_apiDataGroups.ContainsKey(key) == false)
                {
                    // check what keys to remove
                    var removeKeys = _apiDataGroups.Keys.Select(checkKey => sortedUUIDs.Any(uuid => checkKey.Contains(uuid))).ToArray();
                    foreach (var removeKey in removeKeys)
                    {
                        _apiDataGroups.Remove(key);
                    }
                }
                // add / update data
                _apiDataGroups[key] = data;
                // TODO notify change
            }
        }

        public static List<(AlgorithmType type, double speed)> GetSpeedForDevice(string uuid)
        {
            // update groups
            lock (_lock)
            {
                ApiData data = _apiDataGroups
                    .Where(kvp => kvp.Key.Contains(uuid))
                    .Select(kvp => kvp.Value)
                    .FirstOrDefault();
                if (data == null) return null;
                var speeds = data.AlgorithmSpeedsPerDevice
                    .Where(speedInfo => speedInfo.Key == uuid)
                    .Select(speedInfo => speedInfo.Value)
                    .FirstOrDefault();
                if (speeds == null) return null;
                return speeds.Select(info => (type: info.AlgorithmType, speed: info.Speed)).ToList();
            }
            // no speed found
            return null;
        }
    }
}

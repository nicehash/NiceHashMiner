using Newtonsoft.Json;
using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NanoMiner
{
    [Serializable]
    public class JsonApiResponse
    {
        public List<Dictionary<string, Dictionary<string, object>>> Algorithms { get; set; }
        public List<Dictionary<string, DeviceData>> Devices { get; set; }
    }

    [Serializable]
    public class DeviceData
    {
        public string Name { get; set; }
        public string Platform { get; set; }
        public string Pci { get; set; }
        public int Temperature { get; set; }
        public double Power { get; set; }
    }

    [Serializable]
    public class HashrateStats
    {
        public double Accepted { get; set; }
        public double Denied { get; set; }
        public double Hashrate { get; set; }
    }

    public struct DeviceStatsData
    {
        // hashrate stuff
        public double Accepted { get; set; }
        public double Denied { get; set; }
        public double Hashrate { get; set; }

        // device monitoring
        public int Temperature { get; set; }
        public double Power { get; set; }

        public DeviceStatsData(HashrateStats hashrate, DeviceData deviceData)
        {
            Accepted = hashrate?.Accepted ?? 0;
            Denied = hashrate?.Denied ?? 0;
            Hashrate = hashrate?.Hashrate ?? 0;
            Temperature = deviceData?.Temperature ?? 0;
            Power = deviceData?.Power ?? 0d;
        }
    }

    public static class JsonApiHelpers
    {
        public static double HashrateFromApiData(string data, string logGroup)
        {
            try
            {
                var hashSplit = data.Substring(data.IndexOf("Hashrate")).Replace("\"", "").Split(':');
                var hash = hashSplit[1].Substring(0, hashSplit[1].IndexOf('\r'));
                return double.Parse(hash, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Logger.Error(logGroup, $"Failed parsing hashrate: {e.Message}");
                return 0.0;
            }
        }

        private static Dictionary<string, object> GetAlgorithmStats(JsonApiResponse apiResponse)
        {
            if (apiResponse == null) return null;
            if (apiResponse.Algorithms == null) return null;
            var algo = apiResponse.Algorithms.FirstOrDefault();
            if (algo == null) return null;
            return algo.FirstOrDefault().Value;
        }

        public static Dictionary<string, DeviceStatsData> ParseJsonApiResponse(JsonApiResponse apiResponse, Dictionary<string, int> mappedIDs)
        {
            var ret = new Dictionary<string, DeviceStatsData>();

            // API seems to return these two by algorithm we mine 1 at a time so get first or default
            var devs = apiResponse?.Devices?.FirstOrDefault();
            var algos = GetAlgorithmStats(apiResponse);

            if (devs != null && algos != null && mappedIDs != null)
            {
                // get all keys and filter out 
                var keys = new HashSet<string>();
                foreach (var key in devs.Keys) keys.Add(key);
                foreach (var key in algos.Keys) keys.Add(key);
                keys.RemoveWhere(key => !key.Contains("GPU"));

                foreach (var key in keys)
                {
                    if (key.Contains("GPU"))
                    {
                        var keyGPUStrID = key.Split(' ').LastOrDefault();
                        if (!int.TryParse(keyGPUStrID, out var minerID)) continue;
                        var devUUIDPair = mappedIDs.Where(kvp => kvp.Value == minerID).FirstOrDefault();
                        if (devUUIDPair.Equals(default(KeyValuePair<string, int>))) continue;
                        var devUUID = devUUIDPair.Key;
                        var hashrate = JsonConvert.DeserializeObject<HashrateStats>(algos[key].ToString());
                        ret[devUUID] = new DeviceStatsData(hashrate, devs[key]);
                    }
                    else if (key.Contains("Total") && algos.ContainsKey(key))
                    {
                        ret[key] = new DeviceStatsData(null, devs[key]);
                    }
                    else
                    {
                        // what??
                    }
                }
            }
            return ret;
        }
    }
}

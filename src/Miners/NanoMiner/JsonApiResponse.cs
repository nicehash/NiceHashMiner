using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

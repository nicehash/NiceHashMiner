using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHM.DeviceMonitoring.TDP;
using System;

namespace NHMCore.Configs.Data
{
    [Serializable]
    public class DeviceTDPSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TDPSettingType SettingType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TDPSimpleType? Simple { get; set; }

        public double? Percentage { get; set; }
    }
}

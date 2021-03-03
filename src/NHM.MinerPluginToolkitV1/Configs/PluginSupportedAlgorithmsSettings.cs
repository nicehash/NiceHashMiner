using Newtonsoft.Json;
using NHM.Common.Algorithm;
using NHM.Common.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1.Configs
{
    public class PluginSupportedAlgorithmsSettings : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("enable_unsafe_RAM_limits")]
        public bool EnableUnsafeRAMLimits { get; set; } = false;

        [JsonProperty("default_fee")]
        public double DefaultFee { get; set; } = 0.0;
        [JsonProperty("algorithm_fees")]
        public Dictionary<AlgorithmType, double> AlgorithmFees { get; set; } = null;

        [JsonProperty("device_algorithms")]
        public Dictionary<DeviceType, List<SupportedAlgorithmSettings>> Algorithms { get; set; } = null;

        // for single algos
        [JsonProperty("plugin_algorithm_name")]
        public Dictionary<AlgorithmType, string> AlgorithmNames { get; set; } = null;

        public class SupportedAlgorithmSettings
        {
            public SupportedAlgorithmSettings() { }
            public SupportedAlgorithmSettings(params AlgorithmType[] ids)
            {
                IDs = new List<AlgorithmType>(ids);
            }
            public List<AlgorithmType> IDs { get; set; }
            public bool? Enabled { get; set; } = null;
            public string ExtraLaunchParameters { get; set; } = null;
            public ulong? NonDefaultRAMLimit { get; set; } = null;

            public Algorithm ToAlgorithm(string PluginUUID, bool enabled = true, string elp = "")
            {
                var setEnabled = Enabled.HasValue ? Enabled.Value : enabled;
                var setElp = ExtraLaunchParameters != null ? ExtraLaunchParameters : elp;
                var ret = new Algorithm(PluginUUID, IDs.ToArray()) { Enabled = setEnabled, ExtraLaunchParameters = setElp };
                return ret;
            }
        }
    }
}

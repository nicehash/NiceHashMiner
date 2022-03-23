using Newtonsoft.Json;
using NHM.Common.Algorithm;
using NHM.Common.Configs;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [Obsolete("UNUSED. Use algorithm_fees_v2", false)]
        [JsonProperty("algorithm_fees")]
        public Dictionary<AlgorithmType, double> AlgorithmFees { get; set; } = null;
        [JsonProperty("algorithm_fees_v2")]
        public Dictionary<string, double> AlgorithmFeesV2 { get; set; } = null;

        [JsonProperty("device_algorithms")]
        public Dictionary<DeviceType, List<SupportedAlgorithmSettings>> Algorithms { get; set; } = null;

        // for single algos
        [Obsolete("UNUSED", false)]
        [JsonProperty("plugin_algorithm_name")]
        public Dictionary<AlgorithmType, string> AlgorithmNames { get; set; } = null;

        public class SupportedAlgorithmSettings
        {
            public SupportedAlgorithmSettings() { }
            public SupportedAlgorithmSettings(params AlgorithmType[] ids)
            {
                var (name, _) = MinerToolkit.AlgorithmIDsToString(ids);
                IDs = name;
            }
            public string IDs { get; set; }
            public bool? Enabled { get; set; } = null;
            public string ExtraLaunchParameters { get; set; } = null;
            public ulong? NonDefaultRAMLimit { get; set; } = null;

            public (Algorithm algorithm, bool ok) ToAlgorithmV2(string PluginUUID, bool enabled = true, string elp = "")
            {
                var setEnabled = Enabled.HasValue ? Enabled.Value : enabled;
                var setElp = ExtraLaunchParameters != null ? ExtraLaunchParameters : elp;
                var (ids, ok) = MinerToolkit.StringToAlgorithmIDs(IDs);
                var ret = new Algorithm(PluginUUID, ids) { Enabled = setEnabled, ExtraLaunchParameters = setElp };
                return (ret, ok);
            }
        }
    }
}

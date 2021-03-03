using Newtonsoft.Json;
using NHM.Common.Configs;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1.Configs
{
    [Serializable]
    public class MinerApiMaxTimeoutSetting : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("general_timeout")]
        public TimeSpan GeneralTimeout { get; set; } = new TimeSpan(0, 5, 0); // 5 minutes is the default

        [JsonProperty("timeout_per_algorithm")]
        public Dictionary<string, TimeSpan> TimeoutPerAlgorithm { get; set; } = new Dictionary<string, TimeSpan>();

        [JsonProperty("timeout_per_device_type")]
        public Dictionary<DeviceType, TimeSpan> TimeoutPerDeviceType { get; set; } = new Dictionary<DeviceType, TimeSpan>();

        #region Functions/Parsers

        // TODO document/add docs
        public static TimeSpan ParseMaxTimeout(TimeSpan defaultMaxTimeout, MinerApiMaxTimeoutSetting config, IEnumerable<MiningPair> miningPairs)
        {
            if (config?.UseUserSettings ?? false)
            {
                // TimeoutPerDeviceType has #1 priority
                var pairDeviceTypeTimeout = config.TimeoutPerDeviceType;
                var deviceTypes = miningPairs.Select(mp => mp.Device.DeviceType);
                if (pairDeviceTypeTimeout != null)
                {
                    var relevantTimeouts = pairDeviceTypeTimeout.Where(kvp => deviceTypes.Contains(kvp.Key))
                        .OrderBy(kvp => kvp.Value);
                    if (relevantTimeouts.Count() > 0) return relevantTimeouts.First().Value;
                }
                // TimeoutPerAlgorithm has #2 priority
                var pairAlgorithmTimeout = config.TimeoutPerAlgorithm;
                var algorithmName = miningPairs.FirstOrDefault()?.Algorithm?.AlgorithmName ?? "";
                if (pairAlgorithmTimeout != null && !string.IsNullOrEmpty(algorithmName) && pairAlgorithmTimeout.ContainsKey(algorithmName))
                {
                    return pairAlgorithmTimeout[algorithmName];
                }

                return config.GeneralTimeout;
            }
            return defaultMaxTimeout;
        }

        public static bool ParseIsEnabled(bool defaultValue, MinerApiMaxTimeoutSetting config)
        {
            if (config?.UseUserSettings ?? false)
            {
                return config.Enabled;
            }

            return defaultValue;
        }
        #endregion Functions/Parsers
    }
}

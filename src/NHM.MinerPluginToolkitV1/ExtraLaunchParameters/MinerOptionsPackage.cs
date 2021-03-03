using Newtonsoft.Json;
using NHM.Common.Configs;
using System;
using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1.ExtraLaunchParameters
{
    /// <summary>
    /// MinerOptionsPackage combines General and Temperature options (both of type MinerOption<see cref="MinerOption"/>)
    /// With UseUserSettings property user can define if Miner options should be used from local MinerOptionsPackage.json file
    /// </summary>
    [Serializable]
    public class MinerOptionsPackage : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("group_mining_pairs_only_with_compatible_options")]
        public bool GroupMiningPairsOnlyWithCompatibleOptions { get; set; } = true;

        [JsonProperty("ignore_default_value_options")]
        public bool IgnoreDefaultValueOptions { get; set; } = true;

        [JsonProperty("general_options")]
        public List<MinerOption> GeneralOptions { get; set; }

        [JsonProperty("temperature_options")]
        public List<MinerOption> TemperatureOptions { get; set; }
    }
}

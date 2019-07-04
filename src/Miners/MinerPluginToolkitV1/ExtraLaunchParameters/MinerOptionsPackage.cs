using MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.ExtraLaunchParameters
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

        [JsonProperty("general_options")]
        public List<MinerOption> GeneralOptions { get; set; }

        [JsonProperty("temperature_options")]
        public List<MinerOption> TemperatureOptions { get; set; }
    }
}

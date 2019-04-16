using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner
{
    public static class StratumService
    {

        // TODO consider using this instead of int index
        //// EU by default
        public static string SelectedServiceLocation {
            get {
                if (ConfigManager.GeneralConfig.ServiceLocation > MiningLocations.Count || ConfigManager.GeneralConfig.ServiceLocation < 0)
                {
                    return "eu";
                }
                return MiningLocations[ConfigManager.GeneralConfig.ServiceLocation];
            }
        }

        // Constants
        public static IReadOnlyList<string> MiningLocations { get; } =
            new[] { "eu", "usa", "hk", "jp", "in", "br" };

        public static readonly object[] MiningLocationNames = new object[] {
            "Europe - Amsterdam",
            "USA - San Jose",
            "China - Hong Kong",
            "Japan - Tokyo",
            "India - Chennai",
            "Brazil - Sao Paulo"
        };
    }
}

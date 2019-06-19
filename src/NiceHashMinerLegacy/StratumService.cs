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
        public static string SelectedServiceLocation => MiningLocations[_serviceLocation];

        public static int _serviceLocation = 0;
        public static int ServiceLocation
        {
            get => _serviceLocation;
            set
            {
                var newValue = (-1 < value && value < MiningLocations.Count) ? value : 0;
                if (_serviceLocation != newValue)
                {
                    _serviceLocation = newValue;
                    // service location is different and changed execute potential actions
                    ConfigManager.GeneralConfigFileCommit();
                }
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

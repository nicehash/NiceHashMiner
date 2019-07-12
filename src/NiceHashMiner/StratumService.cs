using NiceHashMiner.Configs;
using System.Collections.Generic;

namespace NiceHashMiner
{
    public static class StratumService
    {
        public static string SelectedServiceLocation => MiningLocations[_serviceLocation];
        private static string _lastSelectedServiceLocation = "";

        private static bool _callResume = false;

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
                _lastSelectedServiceLocation = SelectedServiceLocation;
            }
        }

        private class Location
        {
            public Location(int index, string code, string name)
            {
                Index = index;
                Code = code;
                Name = name;
            }
            public int Index { get; }
            public string Code { get; }
            public string Name { get; }
            public bool Enabled { get; set; } = true;
        }

        public static void SetEnabled(bool eu, bool usa)
        {
            var oldStates = new Dictionary<string, bool>();
            foreach (var kvp in _miningLocations)
            {
                oldStates[kvp.Key] = kvp.Value.Enabled;
            }
            foreach (var key in _miningLocationsEU)
            {
                _miningLocations[key].Enabled = eu;
            }
            foreach (var key in _miningLocationsUSA)
            {
                _miningLocations[key].Enabled = usa;
            }
            // determine if there is a change
            var hasChange = false;
            foreach (var kvp in oldStates)
            {
                var key = kvp.Key;
                if (oldStates[key] != _miningLocations[key].Enabled)
                {
                    hasChange = true;
                    break;
                }
            }
            if (hasChange)
            {
                // TODO update GUI
                var lastSelectedServiceLocation = _lastSelectedServiceLocation;
                // TODO check if we must restart mining
                var mustRestart = _miningLocations[SelectedServiceLocation].Enabled == false || _callResume;
                _lastSelectedServiceLocation = SelectedServiceLocation;
                if (mustRestart)
                {
                    var canSwitchMarket = false;
                    // take first market to switch
                    foreach (var key in MiningLocations)
                    {
                        if (_miningLocations[key].Enabled)
                        {
                            canSwitchMarket = true;
                            _serviceLocation = _miningLocations[key].Index;
                            break;
                        }
                    }
                    if (_miningLocations[lastSelectedServiceLocation].Enabled)
                    {
                        canSwitchMarket = true;
                        _serviceLocation = _miningLocations[lastSelectedServiceLocation].Index;
                    }
                    if (canSwitchMarket)
                    {
                        // restart or resume mining
                        ApplicationStateManager.ResumeMiners();
                    }
                    else
                    {
                        _callResume = true;
                        ApplicationStateManager.PauseMiners();
                        // TODO notify GUI 
                    }
                }
                
            }
        }

        // Constants
        private static IReadOnlyDictionary<string, Location> _miningLocations { get; } = new Dictionary<string, Location> {
            { "eu",     new Location(0, "eu", "Europe - Amsterdam") },
            { "usa",    new Location(1, "usa", "USA - San Jose") },
            { "hk",     new Location(2, "hk", "China - Hong Kong") },
            { "jp",     new Location(3, "jp", "Japan - Tokyo") },
            { "in",     new Location(4, "in", "India - Chennai") },
            { "br",     new Location(5, "br", "Brazil - Sao Paulo") },
        };

        private static IReadOnlyList<string> _miningLocationsEU { get; } =
            new[] { "eu" };

        private static IReadOnlyList<string> _miningLocationsUSA { get; } =
            new[] { "usa", "hk", "jp", "in", "br" };

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

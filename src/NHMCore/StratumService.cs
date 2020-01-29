using NHM.Common;
using NHMCore.Configs;
using NHMCore.Notifications;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore
{
    public class StratumService : NotifyChangedBase
    {
        public static StratumService Instance { get; } = new StratumService();

        private StratumService() { }

        public string SelectedServiceLocation => MiningLocations[_serviceLocation];
        public string SelectedFallbackServiceLocation { get; private set; } = null;
        public bool SelectedServiceLocationOperational { get; private set; } = true;
        public bool EU_ServiceLocationOperational { get; private set; } = true;
        public bool USA_ServiceLocationOperational { get; private set; } = true;
        public bool ServiceLocationsNotOperational { get; private set; } = false;

        public int _serviceLocation = 0;
        public int ServiceLocation
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
                OnPropertyChanged(nameof(ServiceLocation));
                OnPropertyChanged(nameof(SelectedServiceLocation));
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

        public void SetEnabled(bool eu, bool usa)
        {
            // backup old states
            var oldStates = new Dictionary<string, bool>();
            foreach (var kvp in _miningLocations)
            {
                oldStates[kvp.Key] = kvp.Value.Enabled;
            }
            // set operational states
            foreach (var key in _miningLocationsEU)
            {
                _miningLocations[key].Enabled = eu;
            }
            foreach (var key in _miningLocationsUSA)
            {
                _miningLocations[key].Enabled = usa;
            }
            // check 
            EU_ServiceLocationOperational = eu;
            USA_ServiceLocationOperational = usa;
            ServiceLocationsNotOperational = !eu && !usa;
            SelectedServiceLocationOperational = _miningLocations[SelectedServiceLocation].Enabled;
            OnPropertyChanged(nameof(EU_ServiceLocationOperational));
            OnPropertyChanged(nameof(USA_ServiceLocationOperational));
            OnPropertyChanged(nameof(ServiceLocationsNotOperational));
            OnPropertyChanged(nameof(SelectedServiceLocationOperational));

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
                if (SelectedServiceLocationOperational)
                {
                    var marketNotifications = NotificationsManager.Instance.Notifications.Where(notif => notif.Group == NotificationsGroup.Market);
                    foreach (var marketNotif in marketNotifications)
                    {
                        NotificationsManager.Instance.RemoveNotificationFromList(marketNotif);
                    }
                    OnPropertyChanged(nameof(SelectedServiceLocation));
                }
                else if (EU_ServiceLocationOperational)
                {
                    AvailableNotifications.CreateUnavailablePrimaryMarketLocationInfo();
                    SelectedFallbackServiceLocation = _miningLocationsEU.FirstOrDefault();
                    OnPropertyChanged(nameof(SelectedFallbackServiceLocation));
                }
                else if (USA_ServiceLocationOperational)
                {
                    AvailableNotifications.CreateUnavailablePrimaryMarketLocationInfo();
                    SelectedFallbackServiceLocation = _miningLocationsUSA.FirstOrDefault();
                    OnPropertyChanged(nameof(SelectedFallbackServiceLocation));
                }
                else
                {
                    // pause mining
                    AvailableNotifications.CreateUnavailableAllMarketsLocationInfo();
                    SelectedFallbackServiceLocation = null;
                    OnPropertyChanged(nameof(SelectedFallbackServiceLocation));
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

        public static IReadOnlyList<string> MiningLocationNames { get; } = new List<string>
        {
            "Europe - Amsterdam",
            "USA - San Jose",
            "China - Hong Kong",
            "Japan - Tokyo",
            "India - Chennai",
            "Brazil - Sao Paulo"
        };
    }
}

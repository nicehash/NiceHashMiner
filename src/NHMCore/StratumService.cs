using NHM.Common;
using NHMCore.Configs;
using NHMCore.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore
{
    public class StratumService : NotifyChangedBase
    {
        public static StratumService Instance { get; } = new StratumService();

        private StratumService() { }

        public event EventHandler<string> OnServiceLocationChanged;

        public bool SelectedServiceLocationOperational => StratumServiceHelpers.MiningServiceLocations[_serviceLocation].IsOperational;
        public bool ServiceLocationsNotOperational => StratumServiceHelpers.MiningServiceLocations.All(location => !location.IsOperational);
        // XAML
        public static IReadOnlyList<StratumServiceHelpers.Location> MiningLocationNames => StratumServiceHelpers.MiningServiceLocations;

        private int _serviceLocation = 0;
        public int ServiceLocation
        {
            get => _serviceLocation;
            set
            {
                var maxIndex = StratumServiceHelpers.MiningServiceLocations.Count - 1;
                var defaultValue = value >= maxIndex ? maxIndex : 0;
                var newValue = (-1 < value && value < StratumServiceHelpers.MiningServiceLocations.Count) ? value : defaultValue;
                if (_serviceLocation != newValue)
                {
                    _serviceLocation = newValue;
                    // service location is different and changed execute potential actions
                    ConfigManager.GeneralConfigFileCommit();
                }
                OnPropertyChanged(nameof(ServiceLocation));
                OnPropertyChanged(nameof(SelectedServiceLocationOperational));
                // update market
                var (serviceLocationCode, _) = SelectedOrFallbackServiceLocationCode();
                // sending null pauses mining
                OnServiceLocationChanged?.Invoke(this, serviceLocationCode);
            }
        }

        public (string miningLocationCode, bool isSelected) SelectedOrFallbackServiceLocationCode()
        {
            var selectedServiceLocationCode = StratumServiceHelpers.MiningServiceLocations[_serviceLocation].Code;
            if (SelectedServiceLocationOperational) return (selectedServiceLocationCode, true);

            var setFallbackLocation = StratumServiceHelpers.MiningServiceLocations
                    .Where(loc => loc.Code != selectedServiceLocationCode)
                    .Where(loc => loc.IsOperational)
                    .OrderByDescending(loc => string.CompareOrdinal(selectedServiceLocationCode, loc.Code))
                    .FirstOrDefault();

            var serviceLocationCode = setFallbackLocation?.Code ?? null;
            return (serviceLocationCode, false);
        }

        public void SetEnabledMarkets(IEnumerable<string> markets)
        {
            // set/update operational markets for ALL locations and determine determine if there is a change
            var hasMarketsChange = StratumServiceHelpers.MiningServiceLocations
                .Select(location => location.SetAndReturnIsOperational(markets))
                .ToArray() // exec ALL
                .Any(p => p.IsOperationalBeforeSet != p.IsOperationalAfterSet);

            if (!hasMarketsChange) return;

            OnPropertyChanged(nameof(MiningLocationNames));
            OnPropertyChanged(nameof(ServiceLocationsNotOperational));
            OnPropertyChanged(nameof(SelectedServiceLocationOperational));


            var (serviceLocationCode, isSelected) = SelectedOrFallbackServiceLocationCode();

            if (isSelected && serviceLocationCode != null)
            {
                var marketNotificationsRemoved = NotificationsManager.Instance.Notifications
                    .Where(notif => notif.Group == NotificationsGroup.Market)
                    .Select(NotificationsManager.Instance.RemoveNotificationFromList)
                    .ToArray()
                    .All(removed => removed);
            }
            else if (!isSelected && serviceLocationCode != null)
            {
                // TODO pass what is the fallback mining location
                AvailableNotifications.CreateUnavailablePrimaryMarketLocationInfo();
            }
            else if (serviceLocationCode == null)
            {
                AvailableNotifications.CreateUnavailableAllMarketsLocationInfo();
            }
            else
            {
                // NEVER
            }

            // sending null pauses mining
            OnServiceLocationChanged?.Invoke(this, serviceLocationCode);
        }
    }
}

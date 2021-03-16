using NHM.Common;
using NHM.Common.Enums;
using NHMCore;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Mining.MiningStats;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Models
{
    /// <summary>
    /// Wrapper for <see cref="ComputeDevice"/> to convert for device status DataGrid
    /// </summary>
    public class DeviceData : NotifyChangedBase
    {
        const string MISSING_INFO = "- - -";
        public ComputeDevice Dev { get; }

        public DeviceMiningStats DeviceMiningStats { get; private set; } = null;
        public string DeviceMiningStatsProfitability { get; private set; } = MISSING_INFO;
        public string DeviceMiningStatsPluginAlgo { get; private set; } = MISSING_INFO;

        public ObservableCollection<AlgorithmContainer> AlgorithmSettingsCollection { get; private set; } = new ObservableCollection<AlgorithmContainer>();

        public bool Enabled
        {
            get => Dev.Enabled;
            // TODO set private set and call an async method here
            set
            {
                ApplicationStateManager.SetDeviceEnabledState(this, (Dev.B64Uuid, value));
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool AllAgorithmsEnabled
        {
            get => Dev.AlgorithmSettings.All(a => a.Enabled);
            // TODO this could be async?
            set
            {
                foreach (var algo in Dev.AlgorithmSettings)
                {
                    algo.Enabled = value;
                }
                OnPropertyChanged();
            }
        }

        public bool CanClearAllSpeeds => !(Dev.State == DeviceState.Benchmarking || Dev.State == DeviceState.Mining);
        public bool CanStopBenchmark => Dev.State == DeviceState.Benchmarking;

        public List<string> AlgoNames { get; private set; }

        // TODO Pending state and error states
        public bool CanStart => Dev.Enabled && Dev.State == DeviceState.Stopped;
        public bool CanStop => Dev.Enabled && (Dev.State == DeviceState.Benchmarking || Dev.State == DeviceState.Mining);

        public string AlgoOptions
        {
            get
            {
                var enabledAlgos = Dev.AlgorithmSettings.Count(a => a.Enabled);
                var benchedAlgos = Dev.AlgorithmSettings.Count(a => !a.BenchmarkNeeded);
                return $"{Dev.AlgorithmSettings.Count} / {enabledAlgos} / {benchedAlgos}";
            }
        }

        public string AlgosEnabled
        {
            get
            {
                var enabledAlgos = Dev.AlgorithmSettings.Count(a => a.Enabled);
                return $"{Dev.AlgorithmSettings.Count} / {enabledAlgos}";
            }
        }

        public string AlgosBenchmarked
        {
            get
            {
                var benchedAlgos = Dev.AlgorithmSettings.Count(a => !a.BenchmarkNeeded);
                return $"{Dev.AlgorithmSettings.Count} / {benchedAlgos}";
            }
        }

        public string ButtonLabel
        {
            get
            {
                // assume disabled
                var buttonLabel = "N/A";
                if (Dev.State == DeviceState.Stopped)
                {
                    buttonLabel = "Start";
                }
                else if (Dev.State == DeviceState.Mining || Dev.State == DeviceState.Benchmarking)
                {
                    buttonLabel = "Stop";
                }
                return Translations.Tr(buttonLabel);
            }
        }

        public DeviceData(ComputeDevice dev)
        {
            AlgoNames = dev.AlgorithmSettings.Select(a => a.AlgorithmName).ToList();
            Dev = dev;

            Dev.PropertyChanged += DevOnPropertyChanged;

            foreach (var algo in Dev.AlgorithmSettings)
            {
                algo.PropertyChanged += AlgoOnPropertyChanged;
            }

            AlgorithmSettingsCollection = new ObservableCollection<AlgorithmContainer>(Dev.AlgorithmSettings);

            MiningDataStats.DevicesMiningStats.CollectionChanged += DevicesMiningStatsOnCollectionChanged;
            RefreshDiag();
        }

        private void DevicesMiningStatsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Logger.Info("DEBUG", $"DevicesMiningStatsOnCollectionChanged {e.Action.ToString()}");
            var oldData = e.OldItems?.OfType<DeviceMiningStats>().FirstOrDefault(d => d.DeviceUUID == Dev.Uuid);
            if (e.NewItems == null && oldData != null)
            {
                DeviceMiningStats = null;
                DeviceMiningStatsProfitability = MISSING_INFO;
                DeviceMiningStatsPluginAlgo = MISSING_INFO;
            }
            else if (e.NewItems != null)
            {
                var data = e.NewItems.OfType<DeviceMiningStats>().FirstOrDefault(d => d.DeviceUUID == Dev.Uuid);
                if (data != null)
                {
                    DeviceMiningStats = data;
                    // in BTC
                    var profitabilityWithCost = 0.0;
                    if (GUISettings.Instance.DisplayPureProfit)
                    {
                        profitabilityWithCost = data?.TotalPayingRateDeductPowerCost(BalanceAndExchangeRates.Instance.GetKwhPriceInBtc()) ?? 0;
                    }
                    else
                    {
                        profitabilityWithCost = data?.TotalPayingRate() ?? 0;
                    }

                    if (GUISettings.Instance.AutoScaleBTCValues && profitabilityWithCost < 0.1)
                    {
                        var scaled = (profitabilityWithCost) * 1000;
                        DeviceMiningStatsProfitability = $"{scaled:F5} mBTC";
                    }
                    else
                    {
                        DeviceMiningStatsProfitability = $"{(profitabilityWithCost):F8} BTC";
                    }
                    // CryptoDredge / Equihash: 1740.77 Sol/s
                    var algoName = string.Join("+", data.Speeds.Select(s => s.type.ToString()));
                    var speedStr = Helpers.FormatSpeedOutput(data.Speeds);
                    DeviceMiningStatsPluginAlgo = $"{data.MinerName} / {algoName}: {speedStr}";
                }
            }
            OnPropertyChanged(nameof(DeviceMiningStats));
            OnPropertyChanged(nameof(DeviceMiningStatsProfitability));
            OnPropertyChanged(nameof(DeviceMiningStatsPluginAlgo));
        }

        private void AlgoOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlgorithmContainer.Enabled) || e.PropertyName == nameof(AlgorithmContainer.BenchmarkNeeded))
            {
                OnPropertyChanged(nameof(AlgoOptions));
                OnPropertyChanged(nameof(AlgosEnabled));
                OnPropertyChanged(nameof(AlgosBenchmarked));
                OnPropertyChanged(nameof(AllAgorithmsEnabled));
            }
        }

        private void DevOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ComputeDevice.AlgorithmSettings):
                    AlgorithmSettingsCollection = new ObservableCollection<AlgorithmContainer>(Dev.AlgorithmSettings);
                    OrderAlgorithms();
                    OnPropertyChanged(nameof(AlgoOptions));
                    OnPropertyChanged(nameof(AlgosEnabled));
                    OnPropertyChanged(nameof(AlgosBenchmarked));
                    // update algorithms event handlers here
                    foreach (var algo in Dev.AlgorithmSettings)
                    {
                        algo.PropertyChanged -= AlgoOnPropertyChanged;
                        algo.PropertyChanged += AlgoOnPropertyChanged;
                    }

                    return;
                default:
                    break;
            }
            if (e.PropertyName == nameof(Dev.State))
            {
                OnPropertyChanged(nameof(ButtonLabel));
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
                OnPropertyChanged(nameof(CanClearAllSpeeds));
                OnPropertyChanged(nameof(CanStopBenchmark));
            }
            else if (e.PropertyName == nameof(Dev.Enabled))
            {
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public float Load { get; private set; } = -1;

        public float Temp { get; private set; } = -1;

        public int _fanSpeed = -1;
        public string FanSpeed
        {
            get
            {
                if (_fanSpeed < 0) return MISSING_INFO;
                return $"{_fanSpeed}%";
            }
        }

        public void RefreshDiag()
        {
            Load = Dev.Load;
            Temp = Dev.Temp;
            _fanSpeed = Dev.FanSpeed;
            OnPropertyChanged(nameof(Load));
            OnPropertyChanged(nameof(Temp));
            OnPropertyChanged(nameof(FanSpeed));
        }

        public async Task StartStopClick()
        {
            switch (Dev.State)
            {
                case DeviceState.Stopped:
                    await ApplicationStateManager.StartSingleDevicePublic(Dev);
                    break;
                case DeviceState.Mining:
                case DeviceState.Benchmarking:
                    await ApplicationStateManager.StopSingleDevicePublic(Dev);
                    break;
            }
        }

        public void ClearAllSpeeds()
        {
            foreach (var a in Dev.AlgorithmSettings)
            {
                a.ClearSpeeds();
            }
        }

        public void EnablebenchmarkedOnly()
        {
            foreach (var a in Dev.AlgorithmSettings)
            {
                a.Enabled = a.HasBenchmark;
            }
        }

        #region AlgorithmSettingsCollection SORTING
        private enum SortColumn
        {
            ALGORITHM = 0,
            PLUGIN,
            SPEEDS,
            PAYING_RATE,
            STATUS,
            ENABLED
        }

        private SortColumn _sortColumn = SortColumn.PLUGIN;
        private bool _sortDescending = false;

        public void OrderAlgorithmsByAlgorithm() => OrderAlgorithmsBy(SortColumn.ALGORITHM);
        public void OrderAlgorithmsByPlugin() => OrderAlgorithmsBy(SortColumn.PLUGIN);
        public void OrderAlgorithmsBySpeeds() => OrderAlgorithmsBy(SortColumn.SPEEDS);
        public void OrderAlgorithmsByPaying() => OrderAlgorithmsBy(SortColumn.PAYING_RATE);
        public void OrderAlgorithmsByStatus() => OrderAlgorithmsBy(SortColumn.STATUS);
        public void OrderAlgorithmsByEnabled() => OrderAlgorithmsBy(SortColumn.ENABLED);

        private void OrderAlgorithmsBy(SortColumn sortByColumn)
        {
            if (_sortColumn == sortByColumn)
            {
                _sortDescending = !_sortDescending;
            }
            else
            {
                _sortColumn = sortByColumn;
                _sortDescending = false;
            }
            OrderAlgorithms();
        }

        private void OrderAlgorithms()
        {
            List<Func<AlgorithmContainer, object>> orderedSortingFunctions = new List<Func<AlgorithmContainer, object>>
            {
                algo => algo.AlgorithmName,
                algo => algo.PluginName,
                algo => algo.BenchmarkSpeed, // FIRST SPEED FIX only
                algo => algo.CurrentEstimatedProfit,
                algo => algo.Status, // TODO STATUS doesn't exist yet
                algo => algo.Enabled,
            };
            // take the first one and order by that first then continue with the rest
            var firstOrder = orderedSortingFunctions[(int)_sortColumn];
            orderedSortingFunctions.RemoveAt((int)_sortColumn);
            IOrderedEnumerable<AlgorithmContainer> ordered;
            if (_sortDescending)
            {
                ordered = AlgorithmSettingsCollection.OrderByDescending(firstOrder);
            }
            else
            {
                ordered = AlgorithmSettingsCollection.OrderBy(firstOrder);
            }

            foreach (var nextOrderBy in orderedSortingFunctions)
            {
                ordered = ordered.ThenBy(nextOrderBy);
            }

            AlgorithmSettingsCollection = new ObservableCollection<AlgorithmContainer>(ordered);
            OnPropertyChanged(nameof(AlgorithmSettingsCollection));
        }


        #endregion AlgorithmSettingsCollection SORTING

        public static implicit operator DeviceData(ComputeDevice dev)
        {
            return new DeviceData(dev);
        }
    }
}

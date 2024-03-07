﻿using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHMCore;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Mining.MiningStats;
using NHMCore.Nhmws.V4;
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
        public List<ComputeDevice> CPUs
        {
            get => AvailableDevices.Devices
                .Where(dev => dev.DeviceType == DeviceType.CPU)
                .Where(dev => dev.Uuid != Dev.Uuid)
                .ToList();
        }

        public List<ComputeDevice> GPUs
        {
            get => AvailableDevices.Devices
                .Where(dev => dev.DeviceType != DeviceType.CPU)
                .Where(dev => dev.Uuid != Dev.Uuid)
                .ToList();
        }

        public List<ComputeDevice> DevicesOfSameType
        {
            get
            {
                return Dev.DeviceType == DeviceType.CPU ? CPUs : GPUs.Where(dev => dev.DeviceType == Dev.DeviceType).ToList();
            }
        }


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
                    //algo.Enabled = value;
                    algo.SetEnabled(value);
                }
                OnPropertyChanged();
                ConfigManager.CommitBenchmarksForDevice(Dev);
                Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
            }
        }

#if NHMWS4
        public bool CanClearAllSpeeds => !(Dev.State == DeviceState.Benchmarking || Dev.State == DeviceState.Mining || Dev.State == DeviceState.Testing);
        public bool CanStopBenchmark => Dev.Enabled && (Dev.State == DeviceState.Benchmarking || Dev.State == DeviceState.Testing);
        public bool CanStopMining => Dev.Enabled && (Dev.State == DeviceState.Mining || Dev.State == DeviceState.Testing);//problem? if testing running  from bench or from mining
#else
        public bool CanClearAllSpeeds => !(Dev.State == DeviceState.Benchmarking || Dev.State == DeviceState.Mining);
        public bool CanStopBenchmark => Dev.Enabled && Dev.State == DeviceState.Benchmarking;
        public bool CanStopMining => Dev.Enabled && Dev.State == DeviceState.Mining;
#endif
        public bool CanCopyFromOtherDevices => AvailableDevices.Devices.Count(dev => dev.DeviceType == Dev.DeviceType) > 1 && CanClearAllSpeeds;


        public List<string> AlgoNames { get; private set; }

        // TODO Pending state and error states
        public bool CanStart => Dev.Enabled && Dev.State == DeviceState.Stopped;
#if NHMWS4
        public bool CanStop => Dev.Enabled && (Dev.State == DeviceState.Benchmarking || Dev.State == DeviceState.Mining || Dev.State == DeviceState.Testing);
#else
        public bool CanStop => Dev.Enabled && (Dev.State == DeviceState.Benchmarking || Dev.State == DeviceState.Mining);
#endif

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
                return $"{enabledAlgos} / {Dev.AlgorithmSettings.Count}";
            }
        }

        public string AlgosBenchmarked
        {
            get
            {
                var benchedAlgos = Dev.AlgorithmSettings.Count(a => !a.BenchmarkNeeded);
                return $"{benchedAlgos} / {Dev.AlgorithmSettings.Count}";
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
#if NHMWS4
                else if (Dev.State == DeviceState.Mining || Dev.State == DeviceState.Benchmarking || Dev.State == DeviceState.Testing)
#else
                else if (Dev.State == DeviceState.Mining || Dev.State == DeviceState.Benchmarking)
#endif
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
            if (e.PropertyName == nameof(ComputeDevice.AlgorithmSettings))
            {
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
            }
            else if (e.PropertyName == nameof(Dev.State))
            {
                OnPropertyChanged(nameof(ButtonLabel));
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
                OnPropertyChanged(nameof(CanClearAllSpeeds));
                OnPropertyChanged(nameof(CanStopBenchmark));
                OnPropertyChanged(nameof(CanStopMining));
                OnPropertyChanged(nameof(CanCopyFromOtherDevices));
                OnPropertyChanged(nameof(DevicesOfSameType));
            }
            else if (e.PropertyName == nameof(Dev.Enabled))
            {
                OnPropertyChanged(nameof(Enabled));
            }
        }


        public float Load { get; private set; } = -1;

        public float Temp { get; private set; } = -1;

        public int _fanSpeed = -1;

        private bool _isRPM = false;

        public string FanSpeed
        {
            get
            {
                if (_fanSpeed < 0) return MISSING_INFO;
                if(!_isRPM) return $"{_fanSpeed}%";
                return $"{_fanSpeed}RPM";
            }
        }

        public void RefreshDiag()
        {
            Load = Dev.Load;
            Temp = Dev.Temp;
            _isRPM = -1 == Dev.FanSpeed;
            _fanSpeed = _isRPM ? Dev.FanSpeedRPM : Dev.FanSpeed;
            OnPropertyChanged(nameof(Load));
            OnPropertyChanged(nameof(Temp));
            OnPropertyChanged(nameof(FanSpeed));
        }

        public void ClearAllSpeeds()
        {
            foreach (var a in Dev.AlgorithmSettings)
            {
                a.ClearSpeeds();
            }
            ConfigManager.CommitBenchmarksForDevice(Dev);
        }

        public void CopySettingsFromAnotherDevice(ComputeDevice source)
        {
            foreach (var algoSource in source.AlgorithmSettings)
            {
                var algoDestination = Dev.AlgorithmSettings.FirstOrDefault(algo => algo.AlgorithmStringID == algoSource.AlgorithmStringID);
                if (algoDestination == null) continue;
                algoDestination.BenchmarkSpeed = algoSource.BenchmarkSpeed;
                algoDestination.SecondaryBenchmarkSpeed = algoSource.SecondaryBenchmarkSpeed;
                algoDestination.PowerUsage = algoSource.PowerUsage;
            }
            ConfigManager.CommitBenchmarksForDevice(Dev);
        }

        public void EnableBenchmarkedOnly()
        {
            foreach (var a in Dev.AlgorithmSettings)
            {
                a.SetEnabled(a.HasBenchmark);
            }
            OnPropertyChanged();
            ConfigManager.CommitBenchmarksForDevice(Dev);
            Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
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
                algo => algo.CurrentEstimatedProfitPure,
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

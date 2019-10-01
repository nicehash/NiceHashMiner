using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Mining;
using NHMCore.Stats;
using NHMCore.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NHM.Wpf.ViewModels.Models
{
    /// <summary>
    /// Wrapper for <see cref="ComputeDevice"/> and <see cref="MiningStats.DeviceMiningStats"/> to convert for mining stats ListView
    /// </summary>
    public class MiningData : NotifyChangedBase, IMiningData
    {
        public ComputeDevice Dev { get; }

        public string Name => Dev.FullName;

        private MiningStats.DeviceMiningStats _stats;
        public MiningStats.DeviceMiningStats Stats
        {
            get => _stats;
            set
            {
                _stats = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Hashrate));
                OnPropertyChanged(nameof(Payrate));
                OnPropertyChanged(nameof(StateName));
                OnPropertyChanged(nameof(Speeds));
                OnPropertyChanged(nameof(FiatPayrate));
                OnPropertyChanged(nameof(PowerUsage));
                OnPropertyChanged(nameof(PowerCost));
                OnPropertyChanged(nameof(Profit));
            }
        }

        public double Hashrate => Stats?.Speeds?.Count > 0 ? Stats.Speeds[0].speed : 0;

        public IEnumerable<Hashrate> Speeds => Stats?.Speeds?.Select(s => (Hashrate) s);

        public double Payrate => TimeFactor.ConvertFromDay((Stats?.TotalPayingRate() ?? 0) * 1000);

        public double FiatPayrate => ExchangeRateApi.ConvertFromBtc(Payrate / 1000);

        public double PowerUsage => Stats?.GetPowerUsage() ?? 0;

        public double PowerCost
        {
            get
            {
                var cost = Stats?.PowerCost(ExchangeRateApi.GetKwhPriceInBtc()) ?? 0;
                return TimeFactor.ConvertFromDay(ExchangeRateApi.ConvertFromBtc(cost));
            }
        }

        public double Profit
        {
            get
            {
                var cost = Stats?.TotalPayingRateDeductPowerCost(ExchangeRateApi.GetKwhPriceInBtc()) ?? 0;
                return TimeFactor.ConvertFromDay(ExchangeRateApi.ConvertFromBtc(cost));
            }
        }

        public string StateName
        {
            get
            {
                if (Stats == null) return Dev.State.ToString();

                var algorithmTypes = Stats.Speeds.Select(pair => pair.type).ToArray();
                var algoName = algorithmTypes.GetNameFromAlgorithmTypes();

                return $"{algoName} ({Stats.MinerName})";
            }
        }

        public MiningData(ComputeDevice dev)
        {
            Dev = dev;

            Dev.PropertyChanged += DevOnPropertyChanged;
        }

        private void DevOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dev.State))
                OnPropertyChanged(nameof(StateName));
        }
    }
}

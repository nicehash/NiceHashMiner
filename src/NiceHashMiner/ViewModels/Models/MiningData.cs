using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Mining.MiningStats;
using NHMCore.Nhmws;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NiceHashMiner.ViewModels.Models
{
    /// <summary>
    /// Wrapper for <see cref="ComputeDevice"/> and <see cref="MiningStats.DeviceMiningStats"/> to convert for mining stats ListView
    /// </summary>
    public class MiningData : NotifyChangedBase
    {
        public ComputeDevice Dev { get; }

        public string Name => Dev.FullName;

        private DeviceMiningStats _stats;
        public DeviceMiningStats Stats
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

        public IEnumerable<Hashrate> Speeds => Stats?.Speeds?.Select(s => (Hashrate)s);

        public double Payrate
        {
            get
            {
                if (GUISettings.Instance.DisplayPureProfit)
                {
                    return TimeFactor.ConvertFromDay((Stats?.TotalPayingRateDeductPowerCost(BalanceAndExchangeRates.Instance.GetKwhPriceInBtc()) ?? 0) * 1000);
                }
                else
                {
                    return TimeFactor.ConvertFromDay((Stats?.TotalPayingRate() ?? 0) * 1000);
                }
            }
        }


        public double FiatPayrate => BalanceAndExchangeRates.Instance.ConvertFromBtc(Payrate / 1000);

        public double PowerUsage => Stats?.GetPowerUsage() ?? 0;

        public double PowerCost
        {
            get
            {
                var cost = Stats?.PowerCost(BalanceAndExchangeRates.Instance.GetKwhPriceInBtc()) ?? 0;
                return TimeFactor.ConvertFromDay(BalanceAndExchangeRates.Instance.ConvertFromBtc(cost));
            }
        }

        public double Profit
        {
            get
            {
                var cost = Stats?.TotalPayingRateDeductPowerCost(BalanceAndExchangeRates.Instance.GetKwhPriceInBtc()) ?? 0;
                return TimeFactor.ConvertFromDay(BalanceAndExchangeRates.Instance.ConvertFromBtc(cost));
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

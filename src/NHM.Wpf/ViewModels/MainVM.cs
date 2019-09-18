using NHM.Common;
using NHM.Wpf.ViewModels.Models;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Stats;
using NHMCore.Switching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using NHM.Common.Enums;
using NHMCore.Mining.IdleChecking;

namespace NHM.Wpf.ViewModels
{
    public class MainVM : BaseVM
    {
        private readonly Timer _updateTimer;

        // For syncing mining data listview collection
        private readonly object _lock = new object();

        private IEnumerable<DeviceData> _devices;
        public IEnumerable<DeviceData> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IMiningData> _miningDevs;
        public ObservableCollection<IMiningData> MiningDevs
        {
            get => _miningDevs;
            set
            {
                _miningDevs = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Elements of <see cref="MiningDevs"/> that represent actual devices (i.e. not total rows) and
        /// are in the mining state.
        /// </summary>
        private IEnumerable<MiningData> WorkingMiningDevs =>
            MiningDevs?.OfType<MiningData>().Where(d => d.Dev.State == DeviceState.Mining);

        public IReadOnlyList<string> ServiceLocations => StratumService.MiningLocationNames;

        public int ServiceLocationIndex
        {
            get => ConfigManager.GeneralConfig.ServiceLocation;
            set => ConfigManager.GeneralConfig.ServiceLocation = value;
        }

        public string BtcAddress
        {
            get => ConfigManager.GeneralConfig.BitcoinAddress;
            set => ConfigManager.GeneralConfig.BitcoinAddress = value;
        }

        public string WorkerName
        {
            get => ConfigManager.GeneralConfig.WorkerName;
            set => ConfigManager.GeneralConfig.WorkerName = value;
        }

        public MiningState State => MiningState.Instance;

        #region Currency-related properties

        // TODO this section getting rather large, maybe good idea to break out into own class

        private string _timeUnit = TimeFactor.UnitType.ToString();

        private string TimeUnit
        {
            get => _timeUnit;
            set
            {
                _timeUnit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PerTime));
                OnPropertyChanged(nameof(CurrencyPerTime));
                OnPropertyChanged(nameof(BtcPerTime));
                OnPropertyChanged(nameof(MBtcPerTime));
                OnPropertyChanged(nameof(ProfitPerTime));
            }
        }

        private string PerTime => $"/{TimeUnit}";

        private string _currency = ExchangeRateApi.ActiveDisplayCurrency;

        public string Currency
        {
            get => _currency;
            set
            {
                _currency = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrencyPerTime));
                OnPropertyChanged(nameof(ProfitPerTime));
                OnPropertyChanged(nameof(ExchangeTooltip));
            }
        }

        public string ExchangeTooltip => $"1 BTC = {ExchangeRateApi.SelectedCurrBtcRate:F2} {Currency}";

        public string CurrencyPerTime => $"{Currency}{PerTime}";

        public string BtcPerTime => $"BTC{PerTime}";

        public string MBtcPerTime => $"m{BtcPerTime}";

        private string _scaledBtcPerTime;
        public string ScaledBtcPerTime
        {
            get => _scaledBtcPerTime;
            set
            {
                if (_scaledBtcPerTime == value) return;
                _scaledBtcPerTime = value;
                OnPropertyChanged();
            }
        }

        private string _scaledBtc = "BTC";
        public string ScaledBtc
        {
            get => _scaledBtc;
            set
            {
                if (_scaledBtc == value) return;
                _scaledBtc = value;
                OnPropertyChanged();
            }
        }

        public string ProfitPerTime => $"Profit ({CurrencyPerTime})";

        public double GlobalRate
        {
            get
            {
                // sum is in mBTC already
                var sum = WorkingMiningDevs?.Sum(d => d.Payrate) ?? 0;
                var scale = 1000;
                if (ConfigManager.GeneralConfig.AutoScaleBTCValues && sum < 100)
                {
                    ScaledBtcPerTime = MBtcPerTime;
                    scale = 1;
                }
                else
                {
                    ScaledBtcPerTime = BtcPerTime;
                }

                return sum / scale;
            }
        }

        public double GlobalRateFiat => WorkingMiningDevs?.Sum(d => d.FiatPayrate) ?? 0;

        private double _btcBalance;
        private double BtcBalance
        {
            get => _btcBalance;
            set
            {
                _btcBalance = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FiatBalance));
                OnPropertyChanged(nameof(ScaledBtcBalance));
            }
        }

        public double ScaledBtcBalance
        {
            get
            {
                var scale = 1;
                if (ConfigManager.GeneralConfig.AutoScaleBTCValues && _btcBalance < 0.1)
                {
                    scale = 1000;
                    ScaledBtc = "mBTC";
                }
                else
                {
                    ScaledBtc = "BTC";
                }

                return _btcBalance * scale;
            }
        }

        public double FiatBalance => ExchangeRateApi.ConvertFromBtc(BtcBalance);

        #endregion

        public MainVM()
            : base(ApplicationStateManager.Title)
        {
            _updateTimer = new Timer(1000);
            _updateTimer.Elapsed += UpdateTimerOnElapsed;

            ExchangeRateApi.CurrencyChanged += (_, curr) =>
            {
                Currency = curr;
                OnPropertyChanged(nameof(FiatBalance));
            };
            ExchangeRateApi.ExchangeChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(ExchangeTooltip));
            };

            ApplicationStateManager.DisplayBTCBalance += UpdateBalance;

            TimeFactor.OnUnitTypeChanged += (_, unit) => { TimeUnit = unit.ToString(); };
        }

        // TODO I don't like this way, a global refresh and notify would be better
        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (Devices == null) return;
            foreach (var dev in Devices)
            {
                dev.RefreshDiag();
            }
        }

        public async Task InitializeNhm(IStartupLoader sl)
        {
            await ApplicationStateManager.InitializeManagersAndMiners(sl);

            Devices = new ObservableCollection<DeviceData>(AvailableDevices.Devices.Select(d => (DeviceData) d));
            MiningDevs = new ObservableCollection<IMiningData>(AvailableDevices.Devices.Select(d => new MiningData(d)));

            // This will sync updating of MiningDevs from different threads. Without this, NotifyCollectionChanged doesn't work.
            BindingOperations.EnableCollectionSynchronization(MiningDevs, _lock);

            MiningStats.DevicesMiningStats.CollectionChanged += DevicesMiningStatsOnCollectionChanged;

            IdleCheckManager.StartIdleCheck();

            _updateTimer.Start();

            if (ConfigManager.GeneralConfig.AutoStartMining)
                await StartMining();
        }

        // This complicated callback will add in total rows to mining stats ListView if they are needed.
        private void DevicesMiningStatsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    foreach (var stat in e.NewItems.OfType<MiningStats.DeviceMiningStats>())
                    {
                        // Update this device row
                        var miningDev = MiningDevs.OfType<MiningData>().FirstOrDefault(d => d.Dev.Uuid == stat.DeviceUUID);
                        if (miningDev == null) continue;

                        miningDev.Stats = stat;

                        // Check for existing total row
                        var totalRow = MiningDevs.OfType<TotalMiningData>().FirstOrDefault(d => d.StateName == miningDev.StateName);
                        if (totalRow != null)
                        {
                            totalRow.AddDevice(miningDev);
                            continue;
                        }

                        // Else add new total row
                        totalRow = new TotalMiningData(miningDev);
                        lock (_lock)
                        {
                            MiningDevs.Add(totalRow);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    var toRemove = new List<TotalMiningData>();

                    foreach (var miningDev in MiningDevs)
                    {
                        if (miningDev is MiningData data)
                            data.Stats = null;
                        else if (miningDev is TotalMiningData total)
                            toRemove.Add(total);
                    }

                    foreach (var remove in toRemove)
                    {
                        MiningDevs.Remove(remove);
                        remove.Dispose();
                    }

                    break;
            }

            OnPropertyChanged(nameof(GlobalRate));
            OnPropertyChanged(nameof(GlobalRateFiat));
        }

        private void UpdateBalance(object sender, double btcBalance)
        {
            BtcBalance = btcBalance;
        }

        public async Task StartMining()
        {
            if (!await NHSmaData.WaitOnDataAsync(10)) return;

            // TODO there is a mess of blocking and not-awaited async code down the line, 
            // Just wrapping with Task.Run here for now

            await Task.Run(() => { ApplicationStateManager.StartAllAvailableDevices(); });
        }

        public async Task StopMining()
        {
            // TODO same as StartMining comment
            await Task.Run(() => { ApplicationStateManager.StopAllDevice(); });
        }
    }
}

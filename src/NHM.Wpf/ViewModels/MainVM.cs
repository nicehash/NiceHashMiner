using NHM.Common;
using NHM.Common.Enums;
using NHM.Wpf.ViewModels.Models;
using NHM.Wpf.ViewModels.Plugins;
using NHM.Wpf.ViewModels.Settings;
using NHMCore;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Configs.Data;
using NHMCore.Mining;
using NHMCore.Mining.IdleChecking;
using NHMCore.Mining.MiningStats;
using NHMCore.Mining.Plugins;
using NHMCore.Stats;
using NHMCore.Switching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;

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
                OnPropertyChanged(nameof(DeviceGPUCount));
                OnPropertyChanged(nameof(DeviceCPUCount));
                OnPropertyChanged(nameof(PerDeviceDisplayString));
            }
        }

        public int DeviceGPUCount => _devices?.Where(d => d.Dev.DeviceType != DeviceType.CPU).Count() ?? 0;
        public int DeviceCPUCount => _devices?.Where(d => d.Dev.DeviceType == DeviceType.CPU).Count() ?? 0;

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

        #region settingsLists

        public IReadOnlyList<string> ServiceLocations => StratumService.MiningLocationNames;
        public IEnumerable<TimeUnitType> TimeUnits => GetEnumValues<TimeUnitType>();
        public IEnumerable<string> LanguageOptions => Translations.GetAvailableLanguagesNames();
        public IReadOnlyList<string> ThemeOptions => _themeList;

        public int SelectedLangIndex
        {
            get => Translations.GetCurrentIndex();
            set
            {
                var code = Translations.GetLanguageCodeFromIndex(value);
                Translations.SelectedLanguage = code;
                GeneralConfig.Language = code;
            }
        }

        private List<string> _themeList = new List<string>{ "Light", "Dark" };

        #endregion settingsLists


        public string PerDeviceDisplayString => $"/ {_devices?.Count() ?? 0}";
        
        public static BalanceAndExchangeRates BalanceAndExchangeRates => BalanceAndExchangeRates.Instance;

        public static MiningState MiningState => MiningState.Instance;

        // TODO don't expose GeneralConfig, make it internal to NHMCore
        public static GeneralConfig GeneralConfig => ConfigManager.GeneralConfig;

        private string _theme = ConfigManager.GeneralConfig.DisplayTheme;
        public string Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                OnPropertyChanged();
            }
        }


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

        private string PerTime => $" / {TimeUnit}";

        // TODO get rif of duplicates
        public string Currency
        {
            get => BalanceAndExchangeRates.SelectedFiatCurrency;
            set
            {
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrencyPerTime));
                OnPropertyChanged(nameof(ProfitPerTime));
                OnPropertyChanged(nameof(ExchangeTooltip));
            }
        }

        public string ExchangeTooltip => $"1 BTC = {BalanceAndExchangeRates.SelectedCurrBtcRate:F2} {Currency}";

        public string CurrencyPerTime => $"{BalanceAndExchangeRates.SelectedFiatCurrency}{PerTime}";

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

        public double FiatBalance => BalanceAndExchangeRates.Instance.ConvertFromBtc(BtcBalance);

        #endregion

        #region MinerPlugins
        public ObservableCollection<PluginEntryVM> Plugins { get; } = new ObservableCollection<PluginEntryVM>();
        public void RefreshPlugins()
        {
            foreach (var plugin in MinerPluginsManager.RankedPlugins)
            {
                var vm = Plugins.FirstOrDefault(pluginVM => pluginVM.Plugin.PluginUUID == plugin.PluginUUID);
                if (vm != null) continue;
                Plugins.Add(new PluginEntryVM(plugin));
            }
        }
        #endregion MinerPlugins

        public MainVM()
            : base(ApplicationStateManager.Title)
        {
            _updateTimer = new Timer(1000);
            _updateTimer.Elapsed += UpdateTimerOnElapsed;

            BalanceAndExchangeRates.OnExchangeUpdate += (_, __) =>
            {
                OnPropertyChanged(nameof(ExchangeTooltip));
            };
            BalanceAndExchangeRates.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(BalanceAndExchangeRates.BtcBalance))
                {
                    BtcBalance = BalanceAndExchangeRates.Instance.BtcBalance ?? 0;
                }
                if (e.PropertyName == nameof(BalanceAndExchangeRates.SelectedFiatCurrency))
                {
                    Currency = BalanceAndExchangeRates.Instance.SelectedFiatCurrency;
                    OnPropertyChanged(nameof(FiatBalance));
                }
            };

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
            MiningDataStats.DevicesMiningStats.CollectionChanged += DevicesMiningStatsOnCollectionChanged;

            IdleCheckManager.StartIdleCheck();

            RefreshPlugins();

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
                    foreach (var stat in e.NewItems.OfType<DeviceMiningStats>())
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

        public async Task StartMining()
        {
            if (!await NHSmaData.WaitOnDataAsync(10)) return;

            // this underlying comment shouldn't be true anymore 
            // TODO there is a mess of blocking and not-awaited async code down the line, 
            // Just wrapping with Task.Run here for now

            ApplicationStateManager.StartAllAvailableDevices();
            await ApplicationStateManager.StartMiningTaskWait();
        }

        public async Task StopMining()
        {
            // TODO same as StartMining comment
            await ApplicationStateManager.StopAllDevice();
        }
    }
}

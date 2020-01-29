using NHM.Common;
using NHM.Common.Enums;
using NiceHashMiner.ViewModels.Models;
using NiceHashMiner.ViewModels.Plugins;
using NHMCore;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Mining.IdleChecking;
using NHMCore.Mining.MiningStats;
using NHMCore.Mining.Plugins;
using NHMCore.Nhmws;
using NHMCore.Notifications;
using NHMCore.Switching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;

namespace NiceHashMiner.ViewModels
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
                OnPropertyChanged(nameof(CPUs));
                OnPropertyChanged(nameof(GPUs));
            }
        }

        private IEnumerable<DeviceDataTDP> _devicesTDP;
        public IEnumerable<DeviceDataTDP> DevicesTDP
        {
            get => _devicesTDP;
            set
            {
                _devicesTDP = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<DeviceData> CPUs
        {
            get => _devices?.Where(d => d.Dev.DeviceType == DeviceType.CPU) ?? Enumerable.Empty<DeviceData>();
        }

        public IEnumerable<DeviceData> GPUs
        {
            get => _devices?.Where(d => d.Dev.DeviceType != DeviceType.CPU) ?? Enumerable.Empty<DeviceData>();
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

        public IEnumerable<TimeUnitType> TimeUnits => GetEnumValues<TimeUnitType>();


        public IReadOnlyList<string> ThemeOptions => _themeList;
        private List<string> _themeList = new List<string>{ "Light", "Dark" };

        #endregion settingsLists


        public string PerDeviceDisplayString => $"/ {_devices?.Count() ?? 0}";

        public DashboardViewModel Dashboard { get; } = new DashboardViewModel();

        #region Exposed settings
        public BalanceAndExchangeRates BalanceAndExchangeRates => BalanceAndExchangeRates.Instance;
        public MiningState MiningState => MiningState.Instance;
        public StratumService StratumService => StratumService.Instance;
        public CredentialsSettings CredentialsSettings => CredentialsSettings.Instance;
        public GlobalDeviceSettings GlobalDeviceSettings => GlobalDeviceSettings.Instance;
        public GUISettings GUISettings => GUISettings.Instance;
        public IdleMiningSettings IdleMiningSettings => IdleMiningSettings.Instance;
        public IFTTTSettings IFTTTSettings => IFTTTSettings.Instance;
        public LoggingDebugConsoleSettings LoggingDebugConsoleSettings => LoggingDebugConsoleSettings.Instance;
        public MiningProfitSettings MiningProfitSettings => MiningProfitSettings.Instance;
        public MiningSettings MiningSettings => MiningSettings.Instance;
        public MiscSettings MiscSettings => MiscSettings.Instance;
        public SwitchSettings SwitchSettings => SwitchSettings.Instance;
        public ToSSetings ToSSetings => ToSSetings.Instance;
        public TranslationsSettings TranslationsSettings => TranslationsSettings.Instance;
        public WarningSettings WarningSettings => WarningSettings.Instance;

        public UpdateSettings UpdateSettings => UpdateSettings.Instance;

        public EthlargementIntegratedPlugin EthlargementIntegratedPlugin => EthlargementIntegratedPlugin.Instance;
        #endregion Exposed settings


        #region HelpNotifications
        private ObservableCollection<Notification> _helpNotificationList;
        public ObservableCollection<Notification> HelpNotificationList
        {
            get => _helpNotificationList;
            set
            {
                _helpNotificationList = value;
                OnPropertyChanged();
            }
        }

        private void RefreshNotifications_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lock (_lock)
            {
                // TODO keep it like this for now but update the collection view in the future
                HelpNotificationList = new ObservableCollection<Notification>(NotificationsManager.Instance.Notifications);
                OnPropertyChanged(nameof(HelpNotificationList));
            }
        }

        #endregion HelpNotifications

        // TODO these versions here will not work
        public string LocalVersion => VersionState.Instance.ProgramVersion.ToString();
        public string OnlineVersion => VersionState.Instance.OnlineVersion?.ToString() ?? "N/A";

        #region Currency-related properties

        // TODO this section getting rather large, maybe good idea to break out into own class

        private string _timeUnit = TimeFactor.UnitType.ToString();
        public string TimeUnit
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

        public string GlobalRate
        {
            get
            {
                // sum is in mBTC already
                var sum = WorkingMiningDevs?.Sum(d => d.Payrate) ?? 0;
                var scale = 1000;
                if (GUISettings.Instance.AutoScaleBTCValues && sum < 100)
                {
                    ScaledBtcPerTime = MBtcPerTime;
                    scale = 1;
                    var retScaled = $"{(sum / scale):F5}";
                    return retScaled;
                }
                ScaledBtcPerTime = BtcPerTime;
                var ret = $"{(sum / scale):F8}";
                return ret;
            }
        }

        public string GlobalRateFiat => $"≈ {(WorkingMiningDevs?.Sum(d => d.FiatPayrate) ?? 0):F2} {BalanceAndExchangeRates.SelectedFiatCurrency}";

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
                if (GUISettings.Instance.AutoScaleBTCValues && _btcBalance < 0.1)
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
        private ObservableCollection<PluginEntryVM> _plugins;
        public ObservableCollection<PluginEntryVM> Plugins
        {
            get => _plugins;
            set
            {
                _plugins = value;
                OnPropertyChanged();
            }
        }

        private void MinerPluginsManagerState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lock (_lock)
            {
                if (Plugins == null) return; 
                var rankedPlugins = MinerPluginsManagerState.Instance.RankedPlugins;
                var rankedPluginsArray = rankedPlugins.ToArray();
                // add new
                foreach (var plugin in rankedPluginsArray)
                {
                    var vm = Plugins.FirstOrDefault(pluginVM => pluginVM.Plugin.PluginUUID == plugin.PluginUUID);
                    if (vm != null) continue;
                    Plugins.Add(new PluginEntryVM(plugin));
                }
                // remove missing
                var remove = Plugins.Where(plugin => rankedPlugins.FirstOrDefault(rankedPlugin => rankedPlugin.PluginUUID == plugin.Plugin.PluginUUID) == null).ToArray();
                foreach (var rem in remove)
                {
                    Plugins.Remove(rem);
                }
                // sort
                var removeUUIDs = remove.Select(rem => rem.Plugin.PluginUUID);
                var sorted = rankedPlugins.Where(rankedPlugin => !removeUUIDs.Contains(rankedPlugin.PluginUUID)).ToList();
                var pluginsToSort = Plugins.ToList();
                for (int i = 0; i < sorted.Count; i++)
                {
                    var oldIndex = pluginsToSort.FindIndex(p => p.Plugin == sorted[i]); 
                    Plugins.Move(oldIndex, i);
                }
            }
        }

        #endregion MinerPlugins


        public BenchmarkViewModel BenchmarkSettings { get; } = new BenchmarkViewModel();
        public DevicesViewModel DevicesViewModel { get; } = new DevicesViewModel();

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

            VersionState.Instance.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(VersionState.OnlineVersion))
                {
                    OnPropertyChanged(nameof(OnlineVersion));
                }
            };

            TimeFactor.OnUnitTypeChanged += (_, unit) => { TimeUnit = unit.ToString(); };

            //MinerPluginsManager.OnCrossReferenceInstalledWithOnlinePlugins += OnCrossReferenceInstalledWithOnlinePlugins;
            MinerPluginsManagerState.Instance.PropertyChanged += MinerPluginsManagerState_PropertyChanged;
            NotificationsManager.Instance.PropertyChanged += RefreshNotifications_PropertyChanged;
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
            Plugins = new ObservableCollection<PluginEntryVM>();
            HelpNotificationList = new ObservableCollection<Notification>();
            await ApplicationStateManager.InitializeManagersAndMiners(sl);

            Devices = new ObservableCollection<DeviceData>(AvailableDevices.Devices.Select(d => (DeviceData) d));
            DevicesTDP = new ObservableCollection<DeviceDataTDP>(AvailableDevices.Devices.Select(d => new DeviceDataTDP(d)));
            MiningDevs = new ObservableCollection<IMiningData>(AvailableDevices.Devices.Select(d => new MiningData(d)));

            // This will sync updating of MiningDevs from different threads. Without this, NotifyCollectionChanged doesn't work.
            BindingOperations.EnableCollectionSynchronization(MiningDevs, _lock);
            BindingOperations.EnableCollectionSynchronization(Plugins, _lock);
            BindingOperations.EnableCollectionSynchronization(HelpNotificationList, _lock);
            MiningDataStats.DevicesMiningStats.CollectionChanged += DevicesMiningStatsOnCollectionChanged;

            IdleCheckManager.StartIdleCheck();

            //RefreshPlugins();

            _updateTimer.Start();

            ConfigManager.CreateBackup();

            if (MiningSettings.Instance.AutoStartMining)
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
            await ApplicationStateManager.StartAllAvailableDevicesTask();


            ////test delete after
            //NotificationsManager.Instance.AddNotificationToList(new Notification("start mining", "device started mining"));
        }

        public async Task StopMining()
        {
            await ApplicationStateManager.StopAllDevicesTask();

            ////test delete after
            //NotificationsManager.Instance.AddNotificationToList(new Notification("stop mining", "device stopped mining"));
        }
    }
}

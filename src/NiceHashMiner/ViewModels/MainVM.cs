using NHM.Common;
using NHM.Common.Enums;
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
using NiceHashMiner.ViewModels.Models;
using NiceHashMiner.ViewModels.Plugins;
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

        public int DeviceGPUCount => GPUs.Count();
        public int DeviceCPUCount => CPUs.Count();

        private ObservableCollection<MiningData> _miningDevs;
        public ObservableCollection<MiningData> MiningDevs
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
        private List<string> _themeList = new List<string> { "Light", "Dark" };

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
            }
        }

        private string PerTime => Translations.Tr($" / {TimeUnit}");

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

        public string GlobalRate
        {
            get
            {
                // sum is in mBTC already
                var sum = WorkingMiningDevs?.Sum(d => d.Payrate) ?? 0;
                var scale = 1000;
                if (GUISettings.Instance.AutoScaleBTCValues && sum < 100)
                {
                    ScaledBtcPerTime = $"mBTC{PerTime}";
                    scale = 1;
                    var retScaled = $"{(sum / scale):F5}";
                    return retScaled;
                }
                ScaledBtcPerTime = $"BTC{PerTime}";
                var ret = $"{(sum / scale):F8}";
                return ret;
            }
        }

        public string GlobalRateFiat => $"≈ {(WorkingMiningDevs?.Sum(d => d.FiatPayrate) ?? 0):F2} {BalanceAndExchangeRates.SelectedFiatCurrency}{PerTime}";


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

        public bool NHMWSConnected { get; private set; } = false;

        public MainVM()
            : base(ApplicationStateManager.Title)
        {
            _updateTimer = new Timer(1000);
            _updateTimer.Elapsed += UpdateTimerOnElapsed;

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

            OnPropertyChanged(nameof(NHMWSConnected));
            ApplicationStateManager.OnNhmwsConnectionChanged += (_, nhmwsConnected) =>
            {
                NHMWSConnected = nhmwsConnected;
                OnPropertyChanged(nameof(NHMWSConnected));
            };
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

            Devices = new ObservableCollection<DeviceData>(AvailableDevices.Devices.Select(d => (DeviceData)d));
            DevicesTDP = new ObservableCollection<DeviceDataTDP>(AvailableDevices.Devices.Select(d => new DeviceDataTDP(d)));
            MiningDevs = new ObservableCollection<MiningData>(AvailableDevices.Devices.Select(d => new MiningData(d)));

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
                        if (miningDev != null) miningDev.Stats = stat;
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
        }

        public async Task StopMining()
        {
            await ApplicationStateManager.StopAllDevicesTask();
        }
    }
}

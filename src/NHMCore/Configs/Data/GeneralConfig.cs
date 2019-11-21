using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Mining;
using NHMCore.Stats;
using NHMCore.Switching;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NHMCore.Configs.Data
{
    [Serializable]
    public class GeneralConfig : NotifyChangedBase
    {
        public Version ConfigFileVersion;

        public string DisplayCurrency
        {
            get => BalanceAndExchangeRates.Instance.SelectedFiatCurrency;
            set
            {
                BalanceAndExchangeRates.Instance.SelectedFiatCurrency = value;
                OnPropertyChanged();
            }
        }

        #region CredentialsSettings
        public string BitcoinAddress
        {
            get => CredentialsSettings.Instance.BitcoinAddress;
            set => CredentialsSettings.Instance.BitcoinAddress = value;
        }

        public string WorkerName
        {
            get => CredentialsSettings.Instance.WorkerName;
            set => CredentialsSettings.Instance.WorkerName = value;
        }

        public string RigGroup
        {
            get => CredentialsSettings.Instance.RigGroup;
            set => CredentialsSettings.Instance.RigGroup = value;
        }
        #endregion CredentialsSettings

        public TimeUnitType TimeUnit
        {
            get => TimeFactor.UnitType;
            set => TimeFactor.UnitType = value;
        }

        public int ServiceLocation
        {
            get => StratumService.ServiceLocation;
            set => StratumService.ServiceLocation = value;
        }

        public bool AutoStartMining { get; set; } = false;


        private bool _hideMiningWindows = false;
        public bool HideMiningWindows
        {
            get => _hideMiningWindows;
            set
            {
                _hideMiningWindows = value;
                MinerPluginToolkitV1.MinerToolkit.HideMiningWindows = value;
            }
        }

        private bool _minimizeMiningWindows = false;
        public bool MinimizeMiningWindows
        {
            get => _minimizeMiningWindows;
            set
            {
                _minimizeMiningWindows = value;
                MinerPluginToolkitV1.MinerToolkit.MinimizeMiningWindows = value;
            }
        }

        public double SwitchProfitabilityThreshold { get; set; } = 0.05; // percent
        public int MinerAPIQueryInterval { get; set; } = 5;


        private int _minerRestartDelayMS = 1000;
        public int MinerRestartDelayMS
        {
            get => _minerRestartDelayMS;
            set
            {
                _minerRestartDelayMS = value;
                MinerPluginToolkitV1.MinerToolkit.MinerRestartDelayMS = value;
            }
        }

        private bool _autoScaleBTCValues = true;
        public bool AutoScaleBTCValues
        {
            get => _autoScaleBTCValues;
            set
            {
                _autoScaleBTCValues = value;
                OnPropertyChanged(nameof(AutoScaleBTCValues));
            }
        }


        public bool StartMiningWhenIdle
        {
            get => IdleMiningSettings.Instance.StartMiningWhenIdle;
            set => IdleMiningSettings.Instance.StartMiningWhenIdle = value;
        }
        public IdleCheckType IdleCheckType
        {
            get => IdleMiningSettings.Instance.IdleCheckType;
            set => IdleMiningSettings.Instance.IdleCheckType = value;
        }

        public int MinIdleSeconds { get; set; } = 60;

        #region LoggingDebugConsoleSettings

        public bool DebugConsole
        {
            get => LoggingDebugConsoleSettings.Instance.DebugConsole;
            set => LoggingDebugConsoleSettings.Instance.DebugConsole = value;
        }

        public bool LogToFile
        {
            get => LoggingDebugConsoleSettings.Instance.LogToFile;
            set => LoggingDebugConsoleSettings.Instance.LogToFile = value;
        }

        // in bytes
        public long LogMaxFileSize
        {
            get => LoggingDebugConsoleSettings.Instance.LogMaxFileSize;
            set => LoggingDebugConsoleSettings.Instance.LogMaxFileSize = value;
        }
        #endregion LoggingDebugConsoleSettings

        public bool ShowDriverVersionWarning { get; set; } = true;
        public bool DisableWindowsErrorReporting { get; set; } = true;
        public bool ShowInternetConnectionWarning { get; set; } = true;
        public bool NVIDIAP0State { get; set; } = false;

        private int _apiBindPortPoolStart { get; set; } = 4000;
        public int ApiBindPortPoolStart
        {
            get => _apiBindPortPoolStart;
            set
            {
                _apiBindPortPoolStart = value;
                MinerPluginToolkitV1.FreePortsCheckerManager.ApiBindPortPoolStart = value;
            }
        }
        public double MinimumProfit
        {
            get => MiningProfitSettings.Instance.MinimumProfit;
            set => MiningProfitSettings.Instance.MinimumProfit = value;
        }
        public bool MineRegardlessOfProfit
        {
            get => MiningProfitSettings.Instance.MineRegardlessOfProfit;
            set => MiningProfitSettings.Instance.MineRegardlessOfProfit = value;
        }

        public bool IdleWhenNoInternetAccess { get; set; } = true;
        
        private bool _allowMultipleInstances { get; set; } = true;
        public bool AllowMultipleInstances
        {
            get => _allowMultipleInstances;
            set
            {
                _allowMultipleInstances = value;
                OnPropertyChanged(nameof(AllowMultipleInstances));
            }
        }

        // IFTTT
        public bool UseIFTTT { get; set; } = false;
        public string IFTTTKey { get; set; } = "";

        #region ToS 'Settings'
        
        // 3rd party miners
        public int Use3rdPartyMinersTOS = 0;

        // 
        public string hwid = "";

        public int agreedWithTOS = 0;
        #endregion ToS 'Settings'

        public bool CoolDownCheckEnabled
        {
            get => MinerApiWatchdog.Enabled;
            set => MinerApiWatchdog.Enabled = value;
        }

        public Interval SwitchSmaTimeChangeSeconds { get; set; } = new Interval(34, 55);
        public Interval SwitchSmaTicksStable = new Interval(2, 3);
        public Interval SwitchSmaTicksUnstable = new Interval(5, 13);

        /// <summary>
        /// Cost of electricity in kW-h
        /// </summary>
        public double KwhPrice { get; set; } = 0;

        /// <summary>
        /// True if NHML should try to cache SMA values for next launch
        /// </summary>
        public bool UseSmaCache { get; set; } = true;

        #region GUI Settings

        public string Language
        {
#if WPF
            get;
            set;
#else
            get => TranslationsSettings.Instance.Language;
            set => TranslationsSettings.Instance.Language = value;
#endif
        }

        public bool MinimizeToTray { get; set; } = false;

        private string _displayTheme = "Light";
        public string DisplayTheme
        {
            get => _displayTheme;
            set
            {
                _displayTheme = value;
                OnPropertyChanged();
            }
        }

        public bool ShowPowerColumns { get; set; } = false;
        public bool ShowDiagColumns { get; set; } = true;

        public Point MainFormSize = new Point(1000, 400);

        public bool GUIWindowsAlwaysOnTop { get; set; } = false;
        
        #endregion GUI Settings

        public bool UseEthlargement { get; set; } = false;

        public bool RunAtStartup
        {
            get => NHMCore.Configs.RunAtStartup.Instance.Enabled;
            set => NHMCore.Configs.RunAtStartup.Instance.Enabled = value;
        }

        #region Global Device settings
        public bool RunScriptOnCUDA_GPU_Lost { get; set; } = false;

        public bool DisableDeviceStatusMonitoring { get; set; } = false;
        public bool DisableDevicePowerModeSettings { get; set; } = true;

        private bool _showGPUPCIeBusIDs { get; set; } = false;
        public bool ShowGPUPCIeBusIDs
        {
            get => _showGPUPCIeBusIDs;
            set
            {
                _showGPUPCIeBusIDs = value;
                OnPropertyChanged(nameof(ShowGPUPCIeBusIDs));
            }
        }

        #endregion Global Device settings

        // methods
        public void SetDefaults()
        {
            ConfigFileVersion = new Version(Application.ProductVersion);
            BitcoinAddress = "";
            WorkerName = "worker1";
            RigGroup = "";
            Language = "";
            TimeUnit = TimeUnitType.Day;
            ServiceLocation = 0;
            AutoStartMining = false;
            //LessThreads = 0;
            DebugConsole = false;
            HideMiningWindows = false;
            MinimizeToTray = false;
            AutoScaleBTCValues = true;
            StartMiningWhenIdle = false;
            LogToFile = true;
            LogMaxFileSize = 1048576;
            ShowDriverVersionWarning = true;
            DisableWindowsErrorReporting = true;
            ShowInternetConnectionWarning = true;
            NVIDIAP0State = false;
            MinerRestartDelayMS = 500;
            SwitchProfitabilityThreshold = 0.05; // percent
            MinIdleSeconds = 60;
            DisplayCurrency = "USD";
            ApiBindPortPoolStart = 4000;
            MinimumProfit = 0;
            IdleWhenNoInternetAccess = true;
            IdleCheckType = IdleCheckType.SessionLock;
            AllowMultipleInstances = true;
            UseIFTTT = false;
            CoolDownCheckEnabled = true;
            RunScriptOnCUDA_GPU_Lost = false;
            SwitchSmaTimeChangeSeconds = new Interval(34, 55);
            SwitchSmaTicksStable = new Interval(2, 3);
            SwitchSmaTicksUnstable = new Interval(5, 13);
            UseSmaCache = true;
            ShowPowerColumns = false;
            ShowDiagColumns = true;
            UseEthlargement = false;
            
            RunAtStartup = false;
            GUIWindowsAlwaysOnTop = false;
            DisableDeviceStatusMonitoring = false;
            DisableDevicePowerModeSettings = true;
            MineRegardlessOfProfit = true;
        }

        public void FixSettingBounds()
        {
            ConfigFileVersion = new Version(Application.ProductVersion);
            if (string.IsNullOrEmpty(DisplayCurrency)
                || string.IsNullOrWhiteSpace(DisplayCurrency))
            {
                DisplayCurrency = "USD";
            }
            if (CredentialValidators.ValidateBitcoinAddress(BitcoinAddress) == false)
            {
                BitcoinAddress = "";
            }
            if (CredentialValidators.ValidateWorkerName(WorkerName) == false)
            {
                WorkerName = "worker1";
            }
            if (MinerAPIQueryInterval <= 0)
            {
                MinerAPIQueryInterval = 5;
            }
            if (MinerRestartDelayMS <= 0)
            {
                MinerRestartDelayMS = 500;
            }
            if (MinIdleSeconds <= 0)
            {
                MinIdleSeconds = 60;
            }
            if (LogMaxFileSize <= 0)
            {
                LogMaxFileSize = 1048576;
            }
            // check port start number, leave about 2000 ports pool size, huge yea!
            if (ApiBindPortPoolStart > (65535 - 2000))
            {
                ApiBindPortPoolStart = 5100;
            }

            if (KwhPrice < 0)
            {
                KwhPrice = 0;
            }
            // for backward compatibility fix the new setting to language codes
            var langCodes = new Dictionary<string, string> {
                { "0", "en" },
                { "1", "ru" },
                { "2", "es" },
                { "3", "pt" },
                { "4", "bg" },
                { "5", "it" },
                { "6", "pl" },
                { "7", "zh_cn" },
                { "8", "ro" },
            };
            if (Language == null)
            {
                Language = "en";
            }
            else if (langCodes.ContainsKey(Language))
            {
                Language = langCodes[Language];
            }

            SwitchSmaTimeChangeSeconds.FixRange();
            SwitchSmaTicksStable.FixRange();
            SwitchSmaTicksUnstable.FixRange();
        }
    }
}

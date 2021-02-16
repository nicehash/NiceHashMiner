using NHM.Common;
using NHM.Common.Enums;
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

        #region ServiceLocationSettings
        public int ServiceLocation
        {
            get => StratumService.Instance.ServiceLocation;
            set => StratumService.Instance.ServiceLocation = value;
        }
        #endregion ServiceLocationSettings

        #region MiningSettings
        public bool AutoStartMining
        {
            get => MiningSettings.Instance.AutoStartMining;
            set => MiningSettings.Instance.AutoStartMining = value;
        }
        public bool HideMiningWindows
        {
            get => MiningSettings.Instance.HideMiningWindows;
            set => MiningSettings.Instance.HideMiningWindows = value;
        }
        public bool MinimizeMiningWindows
        {
            get => MiningSettings.Instance.MinimizeMiningWindows;
            set => MiningSettings.Instance.MinimizeMiningWindows = value;
        }
        public int MinerAPIQueryInterval
        {
            get => MiningSettings.Instance.MinerAPIQueryInterval;
            set => MiningSettings.Instance.MinerAPIQueryInterval = value;
        }
        public int MinerRestartDelayMS
        {
            get => MiningSettings.Instance.MinerRestartDelayMS;
            set => MiningSettings.Instance.MinerRestartDelayMS = value;
        }
        public int ApiBindPortPoolStart
        {
            get => MiningSettings.Instance.ApiBindPortPoolStart;
            set => MiningSettings.Instance.ApiBindPortPoolStart = value;
        }
        public bool NVIDIAP0State
        {
            get => MiningSettings.Instance.NVIDIAP0State;
            set => MiningSettings.Instance.NVIDIAP0State = value;
        }
        #endregion MiningSettings

        #region IdleMiningSettings
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
        public int MinIdleSeconds
        {
            get => IdleMiningSettings.Instance.MinIdleSeconds;
            set => IdleMiningSettings.Instance.MinIdleSeconds = value;
        }
        public bool IdleWhenNoInternetAccess
        {
            get => IdleMiningSettings.Instance.IdleWhenNoInternetAccess;
            set => IdleMiningSettings.Instance.IdleWhenNoInternetAccess = value;
        }
        #endregion IdleMiningSettings

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

        #region WarningSettings
        public bool ShowDriverVersionWarning
        {
            get => WarningSettings.Instance.ShowDriverVersionWarning;
            set => WarningSettings.Instance.ShowDriverVersionWarning = value;
        }
        public bool DisableWindowsErrorReporting
        {
            get => WarningSettings.Instance.DisableWindowsErrorReporting;
            set => WarningSettings.Instance.DisableWindowsErrorReporting = value;
        }
        public bool ShowInternetConnectionWarning
        {
            get => WarningSettings.Instance.ShowInternetConnectionWarning;
            set => WarningSettings.Instance.ShowInternetConnectionWarning = value;
        }
        #endregion WarningSettings

        #region MiningProfitSettings
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
        #endregion MiningProfitSettings

        #region IFTTTSettings
        public bool UseIFTTT
        {
            get => IFTTTSettings.Instance.UseIFTTT;
            set => IFTTTSettings.Instance.UseIFTTT = value;
        }
        public string IFTTTKey
        {
            get => IFTTTSettings.Instance.IFTTTKey;
            set => IFTTTSettings.Instance.IFTTTKey = value;
        }
        #endregion IFTTTSettings

        #region ToS 'Settings'
        public int Use3rdPartyMinersTOS
        {
            get => ToSSetings.Instance.Use3rdPartyMinersTOS;
            set => ToSSetings.Instance.Use3rdPartyMinersTOS = value;
        }
        public string Hwid
        {
            get => ApplicationStateManager.RigID();
            set => ToSSetings.Instance.Hwid = value;
        }
        public int AgreedWithTOS
        {
            get => ToSSetings.Instance.AgreedWithTOS;
            set => ToSSetings.Instance.AgreedWithTOS = value;
        }
        #endregion ToS 'Settings'

        #region SwitchSettings
        public Interval SwitchSmaTimeChangeSeconds
        {
            get => SwitchSettings.Instance.SwitchSmaTimeChangeSeconds;
            set => SwitchSettings.Instance.SwitchSmaTimeChangeSeconds = value;
        }
        public Interval SwitchSmaTicksStable
        {
            get => SwitchSettings.Instance.SwitchSmaTicksStable;
            set => SwitchSettings.Instance.SwitchSmaTicksStable = value;
        }
        public Interval SwitchSmaTicksUnstable
        {
            get => SwitchSettings.Instance.SwitchSmaTicksUnstable;
            set => SwitchSettings.Instance.SwitchSmaTicksUnstable = value;
        }
        public double KwhPrice
        {
            get => SwitchSettings.Instance.KwhPrice;
            set => SwitchSettings.Instance.KwhPrice = value;
        }
        public double SwitchProfitabilityThreshold
        {
            get => SwitchSettings.Instance.SwitchProfitabilityThreshold;
            set => SwitchSettings.Instance.SwitchProfitabilityThreshold = value;
        }
        #endregion SwitchSettings

        #region GUI Settings
        public string DisplayCurrency
        {
            get => GUISettings.Instance.DisplayCurrency;
            set => GUISettings.Instance.DisplayCurrency = value;
        }
        public TimeUnitType TimeUnit
        {
            get => GUISettings.Instance.TimeUnit;
            set => GUISettings.Instance.TimeUnit = value;
        }
        public bool AutoScaleBTCValues
        {
            get => GUISettings.Instance.AutoScaleBTCValues;
            set => GUISettings.Instance.AutoScaleBTCValues = value;
        }
        public string Language
        {
            get => TranslationsSettings.Instance.Language;
            set => TranslationsSettings.Instance.Language = value;
        }
        public bool MinimizeToTray
        {
            get => GUISettings.Instance.MinimizeToTray;
            set => GUISettings.Instance.MinimizeToTray = value;
        }
        public bool DisplayPureProfit
        {
            get => GUISettings.Instance.DisplayPureProfit;
            set => GUISettings.Instance.DisplayPureProfit = value;
        }
        public string DisplayTheme
        {
            get => GUISettings.Instance.DisplayTheme;
            set => GUISettings.Instance.DisplayTheme = value;
        }
        public bool ShowPowerColumns
        {
            get => GUISettings.Instance.ShowPowerColumns;
            set => GUISettings.Instance.ShowPowerColumns = value;
        }
        public bool ShowDiagColumns
        {
            get => GUISettings.Instance.ShowDiagColumns;
            set => GUISettings.Instance.ShowDiagColumns = value;
        }
        public bool GUIWindowsAlwaysOnTop
        {
            get => GUISettings.Instance.GUIWindowsAlwaysOnTop;
            set => GUISettings.Instance.GUIWindowsAlwaysOnTop = value;
        }
        public Size MainFormSize
        {
            get => GUISettings.Instance.MainFormSize;
            set => GUISettings.Instance.MainFormSize = value;
        }
        #endregion GUI Settings

        #region MiscSettings
        public bool AllowMultipleInstances
        {
            get => MiscSettings.Instance.AllowMultipleInstances;
            set => MiscSettings.Instance.AllowMultipleInstances = value;
        }
        public bool CoolDownCheckEnabled
        {
            get => MiscSettings.Instance.CoolDownCheckEnabled;
            set => MiscSettings.Instance.CoolDownCheckEnabled = value;
        }
        public bool UseSmaCache
        {
            get => MiscSettings.Instance.UseSmaCache;
            set => MiscSettings.Instance.UseSmaCache = value;
        }
        public bool UseEthlargement
        {
            get => MiscSettings.Instance.UseEthlargement;
            set => MiscSettings.Instance.UseEthlargement = value;
        }
        public bool RunAtStartup
        {
            get => MiscSettings.Instance.RunAtStartup;
            set => MiscSettings.Instance.RunAtStartup = value;
        }
        public Dictionary<string, bool> ShowNotifications
        {
            get => MiscSettings.Instance.ShowNotifications;
            set => MiscSettings.Instance.ShowNotifications = value;
        }
        public bool DisableVisualCRedistributableCheck
        {
            get => MiscSettings.Instance.DisableVisualCRedistributableCheck;
            set => MiscSettings.Instance.DisableVisualCRedistributableCheck = value;
        }
        #endregion MiscSettings

        #region Global Device settings
        public bool CheckForMissingGPUs
        {
            get => GlobalDeviceSettings.Instance.CheckForMissingGPUs;
            set => GlobalDeviceSettings.Instance.CheckForMissingGPUs = value;
        }
        public bool RestartMachineOnLostGPU
        {
            get => GlobalDeviceSettings.Instance.RestartMachineOnLostGPU;
            set => GlobalDeviceSettings.Instance.RestartMachineOnLostGPU = value;
        }
        public bool DisableDeviceStatusMonitoring
        {
            get => GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring;
            set => GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring = value;
        }
        public bool DisableDevicePowerModeSettings
        {
            get => GlobalDeviceSettings.Instance.DisableDevicePowerModeSettings;
            set => GlobalDeviceSettings.Instance.DisableDevicePowerModeSettings = value;
        }
        public bool ShowGPUPCIeBusIDs
        {
            get => GlobalDeviceSettings.Instance.ShowGPUPCIeBusIDs;
            set => GlobalDeviceSettings.Instance.ShowGPUPCIeBusIDs = value;
        }
        #endregion Global Device settings

        #region UpdateSettings
        public bool AutoUpdateNiceHashMiner2
        {
            get => UpdateSettings.Instance.AutoUpdateNiceHashMiner;
            set => UpdateSettings.Instance.AutoUpdateNiceHashMiner = value;
        }
        public bool AutoUpdateMinerPlugins
        {
            get => UpdateSettings.Instance.AutoUpdateMinerPlugins;
            set => UpdateSettings.Instance.AutoUpdateMinerPlugins = value;
        }
        #endregion UpdateSettings

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
            MinimizeMiningWindows = false;
            MinimizeToTray = false;
            AutoScaleBTCValues = true;
            StartMiningWhenIdle = false;
            LogToFile = true;
            LogMaxFileSize = 1048576;
            ShowDriverVersionWarning = true;
            DisableWindowsErrorReporting = true;
            ShowInternetConnectionWarning = true;
            NVIDIAP0State = false;
            MinerAPIQueryInterval = 5;
            MinerRestartDelayMS = 500;
            SwitchProfitabilityThreshold = 0.02; // percent
            MinIdleSeconds = 60;
            DisplayCurrency = "USD";
            ApiBindPortPoolStart = 4000;
            MinimumProfit = 0;
            IdleWhenNoInternetAccess = true;
            IdleCheckType = IdleCheckType.SessionLock;
            AllowMultipleInstances = false;
            UseIFTTT = false;
            IFTTTKey = "";
            CoolDownCheckEnabled = true;
            CheckForMissingGPUs = false;
            RestartMachineOnLostGPU = false;
            SwitchSmaTimeChangeSeconds = new Interval(34, 55);
            SwitchSmaTicksStable = new Interval(2, 3);
            SwitchSmaTicksUnstable = new Interval(5, 13);
            UseSmaCache = true;
            ShowPowerColumns = false;
            ShowDiagColumns = true;
            UseEthlargement = false;
            Use3rdPartyMinersTOS = 0;
            Hwid = "";
            AgreedWithTOS = 0;
            KwhPrice = 0;
            DisplayPureProfit = false;
            DisplayTheme = "Light";
            ShowGPUPCIeBusIDs = false;
            ShowNotifications = new Dictionary<string, bool>();

            RunAtStartup = false;
            GUIWindowsAlwaysOnTop = false;
            DisableDeviceStatusMonitoring = false;
            DisableDevicePowerModeSettings = true;
            MineRegardlessOfProfit = true;

            AutoUpdateNiceHashMiner2 = false;
            AutoUpdateMinerPlugins = true;
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

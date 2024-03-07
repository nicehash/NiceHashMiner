﻿using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Switching;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

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
        public bool PauseMiningWhenGamingMode
        {
            get => MiningSettings.Instance.PauseMiningWhenGamingMode;
            set => MiningSettings.Instance.PauseMiningWhenGamingMode = value;
        }
        public string DeviceToPauseUuid
        {
            get => MiningSettings.Instance.DeviceToPauseUuid;
            set => MiningSettings.Instance.DeviceToPauseUuid = value;
        }

        public bool UseScheduler
        {
            get => MiningSettings.Instance.UseScheduler;
            set => MiningSettings.Instance.UseScheduler = value;
        }

        public bool EnableSSLMining
        {
            get => MiningSettings.Instance.EnableSSLMining;
            set => MiningSettings.Instance.EnableSSLMining = value;
        }
        public bool AdvancedMode
        {
            get => MiscSettings.Instance.AdvancedMode;
            set => MiscSettings.Instance.AdvancedMode = value;
        }
        public bool SendEvents
        {
            get => MiscSettings.Instance.SendEvents;
            set => MiscSettings.Instance.SendEvents = value;
        }
        public bool AutoResetOC
        {
            get => MiscSettings.Instance.AutoResetOC;
            set => MiscSettings.Instance.AutoResetOC = value;
        }
        public bool EnableGPUManagement
        {
            get => MiscSettings.Instance.EnableGPUManagement;
            set => MiscSettings.Instance.EnableGPUManagement = value;
        }
        public bool DetectIntegratedDevices
        {
            get => MiscSettings.Instance.DetectIntegratedDevices;
            set => MiscSettings.Instance.DetectIntegratedDevices = value;
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
        public string Hwid
        {
            get => ApplicationStateManager.RigID();
            set => ToSSetings.Instance.Hwid = value;
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
        public bool UseOptimizationProfiles
        {
            get => MiscSettings.Instance.UseOptimizationProfiles;
            set => MiscSettings.Instance.UseOptimizationProfiles = value;
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

        public bool ResolveNiceHashDomainsToIPs
        {
            get => MiscSettings.Instance.ResolveNiceHashDomainsToIPs;
            set => MiscSettings.Instance.ResolveNiceHashDomainsToIPs = value;
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
            ConfigFileVersion = new Version(NHMApplication.ProductVersion);
            BitcoinAddress = "";
            WorkerName = "worker1";
            RigGroup = "";
            Language = "";
            TimeUnit = TimeUnitType.Day;
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
            PauseMiningWhenGamingMode = false;
            DeviceToPauseUuid = "";
            UseScheduler = false;
            EnableSSLMining = false;
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
            ShowPowerColumns = false;
            ShowDiagColumns = true;
            UseOptimizationProfiles = false;
            Hwid = "";
            KwhPrice = 0;
            DisplayPureProfit = false;
            DisplayTheme = "Light";
            ShowGPUPCIeBusIDs = false;
            ShowNotifications = new Dictionary<string, bool>();

            ResolveNiceHashDomainsToIPs = true;

            RunAtStartup = false;
            GUIWindowsAlwaysOnTop = false;
            DisableDeviceStatusMonitoring = false;
            MineRegardlessOfProfit = true;

            AutoUpdateNiceHashMiner2 = false;
            AutoUpdateMinerPlugins = true;
            AdvancedMode = false;
            SendEvents = true;
            AutoResetOC = true;
            EnableGPUManagement = true;
        }

        public void SetValues(GeneralConfigOld configOld)
        {
            ConfigFileVersion = new Version(NHMApplication.ProductVersion);
            BitcoinAddress = configOld.BitcoinAddress;
            WorkerName = configOld.WorkerName;
            RigGroup = configOld.RigGroup;
            Language = configOld.Language;
            TimeUnit = configOld.TimeUnit;
            AutoStartMining = configOld.AutoStartMining;
            //LessThreads = 0;
            DebugConsole = configOld.DebugConsole;
            HideMiningWindows = configOld.HideMiningWindows;
            MinimizeMiningWindows = configOld.MinimizeMiningWindows;
            MinimizeToTray = configOld.MinimizeToTray;
            AutoScaleBTCValues = configOld.AutoScaleBTCValues;
            StartMiningWhenIdle = configOld.StartMiningWhenIdle;
            LogToFile = configOld.LogToFile;
            LogMaxFileSize = configOld.LogMaxFileSize;
            ShowDriverVersionWarning = configOld.ShowDriverVersionWarning;
            DisableWindowsErrorReporting = configOld.DisableWindowsErrorReporting;
            ShowInternetConnectionWarning = configOld.ShowInternetConnectionWarning;
            PauseMiningWhenGamingMode = configOld.PauseMiningWhenGamingMode;
            DeviceToPauseUuid = configOld.DeviceToPauseUuid;
            UseScheduler = false;
            EnableSSLMining = configOld.EnableSSLMining;
            MinerAPIQueryInterval = configOld.MinerAPIQueryInterval;
            MinerRestartDelayMS = configOld.MinerRestartDelayMS;
            SwitchProfitabilityThreshold = configOld.SwitchProfitabilityThreshold;
            MinIdleSeconds = configOld.MinIdleSeconds;
            DisplayCurrency = configOld.DisplayCurrency;
            ApiBindPortPoolStart = configOld.ApiBindPortPoolStart;
            MinimumProfit = configOld.MinimumProfit;
            IdleWhenNoInternetAccess = configOld.IdleWhenNoInternetAccess;
            IdleCheckType = configOld.IdleCheckType;
            AllowMultipleInstances = configOld.AllowMultipleInstances;
            UseIFTTT = configOld.UseIFTTT;
            IFTTTKey = configOld.IFTTTKey;
            CoolDownCheckEnabled = configOld.CoolDownCheckEnabled;
            CheckForMissingGPUs = configOld.CheckForMissingGPUs;
            RestartMachineOnLostGPU = configOld.RestartMachineOnLostGPU;
            SwitchSmaTimeChangeSeconds = configOld.SwitchSmaTimeChangeSeconds;
            SwitchSmaTicksStable = configOld.SwitchSmaTicksStable;
            SwitchSmaTicksUnstable = configOld.SwitchSmaTicksUnstable;
            ShowPowerColumns = configOld.ShowPowerColumns;
            ShowDiagColumns = configOld.ShowDiagColumns;
            UseOptimizationProfiles = configOld.UseOptimizationProfiles;
            Hwid = configOld.Hwid;
            KwhPrice = configOld.KwhPrice;
            DisplayPureProfit = configOld.DisplayPureProfit;
            DisplayTheme = configOld.DisplayTheme;
            ShowGPUPCIeBusIDs = configOld.ShowGPUPCIeBusIDs;
            ShowNotifications = configOld.ShowNotifications;

            ResolveNiceHashDomainsToIPs = configOld.ResolveNiceHashDomainsToIPs;

            RunAtStartup = configOld.RunAtStartup;
            GUIWindowsAlwaysOnTop = configOld.GUIWindowsAlwaysOnTop;
            DisableDeviceStatusMonitoring = configOld.DisableDeviceStatusMonitoring;
            MineRegardlessOfProfit = configOld.MineRegardlessOfProfit;

            AutoUpdateNiceHashMiner2 = configOld.AutoUpdateNiceHashMiner2;
            AutoUpdateMinerPlugins = configOld.AutoUpdateMinerPlugins;
            AdvancedMode = false;
            SendEvents = true;
            AutoResetOC = true;
            EnableGPUManagement = true;
        }

        public void FixSettingBounds()
        {
            ConfigFileVersion = new Version(NHMApplication.ProductVersion);
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

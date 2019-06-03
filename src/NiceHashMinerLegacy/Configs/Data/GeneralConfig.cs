using System;
using System.Collections.Generic;
using System.Drawing;
using NiceHashMiner.Miners;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Configs.Data
{
    [Serializable]
    public class GeneralConfig
    {
        public Version ConfigFileVersion;
        public string Language = ""; // no language by default
        public string DisplayCurrency = "USD";

        public bool DebugConsole = false;
        public string BitcoinAddress = "";
        public string WorkerName = "worker1";
        public TimeUnitType TimeUnit = TimeUnitType.Day;
        public string IFTTTKey = "";
        public int ServiceLocation = 0;

        public bool AutoStartMining = false;


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

        public bool MinimizeToTray = false;

        public double SwitchProfitabilityThreshold = 0.05; // percent
        public int MinerAPIQueryInterval = 5;


        private int _minerRestartDelayMS = 500;
        public int MinerRestartDelayMS
        {
            get => _minerRestartDelayMS;
            set
            {
                _minerRestartDelayMS = value;
                MinerPluginToolkitV1.MinerToolkit.MinerRestartDelayMS = value;
            }
        }

        public bool AutoScaleBTCValues = true;

        public bool StartMiningWhenIdle = false;
        public IdleCheckType IdleCheckType = IdleCheckType.SessionLock;
        public int MinIdleSeconds = 60;

        public bool LogToFile = true;

        // in bytes
        public long LogMaxFileSize = 1048576;

        public bool ShowDriverVersionWarning = true;
        public bool DisableWindowsErrorReporting = true;
        public bool ShowInternetConnectionWarning = true;
        public bool NVIDIAP0State = false;

        private int _apiBindPortPoolStart = 4000;
        public int ApiBindPortPoolStart
        {
            get => _apiBindPortPoolStart;
            set
            {
                _apiBindPortPoolStart = value;
                MinerPluginToolkitV1.FreePortsCheckerManager.ApiBindPortPoolStart = value;
            }
        }
        public double MinimumProfit = 0;
        public bool IdleWhenNoInternetAccess = true;
        public bool UseIFTTT = false;

        public bool RunScriptOnCUDA_GPU_Lost = false;

        // 3rd party miners
        public Use3rdPartyMiners Use3rdPartyMiners = Use3rdPartyMiners.NOT_SET;

        public bool AllowMultipleInstances = true;

        // 
        public string hwid = "";

        public int agreedWithTOS = 0;


        public bool CoolDownCheckEnabled
        {
            get => MinerApiWatchdog.Enabled;
            set => MinerApiWatchdog.Enabled = value;
        }

        // Overriding AMDOpenCLDeviceDetection returned Bus IDs (in case of driver error, e.g. 17.12.1)
        public string OverrideAMDBusIds = "";

        public Interval SwitchSmaTimeChangeSeconds = new Interval(34, 55);
        public Interval SwitchSmaTicksStable = new Interval(2, 3);
        public Interval SwitchSmaTicksUnstable = new Interval(5, 13);

        /// <summary>
        /// Cost of electricity in kW-h
        /// </summary>
        public double KwhPrice = 0;

        /// <summary>
        /// True if NHML should try to cache SMA values for next launch
        /// </summary>
        public bool UseSmaCache = true;

        public bool ShowPowerColumns = false;
        public bool ShowDiagColumns = true;

        public Point MainFormSize = new Point(1000, 400);

        public bool UseEthlargement = false;

        public string RigGroup = "";

        // methods
        public void SetDefaults()
        {
            ConfigFileVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            //Language = "en";
            BitcoinAddress = "";
            WorkerName = "worker1";
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
            //ContinueMiningIfNoInternetAccess = false;
            IdleWhenNoInternetAccess = true;
            IdleCheckType = IdleCheckType.SessionLock;
            Use3rdPartyMiners = Use3rdPartyMiners.NOT_SET;
            AllowMultipleInstances = true;
            UseIFTTT = false;
            CoolDownCheckEnabled = true;
            RunScriptOnCUDA_GPU_Lost = false;
            OverrideAMDBusIds = "";
            SwitchSmaTimeChangeSeconds = new Interval(34, 55);
            SwitchSmaTicksStable = new Interval(2, 3);
            SwitchSmaTicksUnstable = new Interval(5, 13);
            UseSmaCache = true;
            ShowPowerColumns = false;
            ShowDiagColumns = true;
            UseEthlargement = false;
            RigGroup = "";
        }

        public void FixSettingBounds()
        {
            ConfigFileVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (string.IsNullOrEmpty(DisplayCurrency)
                || string.IsNullOrWhiteSpace(DisplayCurrency))
            {
                DisplayCurrency = "USD";
            }
            if (NiceHashMiner.BitcoinAddress.ValidateBitcoinAddress(BitcoinAddress) == false)
            {
                BitcoinAddress = "";
            }
            if (NiceHashMiner.BitcoinAddress.ValidateWorkerName(WorkerName) == false)
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

        public bool HasValidUserWorker()
        {
            return NiceHashMiner.BitcoinAddress.ValidateBitcoinAddress(BitcoinAddress) &&
                   NiceHashMiner.BitcoinAddress.ValidateWorkerName(WorkerName);
        }

        //C#7
        public (string btc, string worker, string group) GetCredentials()
        {
            return (BitcoinAddress.Trim(), WorkerName.Trim(), RigGroup.Trim());
        }
    }
}

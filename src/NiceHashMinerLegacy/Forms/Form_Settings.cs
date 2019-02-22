using Microsoft.Win32;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common.Enums;
using static NiceHashMiner.Translations;

namespace NiceHashMiner.Forms
{
    public partial class Form_Settings : Form
    {
        private readonly bool _isInitFinished = false;
        private bool _isChange = false;

        public bool IsChange
        {
            get => _isChange;
            private set => _isChange = _isInitFinished && value;
        }

        private bool _isCredChange = false;
        public bool IsChangeSaved { get; private set; }
        public bool IsRestartNeeded { get; private set; }

        // most likely we wil have settings only per unique devices
        private const bool ShowUniqueDeviceList = true;

        private ComputeDevice _selectedComputeDevice;

        private readonly RegistryKey _rkStartup;

        private bool _isStartupChanged = false;

        public Form_Settings()
        {
            InitializeComponent();
            Icon = Properties.Resources.logo;

            //ret = 1; // default
            IsChange = false;
            IsChangeSaved = false;

            // backup settings
            ConfigManager.CreateBackup();

            // initialize form
            InitializeFormTranslations();

            // Initialize toolTip
            InitializeToolTip();

            // Initialize tabs
            InitializeGeneralTab();

            // initialization calls 
            InitializeDevicesTab();
            // link algorithm list with algorithm settings control
            algorithmSettingsControl1.Enabled = false;
            algorithmsListView1.ComunicationInterface = algorithmSettingsControl1;
            //algorithmsListView1.RemoveRatioRates();


            // set first device selected {
            if (AvailableDevices.Devices.Count > 0)
            {
                _selectedComputeDevice = AvailableDevices.Devices[0];
                algorithmsListView1.SetAlgorithms(_selectedComputeDevice, _selectedComputeDevice.Enabled);
                groupBoxAlgorithmSettings.Text = string.Format(Tr("Algorithm settings for {0} :"),
                    _selectedComputeDevice.Name);
            }

            // At the very end set to true
            _isInitFinished = true;

            try
            {
                _rkStartup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            }
            catch (SecurityException)
            {
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SETTINGS", e.ToString());
            }
        }

        #region Initializations

        private void InitializeToolTip()
        {
            // Setup Tooltips
            toolTip1.SetToolTip(comboBox_Language, Tr("Changes the default language for NiceHash Miner Legacy."));
            toolTip1.SetToolTip(label_Language, Tr("Changes the default language for NiceHash Miner Legacy."));
            toolTip1.SetToolTip(pictureBox_Language, Tr("Changes the default language for NiceHash Miner Legacy."));

            toolTip1.SetToolTip(checkBox_DebugConsole,
                Tr("When checked, it displays debug console."));
            toolTip1.SetToolTip(pictureBox_DebugConsole,
               Tr("When checked, it displays debug console."));

            toolTip1.SetToolTip(textBox_BitcoinAddress, Tr("User's bitcoin address for mining."));
            toolTip1.SetToolTip(label_BitcoinAddress, Tr("User's bitcoin address for mining."));
            toolTip1.SetToolTip(pictureBox_Info_BitcoinAddress,
                Tr("User's bitcoin address for mining."));

            toolTip1.SetToolTip(textBox_WorkerName, Tr("To identify the user's computer."));
            toolTip1.SetToolTip(label_WorkerName, Tr("To identify the user's computer."));
            toolTip1.SetToolTip(pictureBox_WorkerName, Tr("To identify the user's computer."));

            toolTip1.SetToolTip(comboBox_ServiceLocation,
                Tr("Sets the mining location. Choosing Hong Kong or Tokyo will add extra latency."));
            toolTip1.SetToolTip(label_ServiceLocation, Tr("Sets the mining location. Choosing Hong Kong or Tokyo will add extra latency."));
            toolTip1.SetToolTip(pictureBox_ServiceLocation,
                Tr("Sets the mining location. Choosing Hong Kong or Tokyo will add extra latency."));

            toolTip1.SetToolTip(comboBox_TimeUnit, Tr("Sets the time unit to report BTC rates."));
            toolTip1.SetToolTip(label_TimeUnit, Tr("Sets the time unit to report BTC rates."));
            toolTip1.SetToolTip(pictureBox_TimeUnit, Tr("Sets the time unit to report BTC rates."));

            toolTip1.SetToolTip(checkBox_HideMiningWindows,
                Tr("When checked, sgminer, ccminer, cpuminer and ethminer console windows will be hidden."));
            toolTip1.SetToolTip(pictureBox_HideMiningWindows,
                Tr("When checked, sgminer, ccminer, cpuminer and ethminer console windows will be hidden."));

            toolTip1.SetToolTip(checkBox_MinimizeToTray,
                Tr("When checked, NiceHash Miner Legacy will minimize to tray."));
            toolTip1.SetToolTip(pictureBox_MinimizeToTray,
                Tr("When checked, NiceHash Miner Legacy will minimize to tray."));

            toolTip1.SetToolTip(checkBox_Use3rdPartyMiners,
                Tr("Use 3rd party closed-source mining software for higher profitability. Usage is on your own responsibility."));
            toolTip1.SetToolTip(pictureBox_Use3rdPartyMiners,
                Tr("Use 3rd party closed-source mining software for higher profitability. Usage is on your own responsibility."));

            toolTip1.SetToolTip(checkBox_AllowMultipleInstances,
                Tr("When unchecked NiceHash Miner Legacy will allow only one instance running (it will close a new started instance if there is an opened instance running)."));
            toolTip1.SetToolTip(pictureBox_AllowMultipleInstances,
                Tr("When unchecked NiceHash Miner Legacy will allow only one instance running (it will close a new started instance if there is an opened instance running)."));
            
            toolTip1.SetToolTip(label_MinProfit, Tr("If set to any value more than 0 (USD), NiceHash Miner Legacy will stop mining\nif the calculated profit falls below the set amount."));
            toolTip1.SetToolTip(pictureBox_MinProfit, Tr("If set to any value more than 0 (USD), NiceHash Miner Legacy will stop mining\nif the calculated profit falls below the set amount."));
            toolTip1.SetToolTip(textBox_MinProfit, Tr("If set to any value more than 0 (USD), NiceHash Miner Legacy will stop mining\nif the calculated profit falls below the set amount."));

            toolTip1.SetToolTip(textBox_SwitchMaxSeconds,
                Tr("Upper bound for the randomly chosen profit check interval.\nProfit may be checked multiple times before a switch is allowed, so don't set too high."));
            toolTip1.SetToolTip(label_SwitchMaxSeconds,
                Tr("Upper bound for the randomly chosen profit check interval.\nProfit may be checked multiple times before a switch is allowed, so don't set too high."));
            toolTip1.SetToolTip(pictureBox_SwitchMaxSeconds,
                Tr("Upper bound for the randomly chosen profit check interval.\nProfit may be checked multiple times before a switch is allowed, so don't set too high."));

            toolTip1.SetToolTip(textBox_SwitchMinSeconds,
                Tr("Lower bound for the randomly chosen profit check interval.\nDo not set too low."));
            toolTip1.SetToolTip(label_SwitchMinSeconds,
                Tr("Lower bound for the randomly chosen profit check interval.\nDo not set too low."));
            toolTip1.SetToolTip(pictureBox_SwitchMinSeconds,
                Tr("Lower bound for the randomly chosen profit check interval.\nDo not set too low."));

            toolTip1.SetToolTip(textBox_MinerAPIQueryInterval,
                Tr("API query interval for ccminer, sgminer cpuminer and ethminer."));
            toolTip1.SetToolTip(label_MinerAPIQueryInterval,
                Tr("API query interval for ccminer, sgminer cpuminer and ethminer."));
            toolTip1.SetToolTip(pictureBox_MinerAPIQueryInterval,
                Tr("API query interval for ccminer, sgminer cpuminer and ethminer."));

            toolTip1.SetToolTip(textBox_MinerRestartDelayMS,
                Tr("Amount of time (in milliseconds) that NiceHash Miner Legacy will wait before restarting the miner."));
            toolTip1.SetToolTip(label_MinerRestartDelayMS,
                Tr("Amount of time (in milliseconds) that NiceHash Miner Legacy will wait before restarting the miner."));
            toolTip1.SetToolTip(pictureBox_MinerRestartDelayMS,
                Tr("Amount of time (in milliseconds) that NiceHash Miner Legacy will wait before restarting the miner."));

            toolTip1.SetToolTip(textBox_APIBindPortStart,
                Tr("Set starting port number from which miner API Bind ports will be set for communication."));
            toolTip1.SetToolTip(label_APIBindPortStart,
                Tr("Set starting port number from which miner API Bind ports will be set for communication."));
            toolTip1.SetToolTip(pictureBox_APIBindPortStart,
                Tr("Set starting port number from which miner API Bind ports will be set for communication."));

            toolTip1.SetToolTip(comboBox_DagLoadMode, Tr("Sets Dag Load Mode for ethminers."));
            toolTip1.SetToolTip(label_DagGeneration, Tr("Sets Dag Load Mode for ethminers."));
            toolTip1.SetToolTip(pictureBox_DagGeneration, Tr("Sets Dag Load Mode for ethminers."));

            benchmarkLimitControlCPU.SetToolTip(ref toolTip1, "CPUs");
            benchmarkLimitControlNVIDIA.SetToolTip(ref toolTip1, "NVIDIA GPUs");
            benchmarkLimitControlAMD.SetToolTip(ref toolTip1, "AMD GPUs");

            toolTip1.SetToolTip(checkBox_DisableDetectionNVIDIA,
                string.Format(Tr("Check it, if you would like to skip the detection of {0} GPUs."), "NVIDIA"));
            toolTip1.SetToolTip(checkBox_DisableDetectionAMD,
                string.Format(Tr("Check it, if you would like to skip the detection of {0} GPUs."), "AMD"));
            toolTip1.SetToolTip(pictureBox_DisableDetectionNVIDIA,
                string.Format(Tr("Check it, if you would like to skip the detection of {0} GPUs."), "NVIDIA"));
            toolTip1.SetToolTip(pictureBox_DisableDetectionAMD,
                string.Format(Tr("Check it, if you would like to skip the detection of {0} GPUs."), "AMD"));

            toolTip1.SetToolTip(checkBox_AutoScaleBTCValues,
                Tr("Check it, if you would like to see the BTC values autoscale to the appropriate scale."));
            toolTip1.SetToolTip(pictureBox_AutoScaleBTCValues,
                Tr("Check it, if you would like to see the BTC values autoscale to the appropriate scale."));

            toolTip1.SetToolTip(checkBox_StartMiningWhenIdle,
                Tr("Automatically start mining when computer is idle and stop mining when computer is being used."));
            toolTip1.SetToolTip(pictureBox_StartMiningWhenIdle,
                Tr("Automatically start mining when computer is idle and stop mining when computer is being used."));

            toolTip1.SetToolTip(textBox_MinIdleSeconds, Tr("When StartMiningWhenIdle is checked, MinIdleSeconds tells how\nmany seconds computer has to be idle before mining starts."));
            toolTip1.SetToolTip(label_MinIdleSeconds, Tr("When StartMiningWhenIdle is checked, MinIdleSeconds tells how\nmany seconds computer has to be idle before mining starts."));
            toolTip1.SetToolTip(pictureBox_MinIdleSeconds,
                Tr("When StartMiningWhenIdle is checked, MinIdleSeconds tells how\nmany seconds computer has to be idle before mining starts."));

            toolTip1.SetToolTip(checkBox_LogToFile, Tr("Check it, to log console output to file."));
            toolTip1.SetToolTip(pictureBox_LogToFile,
                Tr("Check it, to log console output to file."));


            toolTip1.SetToolTip(textBox_LogMaxFileSize, Tr("Sets the maximum size for the log file."));
            toolTip1.SetToolTip(label_LogMaxFileSize, Tr("Sets the maximum size for the log file."));
            toolTip1.SetToolTip(pictureBox_LogMaxFileSize,
                Tr("Sets the maximum size for the log file."));

            toolTip1.SetToolTip(checkBox_ShowDriverVersionWarning,
                Tr("When checked, NiceHash Miner Legacy would issue a warning if\na less optimal version of a driver is installed."));
            toolTip1.SetToolTip(pictureBox_ShowDriverVersionWarning,
                Tr("When checked, NiceHash Miner Legacy would issue a warning if\na less optimal version of a driver is installed."));

            toolTip1.SetToolTip(checkBox_DisableWindowsErrorReporting,
                Tr("When checked, in the event of a miner crash,\nNiceHash Miner Legacy would still be able to restart the miner again as it is not blocked by Windows error message.\nIt is recommended to have this setting checked for uninterrupted mining process because mining programs are not 100% stable."));
            toolTip1.SetToolTip(pictureBox_DisableWindowsErrorReporting,
                Tr("When checked, in the event of a miner crash,\nNiceHash Miner Legacy would still be able to restart the miner again as it is not blocked by Windows error message.\nIt is recommended to have this setting checked for uninterrupted mining process because mining programs are not 100% stable."));

            toolTip1.SetToolTip(checkBox_ShowInternetConnectionWarning,
                Tr("When checked, NiceHash Miner Legacy would issue a warning if\nthe internet connection is not available."));
            toolTip1.SetToolTip(pictureBox_ShowInternetConnectionWarning,
                Tr("When checked, NiceHash Miner Legacy would issue a warning if\nthe internet connection is not available."));

            toolTip1.SetToolTip(checkBox_NVIDIAP0State,
                Tr("When checked, NiceHash Miner Legacy will change all supported NVIDIA GPUs to P0 state.\nThis will slightly increase performance on certain algorithms.\nThis feature needs administrator privileges to be activated."));
            toolTip1.SetToolTip(pictureBox_NVIDIAP0State,
                Tr("When checked, NiceHash Miner Legacy will change all supported NVIDIA GPUs to P0 state.\nThis will slightly increase performance on certain algorithms.\nThis feature needs administrator privileges to be activated."));

            toolTip1.SetToolTip(checkBox_RunScriptOnCUDA_GPU_Lost,
                Tr("When checked, NiceHash Miner Legacy will run OnGPUsLost.bat in case at least one CUDA GPU is lost,\nby default script should restart whole system."));
            toolTip1.SetToolTip(pictureBox_RunScriptOnCUDA_GPU_Lost,
                Tr("When checked, NiceHash Miner Legacy will run OnGPUsLost.bat in case at least one CUDA GPU is lost,\nby default script should restart whole system."));

            toolTip1.SetToolTip(checkBox_RunAtStartup,
                Tr("When checked, NiceHash Miner Legacy will run on login."));
            toolTip1.SetToolTip(pictureBox_RunAtStartup,
                Tr("When checked, NiceHash Miner Legacy will run on login."));


            toolTip1.SetToolTip(checkBox_AutoStartMining,
                Tr("When checked, NiceHash Miner Legacy will automatically start mining when launched."));
            toolTip1.SetToolTip(pictureBox_AutoStartMining,
                Tr("When checked, NiceHash Miner Legacy will automatically start mining when launched."));


            toolTip1.SetToolTip(textBox_ethminerDefaultBlockHeight,
                Tr("Sets the default block height used for benchmarking if API call fails."));
            toolTip1.SetToolTip(label_ethminerDefaultBlockHeight,
                Tr("Sets the default block height used for benchmarking if API call fails."));
            toolTip1.SetToolTip(pictureBox_ethminerDefaultBlockHeight,
                Tr("Sets the default block height used for benchmarking if API call fails."));

            toolTip1.SetToolTip(label_displayCurrency, Tr("Choose what Currency to Display mining profit."));
            toolTip1.SetToolTip(pictureBox_displayCurrency,
                Tr("Choose what Currency to Display mining profit."));
            toolTip1.SetToolTip(currencyConverterCombobox,
                Tr("Choose what Currency to Display mining profit."));

            // Setup Tooltips CPU
            toolTip1.SetToolTip(comboBox_CPU0_ForceCPUExtension,
                Tr("Force certain CPU extension miner."));
            toolTip1.SetToolTip(label_CPU0_ForceCPUExtension,
                Tr("Force certain CPU extension miner."));
            toolTip1.SetToolTip(pictureBox_CPU0_ForceCPUExtension,
                Tr("Force certain CPU extension miner."));

            // amd disable temp control
            toolTip1.SetToolTip(checkBox_AMD_DisableAMDTempControl,
                Tr("Check it, if you would like to disable the built-in temperature control\nfor AMD GPUs (except daggerhashimoto algorithm)."));
            toolTip1.SetToolTip(pictureBox_AMD_DisableAMDTempControl,
                Tr("Check it, if you would like to disable the built-in temperature control\nfor AMD GPUs (except daggerhashimoto algorithm)."));

            // disable default optimizations
            toolTip1.SetToolTip(checkBox_DisableDefaultOptimizations,
                Tr("When checked it disables all default optimization settings, making mining potentially more stable but significantly slower (especially for AMD cards)."));
            toolTip1.SetToolTip(pictureBox_DisableDefaultOptimizations,
                Tr("When checked it disables all default optimization settings, making mining potentially more stable but significantly slower (especially for AMD cards)."));

            // internet connection mining check
            toolTip1.SetToolTip(checkBox_IdleWhenNoInternetAccess,
                Tr("If enabled NiceHash Miner Legacy will stop mining without internet connectivity"));
            toolTip1.SetToolTip(pictureBox_IdleWhenNoInternetAccess,
                Tr("If enabled NiceHash Miner Legacy will stop mining without internet connectivity"));

            // IFTTT notification check
            toolTip1.SetToolTip(checkBox_UseIFTTT, Tr("If enabled, NiceHash Miner Legacy will use the API Key you provide to notify you when profitability has gone below the profitability you have configured.\nSee instructions for details on configuring this functionality."));
            toolTip1.SetToolTip(pictureBox_UseIFTTT, Tr("If enabled, NiceHash Miner Legacy will use the API Key you provide to notify you when profitability has gone below the profitability you have configured.\nSee instructions for details on configuring this functionality."));

            toolTip1.SetToolTip(pictureBox_SwitchProfitabilityThreshold,
                Tr("Miner will not switch if the profitability is below SwitchProfitabilityThreshold. Value is in percentage [0 - 1]"));
            toolTip1.SetToolTip(label_SwitchProfitabilityThreshold,
                Tr("Miner will not switch if the profitability is below SwitchProfitabilityThreshold. Value is in percentage [0 - 1]"));

            toolTip1.SetToolTip(pictureBox_MinimizeMiningWindows,
                Tr("When checked, mining windows will start minimized."));
            toolTip1.SetToolTip(checkBox_MinimizeMiningWindows,
                Tr("When checked, mining windows will start minimized."));

            // Electricity cost
            toolTip1.SetToolTip(label_ElectricityCost, Tr("Set this to a positive value to factor in electricity costs when switching.\nValue is cost per kW-hour in your chosen display currency.\nSet to 0 to disable power switching functionality."));
            toolTip1.SetToolTip(textBox_ElectricityCost, Tr("Set this to a positive value to factor in electricity costs when switching.\nValue is cost per kW-hour in your chosen display currency.\nSet to 0 to disable power switching functionality."));
            toolTip1.SetToolTip(pictureBox_ElectricityCost, Tr("Set this to a positive value to factor in electricity costs when switching.\nValue is cost per kW-hour in your chosen display currency.\nSet to 0 to disable power switching functionality."));

            SetToolTip("Form_Settings_ToolTip_RunEthlargement", checkBox_RunEthlargement, pictureBox_RunEthlargement);

            Text = Tr("Settings");

            algorithmSettingsControl1.InitLocale(toolTip1);
        }

        private void SetToolTip(string internationalKey, params Control[] controls)
        {
            foreach (var control in controls)
            {
                toolTip1.SetToolTip(control, Tr(internationalKey));
            }
        }

        #region Form this

        private void InitializeFormTranslations()
        {
            buttonDefaults.Text = Tr("&Defaults");
            buttonSaveClose.Text = Tr("&Save and Close");
            buttonCloseNoSave.Text = Tr("&Close without Saving");
        }

        #endregion //Form this

        #region Tab General

        private void InitializeGeneralTabTranslations()
        {
            checkBox_DebugConsole.Text = Tr("Debug Console");
            checkBox_AutoStartMining.Text = Tr("Autostart Mining");
            checkBox_HideMiningWindows.Text = Tr("Hide Mining Windows");
            checkBox_MinimizeToTray.Text = Tr("Minimize To Tray");
            checkBox_DisableDetectionNVIDIA.Text =
                string.Format(Tr("Disable Detection of {0}"), "NVIDIA");
            checkBox_DisableDetectionAMD.Text =
                string.Format(Tr("Disable Detection of {0}"), "AMD");
            checkBox_AutoScaleBTCValues.Text = Tr("Autoscale BTC Values");
            checkBox_StartMiningWhenIdle.Text = Tr("Start Mining When Idle");
            checkBox_ShowDriverVersionWarning.Text =
                Tr("Show Driver Version Warning");
            checkBox_DisableWindowsErrorReporting.Text =
                Tr("Disable Windows Error Reporting");
            checkBox_ShowInternetConnectionWarning.Text =
                Tr("Show Internet Connection Warning");
            checkBox_Use3rdPartyMiners.Text = Tr("Enable 3rd Party Miners");
            checkBox_NVIDIAP0State.Text = Tr("NVIDIA P0 State");
            checkBox_LogToFile.Text = Tr("Log To File");
            checkBox_AMD_DisableAMDTempControl.Text =
                Tr("Disable AMD Temperature Control");
            checkBox_AllowMultipleInstances.Text =
                Tr("Allow Multiple Instances");
            checkBox_RunAtStartup.Text = Tr("Run With Windows");
            checkBox_MinimizeMiningWindows.Text = Tr("Minimize Mining Windows");
            checkBox_UseIFTTT.Text = Tr("Use IFTTT");
            checkBox_RunScriptOnCUDA_GPU_Lost.Text =
                Tr("Run script when CUDA GPU is lost");

            label_Language.Text = Tr("Language") + ":";
            label_BitcoinAddress.Text = Tr("Bitcoin Address") + ":";
            label_WorkerName.Text = Tr("Worker Name") + ":";
            label_ServiceLocation.Text = Tr("Service location") + ":";
            {
                comboBox_ServiceLocation.Items[0] = Tr("Europe - Amsterdam");
                comboBox_ServiceLocation.Items[1] = Tr("USA - San Jose");
                comboBox_ServiceLocation.Items[2] = Tr("China - Hong Kong");
                comboBox_ServiceLocation.Items[3] = Tr("Japan - Tokyo");
                comboBox_ServiceLocation.Items[4] = Tr("India - Chennai");
                comboBox_ServiceLocation.Items[5] = Tr("Brazil - Sao Paulo");
            }
            label_MinIdleSeconds.Text = Tr("Minimum Idle [s]") + ":";
            label_MinerRestartDelayMS.Text = Tr("Miner Restart Delay [ms]") + ":";
            label_MinerAPIQueryInterval.Text =
                Tr("Miner API Query Interval [s]") + ":";
            label_LogMaxFileSize.Text = Tr("Log Max File Size [bytes]") + ":";
            
            label_SwitchMaxSeconds.Text =
                Tr("Switch Maximum [s]") + ":";
            label_SwitchMinSeconds.Text = Tr("Switch Minimum [s]") + ":";

            label_ethminerDefaultBlockHeight.Text =
                Tr("Ethminer Default Block Height") + ":";
            label_DagGeneration.Text = Tr("Dag Load Mode") + ":";
            label_APIBindPortStart.Text = Tr("API Bind port pool start") + ":";

            label_MinProfit.Text = Tr("Minimum Profit ($/day)") + ":";

            label_displayCurrency.Text = Tr("Display Currency");

            label_IFTTTAPIKey.Text = Tr("IFTTT API Key");

            label_ElectricityCost.Text = Tr("Electricity Cost (/KWh)");

            // Benchmark time limits
            // internationalization change
            groupBoxBenchmarkTimeLimits.Text =
                Tr("Benchmark Time Limits") + ":";
            benchmarkLimitControlCPU.GroupName =
                Tr("(CPU) [s]") + ":";
            benchmarkLimitControlNVIDIA.GroupName =
                Tr("(NVIDIA) [s]") + ":";
            benchmarkLimitControlAMD.GroupName =
                Tr("(AMD) [s]") + ":";
            // moved from constructor because of editor
            benchmarkLimitControlCPU.InitLocale();
            benchmarkLimitControlNVIDIA.InitLocale();
            benchmarkLimitControlAMD.InitLocale();

            // device enabled listview translation
            devicesListViewEnableControl1.InitLocale();
            algorithmsListView1.InitLocale();

            // Setup Tooltips CPU
            label_CPU0_ForceCPUExtension.Text =
                Tr("Force CPU Extension") + ":";
            // new translations
            tabControlGeneral.TabPages[0].Text = Tr("General");
            tabControlGeneral.TabPages[1].Text = Tr("Advanced");
            tabControlGeneral.TabPages[2].Text = Tr("Devices/Algorithms");
            groupBox_Main.Text = Tr("Main");
            groupBox_Localization.Text = Tr("Localization");
            groupBox_Logging.Text = Tr("Logging");
            groupBox_Misc.Text = Tr("Misc");
            // advanced
            groupBox_Miners.Text = Tr("Miners");
            groupBoxBenchmarkTimeLimits.Text =
                Tr("Benchmark Time Limits:");

            checkBox_DisableDefaultOptimizations.Text =
                Tr("Disable Default Optimizations");
            checkBox_IdleWhenNoInternetAccess.Text =
                Tr("Idle When No Internet Access");

            label_SwitchProfitabilityThreshold.Text =
                Tr("Switch Profitability Threshold");

            checkBox_RunEthlargement.Text = Tr("Run Ethlargement");
            checkBox_RunEthlargement.Enabled = Helpers.IsElevated && ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
        }

        private void InitializeGeneralTabCallbacks()
        {
            // Add EventHandler for all the general tab's checkboxes
            {
                checkBox_AutoScaleBTCValues.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_DisableDetectionAMD.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_DisableDetectionNVIDIA.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_MinimizeToTray.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_HideMiningWindows.CheckedChanged += CheckBox_HideMiningWindows_CheckChanged;
                checkBox_DebugConsole.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_ShowDriverVersionWarning.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_DisableWindowsErrorReporting.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_ShowInternetConnectionWarning.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_StartMiningWhenIdle.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_NVIDIAP0State.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_LogToFile.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_AutoStartMining.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_AllowMultipleInstances.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_MinimizeMiningWindows.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_UseIFTTT.CheckedChanged += CheckBox_UseIFTTT_CheckChanged;
                checkBox_RunScriptOnCUDA_GPU_Lost.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_RunEthlargement.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
            }
            // Add EventHandler for all the general tab's textboxes
            {
                textBox_BitcoinAddress.Leave += GeneralTextBoxes_Leave;
                textBox_WorkerName.Leave += GeneralTextBoxes_Leave;
                textBox_IFTTTKey.Leave += GeneralTextBoxes_Leave;
                // these are ints only
                textBox_SwitchMaxSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_SwitchMinSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_MinerAPIQueryInterval.Leave += GeneralTextBoxes_Leave;
                textBox_MinerRestartDelayMS.Leave += GeneralTextBoxes_Leave;
                textBox_MinIdleSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_LogMaxFileSize.Leave += GeneralTextBoxes_Leave;
                textBox_ethminerDefaultBlockHeight.Leave += GeneralTextBoxes_Leave;
                textBox_APIBindPortStart.Leave += GeneralTextBoxes_Leave;
                textBox_MinProfit.Leave += GeneralTextBoxes_Leave;
                textBox_ElectricityCost.Leave += GeneralTextBoxes_Leave;
                // set int only keypress
                textBox_SwitchMaxSeconds.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_SwitchMinSeconds.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_MinerAPIQueryInterval.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_MinerRestartDelayMS.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_MinIdleSeconds.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_LogMaxFileSize.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_ethminerDefaultBlockHeight.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_APIBindPortStart.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                // set double only keypress
                textBox_MinProfit.KeyPress += TextBoxKeyPressEvents.TextBoxDoubleOnly_KeyPress;
                textBox_ElectricityCost.KeyPress += TextBoxKeyPressEvents.TextBoxDoubleOnly_KeyPress;
            }
            // Add EventHandler for all the general tab's textboxes
            {
                comboBox_Language.Leave += GeneralComboBoxes_Leave;
                comboBox_ServiceLocation.Leave += GeneralComboBoxes_Leave;
                comboBox_TimeUnit.Leave += GeneralComboBoxes_Leave;
                comboBox_DagLoadMode.Leave += GeneralComboBoxes_Leave;
            }

            // CPU exceptions
            comboBox_CPU0_ForceCPUExtension.SelectedIndex = (int) ConfigManager.GeneralConfig.ForceCPUExtension;
            comboBox_CPU0_ForceCPUExtension.SelectedIndexChanged +=
                ComboBox_CPU0_ForceCPUExtension_SelectedIndexChanged;
            // fill dag dropdown
            comboBox_DagLoadMode.Items.Clear();
            for (var i = 0; i < (int) DagGenerationType.END; ++i)
            {
                comboBox_DagLoadMode.Items.Add(MinerEtherum.GetDagGenerationString((DagGenerationType) i));
            }

            // set selected
            comboBox_DagLoadMode.SelectedIndex = (int) ConfigManager.GeneralConfig.EthminerDagGenerationType;
        }

        private void InitializeGeneralTabFieldValuesReferences()
        {
            // Checkboxes set checked value
            {
                checkBox_DebugConsole.Checked = ConfigManager.GeneralConfig.DebugConsole;
                checkBox_AutoStartMining.Checked = ConfigManager.GeneralConfig.AutoStartMining;
                checkBox_HideMiningWindows.Checked = ConfigManager.GeneralConfig.HideMiningWindows;
                checkBox_MinimizeToTray.Checked = ConfigManager.GeneralConfig.MinimizeToTray;
                checkBox_DisableDetectionNVIDIA.Checked =
                    ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA;
                checkBox_DisableDetectionAMD.Checked = ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD;
                checkBox_AutoScaleBTCValues.Checked = ConfigManager.GeneralConfig.AutoScaleBTCValues;
                checkBox_StartMiningWhenIdle.Checked = ConfigManager.GeneralConfig.StartMiningWhenIdle;
                checkBox_ShowDriverVersionWarning.Checked = ConfigManager.GeneralConfig.ShowDriverVersionWarning;
                checkBox_DisableWindowsErrorReporting.Checked =
                    ConfigManager.GeneralConfig.DisableWindowsErrorReporting;
                checkBox_ShowInternetConnectionWarning.Checked =
                    ConfigManager.GeneralConfig.ShowInternetConnectionWarning;
                checkBox_NVIDIAP0State.Checked = ConfigManager.GeneralConfig.NVIDIAP0State;
                checkBox_LogToFile.Checked = ConfigManager.GeneralConfig.LogToFile;
                checkBox_AMD_DisableAMDTempControl.Checked = ConfigManager.GeneralConfig.DisableAMDTempControl;
                checkBox_DisableDefaultOptimizations.Checked = ConfigManager.GeneralConfig.DisableDefaultOptimizations;
                checkBox_IdleWhenNoInternetAccess.Checked = ConfigManager.GeneralConfig.IdleWhenNoInternetAccess;
                checkBox_Use3rdPartyMiners.Checked =
                    ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
                checkBox_AllowMultipleInstances.Checked = ConfigManager.GeneralConfig.AllowMultipleInstances;
                checkBox_RunAtStartup.Checked = IsInStartupRegistry();
                checkBox_MinimizeMiningWindows.Checked = ConfigManager.GeneralConfig.MinimizeMiningWindows;
                checkBox_MinimizeMiningWindows.Enabled = !ConfigManager.GeneralConfig.HideMiningWindows;
                checkBox_UseIFTTT.Checked = ConfigManager.GeneralConfig.UseIFTTT;
                checkBox_RunScriptOnCUDA_GPU_Lost.Checked = ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost;
                checkBox_RunEthlargement.Checked = ConfigManager.GeneralConfig.UseEthlargement;
            }

            // Textboxes
            {
                textBox_BitcoinAddress.Text = ConfigManager.GeneralConfig.BitcoinAddress;
                textBox_WorkerName.Text = ConfigManager.GeneralConfig.WorkerName;
                textBox_IFTTTKey.Text = ConfigManager.GeneralConfig.IFTTTKey;
                textBox_IFTTTKey.Enabled = ConfigManager.GeneralConfig.UseIFTTT;
                textBox_SwitchMaxSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Upper.ToString();
                textBox_SwitchMinSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Lower.ToString();
                textBox_MinerAPIQueryInterval.Text = ConfigManager.GeneralConfig.MinerAPIQueryInterval.ToString();
                textBox_MinerRestartDelayMS.Text = ConfigManager.GeneralConfig.MinerRestartDelayMS.ToString();
                textBox_MinIdleSeconds.Text = ConfigManager.GeneralConfig.MinIdleSeconds.ToString();
                textBox_LogMaxFileSize.Text = ConfigManager.GeneralConfig.LogMaxFileSize.ToString();
                textBox_ethminerDefaultBlockHeight.Text =
                    ConfigManager.GeneralConfig.ethminerDefaultBlockHeight.ToString();
                textBox_APIBindPortStart.Text = ConfigManager.GeneralConfig.ApiBindPortPoolStart.ToString();
                textBox_MinProfit.Text =
                    ConfigManager.GeneralConfig.MinimumProfit.ToString("F2").Replace(',', '.'); // force comma;
                textBox_SwitchProfitabilityThreshold.Text = ConfigManager.GeneralConfig.SwitchProfitabilityThreshold
                    .ToString("F2").Replace(',', '.'); // force comma;
                textBox_ElectricityCost.Text = ConfigManager.GeneralConfig.KwhPrice.ToString("0.0000");
            }

            // set custom control referances
            {
                benchmarkLimitControlCPU.TimeLimits = ConfigManager.GeneralConfig.BenchmarkTimeLimits.CPU;
                benchmarkLimitControlNVIDIA.TimeLimits = ConfigManager.GeneralConfig.BenchmarkTimeLimits.NVIDIA;
                benchmarkLimitControlAMD.TimeLimits = ConfigManager.GeneralConfig.BenchmarkTimeLimits.AMD;

                // here we want all devices
                devicesListViewEnableControl1.SetComputeDevices(AvailableDevices.Devices);
                devicesListViewEnableControl1.SetAlgorithmsListView(algorithmsListView1);
                devicesListViewEnableControl1.IsSettingsCopyEnabled = true;
            }

            // Add language selections list
            {
                var langs = GetAvailableLanguagesNames();

                comboBox_Language.Items.Clear();
                foreach(var lang in langs)
                {
                    comboBox_Language.Items.Add(lang);
                }
            }

            // Add time unit selection list
            {
                var timeunits = new Dictionary<TimeUnitType, string>();

                foreach (TimeUnitType timeunit in Enum.GetValues(typeof(TimeUnitType)))
                {
                    timeunits.Add(timeunit, Tr(timeunit.ToString()));
                    comboBox_TimeUnit.Items.Add(timeunits[timeunit]);
                }
            }

            // ComboBox
            {
                comboBox_Language.SelectedIndex = GetLanguageIndexFromCode(ConfigManager.GeneralConfig.Language);


                comboBox_ServiceLocation.SelectedIndex = ConfigManager.GeneralConfig.ServiceLocation;
                comboBox_TimeUnit.SelectedItem = Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                currencyConverterCombobox.SelectedItem = ConfigManager.GeneralConfig.DisplayCurrency;
            }
        }

        private void InitializeGeneralTab()
        {
            InitializeGeneralTabTranslations();
            InitializeGeneralTabCallbacks();
            InitializeGeneralTabFieldValuesReferences();
        }

        #endregion //Tab General

        #region Tab Devices

        private void InitializeDevicesTab()
        {
            InitializeDevicesCallbacks();
        }

        private void InitializeDevicesCallbacks()
        {
            devicesListViewEnableControl1.SetDeviceSelectionChangedCallback(DevicesListView1_ItemSelectionChanged);
        }

        #endregion //Tab Devices

        #endregion // Initializations

        #region Form Callbacks

        #region Tab General

        private void GeneralCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            // indicate there has been a change
            IsChange = true;
            ConfigManager.GeneralConfig.DebugConsole = checkBox_DebugConsole.Checked;
            ConfigManager.GeneralConfig.AutoStartMining = checkBox_AutoStartMining.Checked;
            ConfigManager.GeneralConfig.HideMiningWindows = checkBox_HideMiningWindows.Checked;
            ConfigManager.GeneralConfig.MinimizeToTray = checkBox_MinimizeToTray.Checked;
            ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA =
                checkBox_DisableDetectionNVIDIA.Checked;
            ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD = checkBox_DisableDetectionAMD.Checked;
            ConfigManager.GeneralConfig.AutoScaleBTCValues = checkBox_AutoScaleBTCValues.Checked;
            ConfigManager.GeneralConfig.StartMiningWhenIdle = checkBox_StartMiningWhenIdle.Checked;
            ConfigManager.GeneralConfig.ShowDriverVersionWarning = checkBox_ShowDriverVersionWarning.Checked;
            ConfigManager.GeneralConfig.DisableWindowsErrorReporting = checkBox_DisableWindowsErrorReporting.Checked;
            ConfigManager.GeneralConfig.ShowInternetConnectionWarning = checkBox_ShowInternetConnectionWarning.Checked;
            ConfigManager.GeneralConfig.NVIDIAP0State = checkBox_NVIDIAP0State.Checked;
            ConfigManager.GeneralConfig.LogToFile = checkBox_LogToFile.Checked;
            ConfigManager.GeneralConfig.IdleWhenNoInternetAccess = checkBox_IdleWhenNoInternetAccess.Checked;
            ConfigManager.GeneralConfig.UseIFTTT = checkBox_UseIFTTT.Checked;
            ConfigManager.GeneralConfig.AllowMultipleInstances = checkBox_AllowMultipleInstances.Checked;
            ConfigManager.GeneralConfig.MinimizeMiningWindows = checkBox_MinimizeMiningWindows.Checked;
            ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost = checkBox_RunScriptOnCUDA_GPU_Lost.Checked;
            ConfigManager.GeneralConfig.UseEthlargement = checkBox_RunEthlargement.Checked;
        }

        private void CheckBox_AMD_DisableAMDTempControl_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;

            // indicate there has been a change
            IsChange = true;
            ConfigManager.GeneralConfig.DisableAMDTempControl = checkBox_AMD_DisableAMDTempControl.Checked;
            foreach (var cDev in AvailableDevices.Devices)
            {
                if (cDev.DeviceType == DeviceType.AMD)
                {
                    foreach (var algorithm in cDev.GetAlgorithmSettings())
                    {
                        if (algorithm.NiceHashID != AlgorithmType.DaggerHashimoto)
                        {
                            algorithm.ExtraLaunchParameters += AmdGpuDevice.TemperatureParam;
                            algorithm.ExtraLaunchParameters = ExtraLaunchParametersParser.ParseForMiningPair(
                                new MiningPair(cDev, algorithm), algorithm.NiceHashID, DeviceType.AMD, false);
                        }
                    }
                }
            }
        }

        private void CheckBox_DisableDefaultOptimizations_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;

            // indicate there has been a change
            IsChange = true;
            ConfigManager.GeneralConfig.DisableDefaultOptimizations = checkBox_DisableDefaultOptimizations.Checked;
            if (ConfigManager.GeneralConfig.DisableDefaultOptimizations)
            {
                foreach (var cDev in AvailableDevices.Devices)
                {
                    foreach (var algorithm in cDev.GetAlgorithmSettings())
                    {
                        algorithm.ExtraLaunchParameters = "";
                        if (cDev.DeviceType == DeviceType.AMD && algorithm.NiceHashID != AlgorithmType.DaggerHashimoto)
                        {
                            algorithm.ExtraLaunchParameters += AmdGpuDevice.TemperatureParam;
                            algorithm.ExtraLaunchParameters = ExtraLaunchParametersParser.ParseForMiningPair(
                                new MiningPair(cDev, algorithm), algorithm.NiceHashID, cDev.DeviceType, false);
                        }
                    }
                }
            }
            else
            {
                foreach (var cDev in AvailableDevices.Devices)
                {
                    if (cDev.DeviceType == DeviceType.CPU) continue; // cpu has no defaults
                    var deviceDefaultsAlgoSettings = GroupAlgorithms.CreateForDeviceList(cDev);
                    foreach (var defaultAlgoSettings in deviceDefaultsAlgoSettings)
                    {
                        var toSetAlgo = cDev.GetAlgorithm(defaultAlgoSettings);
                        if (toSetAlgo != null)
                        {
                            toSetAlgo.ExtraLaunchParameters = defaultAlgoSettings.ExtraLaunchParameters;
                            toSetAlgo.ExtraLaunchParameters = ExtraLaunchParametersParser.ParseForMiningPair(
                                new MiningPair(cDev, toSetAlgo), toSetAlgo.NiceHashID, cDev.DeviceType, false);
                        }
                    }
                }
            }
        }

        private void CheckBox_RunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            _isStartupChanged = true;
        }

        private bool IsInStartupRegistry()
        {
            // Value is stored in registry
            var startVal = "";
            try
            {
                startVal = (string) _rkStartup?.GetValue(Application.ProductName, "");
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("REGISTRY", e.ToString());
            }

            return startVal == Application.ExecutablePath;
        }

        private void GeneralTextBoxes_Leave(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;
            if (ConfigManager.GeneralConfig.BitcoinAddress != textBox_BitcoinAddress.Text.Trim()) _isCredChange = true;
            ConfigManager.GeneralConfig.BitcoinAddress = textBox_BitcoinAddress.Text.Trim();
            if (ConfigManager.GeneralConfig.WorkerName != textBox_WorkerName.Text.Trim()) _isCredChange = true;
            ConfigManager.GeneralConfig.WorkerName = textBox_WorkerName.Text.Trim();
            
            ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Upper =
                Helpers.ParseInt(textBox_SwitchMaxSeconds.Text);
            ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Lower = Helpers.ParseInt(textBox_SwitchMinSeconds.Text);
            ConfigManager.GeneralConfig.MinerAPIQueryInterval = Helpers.ParseInt(textBox_MinerAPIQueryInterval.Text);
            ConfigManager.GeneralConfig.MinerRestartDelayMS = Helpers.ParseInt(textBox_MinerRestartDelayMS.Text);
            ConfigManager.GeneralConfig.MinIdleSeconds = Helpers.ParseInt(textBox_MinIdleSeconds.Text);
            ConfigManager.GeneralConfig.LogMaxFileSize = Helpers.ParseLong(textBox_LogMaxFileSize.Text);
            ConfigManager.GeneralConfig.ethminerDefaultBlockHeight =
                Helpers.ParseInt(textBox_ethminerDefaultBlockHeight.Text);
            ConfigManager.GeneralConfig.ApiBindPortPoolStart = Helpers.ParseInt(textBox_APIBindPortStart.Text);
            // min profit
            ConfigManager.GeneralConfig.MinimumProfit = Helpers.ParseDouble(textBox_MinProfit.Text);
            ConfigManager.GeneralConfig.SwitchProfitabilityThreshold =
                Helpers.ParseDouble(textBox_SwitchProfitabilityThreshold.Text);

            ConfigManager.GeneralConfig.IFTTTKey = textBox_IFTTTKey.Text.Trim();

            ConfigManager.GeneralConfig.KwhPrice = Helpers.ParseDouble(textBox_ElectricityCost.Text);

            // Fix bounds
            ConfigManager.GeneralConfig.FixSettingBounds();
            // update strings
            textBox_MinProfit.Text =
                ConfigManager.GeneralConfig.MinimumProfit.ToString("F2").Replace(',', '.'); // force comma
            textBox_SwitchProfitabilityThreshold.Text = ConfigManager.GeneralConfig.SwitchProfitabilityThreshold
                .ToString("F2").Replace(',', '.'); // force comma
            textBox_SwitchMaxSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Upper.ToString();
            textBox_SwitchMinSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Lower.ToString();
            textBox_MinerAPIQueryInterval.Text = ConfigManager.GeneralConfig.MinerAPIQueryInterval.ToString();
            textBox_MinerRestartDelayMS.Text = ConfigManager.GeneralConfig.MinerRestartDelayMS.ToString();
            textBox_MinIdleSeconds.Text = ConfigManager.GeneralConfig.MinIdleSeconds.ToString();
            textBox_LogMaxFileSize.Text = ConfigManager.GeneralConfig.LogMaxFileSize.ToString();
            textBox_ethminerDefaultBlockHeight.Text = ConfigManager.GeneralConfig.ethminerDefaultBlockHeight.ToString();
            textBox_APIBindPortStart.Text = ConfigManager.GeneralConfig.ApiBindPortPoolStart.ToString();
            textBox_ElectricityCost.Text = ConfigManager.GeneralConfig.KwhPrice.ToString("0.0000");
        }

        private void GeneralComboBoxes_Leave(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;

            ConfigManager.GeneralConfig.Language = Translations.GetLanguageCodeFromIndex(comboBox_Language.SelectedIndex);
            ConfigManager.GeneralConfig.ServiceLocation = comboBox_ServiceLocation.SelectedIndex;
            ConfigManager.GeneralConfig.TimeUnit = (TimeUnitType) comboBox_TimeUnit.SelectedIndex;
            ConfigManager.GeneralConfig.EthminerDagGenerationType =
                (DagGenerationType) comboBox_DagLoadMode.SelectedIndex;
        }

        private void ComboBox_CPU0_ForceCPUExtension_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cmbbox = (ComboBox) sender;
            ConfigManager.GeneralConfig.ForceCPUExtension = (CpuExtensionType) cmbbox.SelectedIndex;
        }

        #endregion //Tab General


        #region Tab Device

        private void DevicesListView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            algorithmSettingsControl1.Deselect();
            // show algorithms
            _selectedComputeDevice =
                AvailableDevices.GetCurrentlySelectedComputeDevice(e.ItemIndex, ShowUniqueDeviceList);
            algorithmsListView1.SetAlgorithms(_selectedComputeDevice, _selectedComputeDevice.Enabled);
            groupBoxAlgorithmSettings.Text = string.Format(Tr("Algorithm settings for {0} :"),
                _selectedComputeDevice.Name);
        }

        #endregion //Tab Device


        private void ToolTip1_Popup(object sender, PopupEventArgs e)
        {
            toolTip1.ToolTipTitle = Tr("Explanation");
        }

        #region Form Buttons

        private void ButtonDefaults_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(Tr("Are you sure you would like to set everything back to defaults? This will restart NiceHash Miner Legacy automatically."),
                Tr("Set default settings?"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                IsChange = true;
                IsChangeSaved = true;
                SetLanguage("en");
                ConfigManager.GeneralConfig.SetDefaults();
                InitializeGeneralTabFieldValuesReferences();
                InitializeGeneralTabTranslations();
            }
        }

        private void ButtonSaveClose_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Tr("Settings saved!"),
                Tr("Settings saved!"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            IsChange = true;
            IsChangeSaved = true;

            if (_isCredChange)
            {
                NiceHashStats.SetCredentials(ConfigManager.GeneralConfig.BitcoinAddress.Trim(),
                    ConfigManager.GeneralConfig.WorkerName.Trim());
            }

            Close();
        }

        private void ButtonCloseNoSave_Click(object sender, EventArgs e)
        {
            IsChangeSaved = false;
            Close();
        }

        #endregion // Form Buttons

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsChange && !IsChangeSaved)
            {
                var result = MessageBox.Show(Tr("Warning! You are choosing to close settings without saving. Are you sure you would like to continue?"),
                    Tr("Warning!"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // check restart parameters change
            IsRestartNeeded = ConfigManager.IsRestartNeeded();

            if (IsChangeSaved)
            {
                ConfigManager.GeneralConfigFileCommit();
                ConfigManager.CommitBenchmarks();

                SetLanguage(ConfigManager.GeneralConfig.Language);

                if (_isStartupChanged)
                {
                    // Commit to registry
                    try
                    {
                        if (checkBox_RunAtStartup.Checked)
                        {
                            // Add NHML to startup registry
                            _rkStartup?.SetValue(Application.ProductName, Application.ExecutablePath);
                        }
                        else
                        {
                            _rkStartup?.DeleteValue(Application.ProductName, false);
                        }
                    }
                    catch (Exception er)
                    {
                        Helpers.ConsolePrint("REGISTRY", er.ToString());
                    }
                }
            }
            else
            {
                ConfigManager.RestoreBackup();
            }
        }

        private void CurrencyConverterCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = currencyConverterCombobox.SelectedItem.ToString();
            ConfigManager.GeneralConfig.DisplayCurrency = selected;
        }

        #endregion Form Callbacks

        private void TabControlGeneral_Selected(object sender, TabControlEventArgs e)
        {
            // set first device selected {
            if (AvailableDevices.Devices.Count > 0)
            {
                algorithmSettingsControl1.Deselect();
            }
        }

        private void CheckBox_Use3rdPartyMiners_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            if (checkBox_Use3rdPartyMiners.Checked)
            {
                // Show TOS
                Form tos = new Form_3rdParty_TOS();
                tos.ShowDialog(this);
                checkBox_Use3rdPartyMiners.Checked =
                    ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
            }
            else
            {
                ConfigManager.GeneralConfig.Use3rdPartyMiners = Use3rdPartyMiners.NO;
            }
        }

        private void CheckBox_HideMiningWindows_CheckChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;
            ConfigManager.GeneralConfig.HideMiningWindows = checkBox_HideMiningWindows.Checked;
            checkBox_MinimizeMiningWindows.Enabled = !checkBox_HideMiningWindows.Checked;
        }

        private void CheckBox_UseIFTTT_CheckChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;

            ConfigManager.GeneralConfig.UseIFTTT = checkBox_UseIFTTT.Checked;

            textBox_IFTTTKey.Enabled = checkBox_UseIFTTT.Checked;
        }
    }
}

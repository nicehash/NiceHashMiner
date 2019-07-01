using Microsoft.Win32;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Interfaces.DataVisualizer;

using static NiceHashMiner.Translations;
using NiceHashMiner.Miners.IntegratedPlugins;
using NiceHashMiner.Plugin;
using NiceHashMinerLegacy.Common;

namespace NiceHashMiner.Forms
{
    public partial class Form_Settings : Form, FormHelpers.ICustomTranslate, IDataVisualizer
    {
        private readonly bool _isInitFinished = false;
        public bool IsRestartNeeded { get; private set; }
        public bool SetDefaults { get; private set; } = false;

        // most likely we wil have settings only per unique devices
        private const bool ShowUniqueDeviceList = true;

        private ComputeDevice _selectedComputeDevice;

        public Form_Settings()
        {
            InitializeComponent();
            ApplicationStateManager.SubscribeStateDisplayer(this);

            Icon = Properties.Resources.logo;

            // backup settings
            ConfigManager.CreateBackup();

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

            checkBox_DebugConsole.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.DebugConsole));
            checkBox_AutoStartMining.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.AutoStartMining));
            checkBox_HideMiningWindows.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.HideMiningWindows));
            checkBox_MinimizeToTray.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.MinimizeToTray));
            checkBox_AutoScaleBTCValues.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.AutoScaleBTCValues), false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_ShowDriverVersionWarning.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.ShowDriverVersionWarning));
            checkBox_DisableWindowsErrorReporting.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.DisableWindowsErrorReporting));
            checkBox_ShowInternetConnectionWarning.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.ShowInternetConnectionWarning));
            checkBox_NVIDIAP0State.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.NVIDIAP0State));
            checkBox_LogToFile.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.LogToFile));
            checkBox_AllowMultipleInstances.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.AllowMultipleInstances));
            checkBox_MinimizeMiningWindows.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.MinimizeMiningWindows));
            checkBox_RunScriptOnCUDA_GPU_Lost.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost));
            checkBox_RunAtStartup.DataBindings.Add("Checked", ConfigManager.RunAtStartup , nameof(ConfigManager.RunAtStartup.Enabled));

            // idle mining
            checkBox_IdleWhenNoInternetAccess.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.IdleWhenNoInternetAccess));
            checkBox_StartMiningWhenIdle.DataBindings.Add("Checked", ConfigManager.IdleMiningSettings, nameof(ConfigManager.IdleMiningSettings.StartMiningWhenIdle), false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_IdleType.DataBindings.Add("Enabled", ConfigManager.IdleMiningSettings, nameof(ConfigManager.IdleMiningSettings.StartMiningWhenIdle), false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_IdleType.DataBindings.Add("SelectedIndex", ConfigManager.IdleMiningSettings, nameof(ConfigManager.IdleMiningSettings.IdleCheckTypeIndex), false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_MinIdleSeconds.DataBindings.Add("Enabled", ConfigManager.IdleMiningSettings, nameof(ConfigManager.IdleMiningSettings.IsIdleCheckTypeInputTimeout), false, DataSourceUpdateMode.OnPropertyChanged);

            // comboBox indexes
            comboBox_Language.DataBindings.Add("SelectedIndex", ConfigManager.TranslationsSettings, nameof(ConfigManager.TranslationsSettings.LanguageIndex), false, DataSourceUpdateMode.OnPropertyChanged);

            // IFTTT textbox
            checkBox_UseIFTTT.DataBindings.Add("Checked", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.UseIFTTT), false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_IFTTTKey.DataBindings.Add("Enabled", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.UseIFTTT), false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_IFTTTKey.DataBindings.Add("Text", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.IFTTTKey), false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_RunEthlargement.DataBindings.Add("Checked", ThirdPartyMinerSettings.Instance, nameof(ThirdPartyMinerSettings.Instance.UseEthlargement), false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_RunEthlargement.DataBindings.Add("Enabled", ThirdPartyMinerSettings.Instance, nameof(ThirdPartyMinerSettings.Instance.CanUseEthlargement), false, DataSourceUpdateMode.OnPropertyChanged);


            checkBox_Use3rdPartyMiners.CheckedChanged += CheckBox_Use3rdPartyMiners_CheckedChanged;
            //checkBox_RunEthlargement.CheckedChanged += CheckBox_RunEthlargement_CheckedChanged;

            // At the very end set to true
            _isInitFinished = true;

            
            FormHelpers.TranslateFormControls(this);
        }

        #region Initializations

        void FormHelpers.ICustomTranslate.CustomTranslate()
        {
            // Setup Tooltips
            SetToolTip(Tr("Changes the default language for NiceHash Miner Legacy."),
                comboBox_Language, label_Language, pictureBox_Language);

            SetToolTip(Tr("When checked, it displays debug console."),
                checkBox_DebugConsole, pictureBox_DebugConsole);

            SetToolTip(Tr("Sets the time unit to report BTC rates."),
                comboBox_TimeUnit, label_TimeUnit, pictureBox_TimeUnit);

            SetToolTip(Tr("When checked, sgminer, ccminer, cpuminer and ethminer console windows will be hidden."),
                checkBox_HideMiningWindows, pictureBox_HideMiningWindows);

            SetToolTip(Tr("When checked, NiceHash Miner Legacy will minimize to tray."),
                checkBox_MinimizeToTray, pictureBox_MinimizeToTray);

            SetToolTip(Tr("Use 3rd party closed-source mining software for higher profitability. Usage is on your own responsibility."),
                checkBox_Use3rdPartyMiners, pictureBox_Use3rdPartyMiners);

            SetToolTip(Tr("When unchecked NiceHash Miner Legacy will allow only one instance running (it will close a new started instance if there is an opened instance running)."),
                checkBox_AllowMultipleInstances, pictureBox_AllowMultipleInstances);

            SetToolTip(Tr("If set to any value more than 0 (USD), NiceHash Miner Legacy will stop mining\nif the calculated profit falls below the set amount."),
                label_MinProfit, pictureBox_MinProfit, textBox_MinProfit);

            SetToolTip(Tr("Upper bound for the randomly chosen profit check interval.\nProfit may be checked multiple times before a switch is allowed, so don't set too high."),
                textBox_SwitchMaxSeconds, label_SwitchMaxSeconds, pictureBox_SwitchMaxSeconds);

            SetToolTip(Tr("Lower bound for the randomly chosen profit check interval.\nDo not set too low."),
                textBox_SwitchMinSeconds, label_SwitchMinSeconds, pictureBox_SwitchMinSeconds);

            SetToolTip(Tr("API query interval for ccminer, sgminer cpuminer and ethminer."),
                textBox_MinerAPIQueryInterval, label_MinerAPIQueryInterval, pictureBox_MinerAPIQueryInterval);

            SetToolTip(Tr("Amount of time (in milliseconds) that NiceHash Miner Legacy will wait before restarting the miner."),
                textBox_MinerRestartDelayMS, label_MinerRestartDelayMS, pictureBox_MinerRestartDelayMS);

            SetToolTip(Tr("Set starting port number from which miner API Bind ports will be set for communication."),
                textBox_APIBindPortStart, label_APIBindPortStart, pictureBox_APIBindPortStart);

            SetToolTip(Tr("Check it, if you would like to see the BTC values autoscale to the appropriate scale."),
                checkBox_AutoScaleBTCValues, pictureBox_AutoScaleBTCValues);

            SetToolTip(Tr("Automatically start mining when computer is idle and stop mining when computer is being used."),
                checkBox_StartMiningWhenIdle, pictureBox_StartMiningWhenIdle);

            SetToolTip(Tr("When StartMiningWhenIdle is checked, MinIdleSeconds tells how\nmany seconds computer has to be idle before mining starts."),
                textBox_MinIdleSeconds, label_MinIdleSeconds, pictureBox_MinIdleSeconds);

            SetToolTip(Tr("Check it, to log console output to file."),
                checkBox_LogToFile, pictureBox_LogToFile);

            SetToolTip(Tr("Sets the maximum size for the log file."),
                textBox_LogMaxFileSize, label_LogMaxFileSize, pictureBox_LogMaxFileSize);

            SetToolTip(Tr("When checked, NiceHash Miner Legacy would issue a warning if\na less optimal version of a driver is installed."),
                checkBox_ShowDriverVersionWarning, pictureBox_ShowDriverVersionWarning);

            SetToolTip(Tr("When checked, in the event of a miner crash,\nNiceHash Miner Legacy would still be able to restart the miner again as it is not blocked by Windows error message.\nIt is recommended to have this setting checked for uninterrupted mining process because mining programs are not 100% stable."),
                checkBox_DisableWindowsErrorReporting, pictureBox_DisableWindowsErrorReporting);

            SetToolTip(Tr("When checked, NiceHash Miner Legacy would issue a warning if\nthe internet connection is not available."),
                checkBox_ShowInternetConnectionWarning, pictureBox_ShowInternetConnectionWarning);

            SetToolTip(Tr("When checked, NiceHash Miner Legacy will change all supported NVIDIA GPUs to P0 state.\nThis will slightly increase performance on certain algorithms.\nThis feature needs administrator privileges to be activated."),
                checkBox_NVIDIAP0State, pictureBox_NVIDIAP0State);

            SetToolTip(Tr("When checked, NiceHash Miner Legacy will run OnGPUsLost.bat in case at least one CUDA GPU is lost,\nby default script should restart whole system."),
                checkBox_RunScriptOnCUDA_GPU_Lost, pictureBox_RunScriptOnCUDA_GPU_Lost);

            SetToolTip(Tr("When checked, NiceHash Miner Legacy will run on login."),
                checkBox_RunAtStartup, pictureBox_RunAtStartup);

            SetToolTip(Tr("When checked, NiceHash Miner Legacy will automatically start mining when launched."),
                checkBox_AutoStartMining, pictureBox_AutoStartMining);

            SetToolTip(Tr("Choose what Currency to Display mining profit."),
                label_displayCurrency, pictureBox_displayCurrency, currencyConverterCombobox);

            // internet connection mining check
            SetToolTip(Tr("If enabled NiceHash Miner Legacy will stop mining without internet connectivity"),
                checkBox_IdleWhenNoInternetAccess, pictureBox_IdleWhenNoInternetAccess);

            // IFTTT notification check
            SetToolTip(Tr("If enabled, NiceHash Miner Legacy will use the API Key you provide to notify you when profitability has gone below the profitability you have configured.\nSee instructions for details on configuring this functionality."),
                checkBox_UseIFTTT, pictureBox_UseIFTTT);

            SetToolTip(Tr("Miner will not switch if the profitability is below SwitchProfitabilityThreshold. Value is in percentage [0 - 1]"),
                pictureBox_SwitchProfitabilityThreshold, label_SwitchProfitabilityThreshold);

            SetToolTip(Tr("When checked, mining windows will start minimized."),
                pictureBox_MinimizeMiningWindows, checkBox_MinimizeMiningWindows);

            // Electricity cost
            SetToolTip(Tr("Set this to a positive value to factor in electricity costs when switching.\nValue is cost per kW-hour in your chosen display currency.\nSet to 0 to disable power switching functionality."),
                label_ElectricityCost, textBox_ElectricityCost, pictureBox_ElectricityCost);

            SetToolTip(Tr("Run Ethlargement for Dagger algorithms when supported GPUs are present.\nRequires running NHML as admin and enabling 3rd-party miners."),
                checkBox_RunEthlargement, pictureBox_RunEthlargement);

            SetToolTip(Tr("Choose how to check if computer is idle when start mining on idle is enabled.\nSession Lock will start when the computer is locked (generally when the screen has turned off).\nInput Timeout will start when there has been no system input for the idle time seconds."),
                comboBox_IdleType, label_IdleType, pictureBox_IdleType);

            algorithmSettingsControl1.InitLocale(toolTip1);
        }

        private void SetToolTip(string text, params Control[] controls)
        {
            foreach (var control in controls)
            {
                toolTip1.SetToolTip(control, text);
            }
        }

#region Tab General
// TODO THIS IS LOGIC INSIDE CONTENT
        private void InitializeGeneralTabTranslations()
        {
            foreach (var type in Enum.GetNames(typeof(IdleCheckType)))
            {
                // translations will handle enum names
                comboBox_IdleType.Items.Add(Tr(type));
            }
        }

        private void InitializeGeneralTabCallbacks()
        {
            // Add EventHandler for all the general tab's textboxes
            {
                // these are ints only
                textBox_SwitchMaxSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_SwitchMinSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_MinerAPIQueryInterval.Leave += GeneralTextBoxes_Leave;
                textBox_MinerRestartDelayMS.Leave += GeneralTextBoxes_Leave;
                textBox_MinIdleSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_LogMaxFileSize.Leave += GeneralTextBoxes_Leave;
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
                //textBox_ethminerDefaultBlockHeight.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_APIBindPortStart.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                // set double only keypress
                textBox_MinProfit.KeyPress += TextBoxKeyPressEvents.TextBoxDoubleOnly_KeyPress;
                textBox_ElectricityCost.KeyPress += TextBoxKeyPressEvents.TextBoxDoubleOnly_KeyPress;
            }
            // Add EventHandler for all the general tab's textboxes
            {
                comboBox_Language.Leave += GeneralComboBoxes_Leave;
                comboBox_TimeUnit.Leave += GeneralComboBoxes_Leave;
                comboBox_IdleType.Leave += GeneralComboBoxes_Leave;
            }
        }

        private void InitializeGeneralTabFieldValuesReferences()
        {
            // Checkboxes set checked value
            {
                checkBox_Use3rdPartyMiners.Checked =
                    ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
                checkBox_RunEthlargement.Checked = ConfigManager.GeneralConfig.UseEthlargement;
            }

            // Textboxes
            {
                textBox_SwitchMaxSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Upper.ToString();
                textBox_SwitchMinSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Lower.ToString();
                textBox_MinerAPIQueryInterval.Text = ConfigManager.GeneralConfig.MinerAPIQueryInterval.ToString();
                textBox_MinerRestartDelayMS.Text = ConfigManager.GeneralConfig.MinerRestartDelayMS.ToString();
                textBox_MinIdleSeconds.Text = ConfigManager.GeneralConfig.MinIdleSeconds.ToString();
                textBox_LogMaxFileSize.Text = ConfigManager.GeneralConfig.LogMaxFileSize.ToString();
                //textBox_ethminerDefaultBlockHeight.Text =
                //    ConfigManager.GeneralConfig.ethminerDefaultBlockHeight.ToString();
                textBox_APIBindPortStart.Text = ConfigManager.GeneralConfig.ApiBindPortPoolStart.ToString();
                textBox_MinProfit.Text =
                    ConfigManager.GeneralConfig.MinimumProfit.ToString("F2").Replace(',', '.'); // force comma;
                textBox_SwitchProfitabilityThreshold.Text = ConfigManager.GeneralConfig.SwitchProfitabilityThreshold
                    .ToString("F2").Replace(',', '.'); // force comma;
                textBox_ElectricityCost.Text = ConfigManager.GeneralConfig.KwhPrice.ToString("0.0000");
            }

            // set custom control referances
            {
                // here we want all devices
                devicesListViewEnableControl1.SetComputeDevices(AvailableDevices.Devices.ToList());
                devicesListViewEnableControl1.SetAlgorithmsListView(algorithmsListView1);
                devicesListViewEnableControl1.SaveToGeneralConfig = true;
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
            minDeviceProfitField.Leave += MinDeviceProfitFieldLeft;
        }

        #endregion //Tab Devices

        #endregion // Initializations

        #region Form Callbacks

        #region Tab General

        private void GeneralTextBoxes_Leave(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            
            ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Upper =
                Helpers.ParseInt(textBox_SwitchMaxSeconds.Text);
            ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Lower = Helpers.ParseInt(textBox_SwitchMinSeconds.Text);
            ConfigManager.GeneralConfig.MinerAPIQueryInterval = Helpers.ParseInt(textBox_MinerAPIQueryInterval.Text);
            ConfigManager.GeneralConfig.MinerRestartDelayMS = Helpers.ParseInt(textBox_MinerRestartDelayMS.Text);
            ConfigManager.GeneralConfig.MinIdleSeconds = Helpers.ParseInt(textBox_MinIdleSeconds.Text);
            ConfigManager.GeneralConfig.LogMaxFileSize = Helpers.ParseLong(textBox_LogMaxFileSize.Text);
            //ConfigManager.GeneralConfig.ethminerDefaultBlockHeight =
            //    Helpers.ParseInt(textBox_ethminerDefaultBlockHeight.Text);
            ConfigManager.GeneralConfig.ApiBindPortPoolStart = Helpers.ParseInt(textBox_APIBindPortStart.Text);
            // min profit
            ConfigManager.GeneralConfig.MinimumProfit = Helpers.ParseDouble(textBox_MinProfit.Text);
            ConfigManager.GeneralConfig.SwitchProfitabilityThreshold =
                Helpers.ParseDouble(textBox_SwitchProfitabilityThreshold.Text);

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
            //textBox_ethminerDefaultBlockHeight.Text = ConfigManager.GeneralConfig.ethminerDefaultBlockHeight.ToString();
            textBox_APIBindPortStart.Text = ConfigManager.GeneralConfig.ApiBindPortPoolStart.ToString();
            textBox_ElectricityCost.Text = ConfigManager.GeneralConfig.KwhPrice.ToString("0.0000");
        }

        private void GeneralComboBoxes_Leave(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            ConfigManager.GeneralConfig.TimeUnit = (TimeUnitType) comboBox_TimeUnit.SelectedIndex;
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
            minDeviceProfitField.Enabled = true;
            minDeviceProfitField.EntryText = _selectedComputeDevice.MinimumProfit.ToString("F2").Replace(',', '.');
        }

        private void MinDeviceProfitFieldLeft(object sender, EventArgs e)
        {
            if (_selectedComputeDevice != null && 
                double.TryParse(minDeviceProfitField.EntryText, out var min))
            {
                if (min < 0) min = 0;

                _selectedComputeDevice.MinimumProfit = min;
            }
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
                SetDefaults = true;
                SetLanguage("en");
                ConfigManager.GeneralConfig.SetDefaults();
                InitializeGeneralTabFieldValuesReferences();
                InitializeGeneralTabTranslations();
                Close();
            }
        }

        private void ButtonSaveClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion // Form Buttons

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ApplicationStateManager.BurnCalled) {
                return;
            }
            // check restart parameters change
            IsRestartNeeded = ConfigManager.IsRestartNeeded();
            ConfigManager.GeneralConfigFileCommit();
            ConfigManager.CommitBenchmarks();
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

            // update logic
            var is3rdPartyEnabled = ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
            checkBox_RunEthlargement.Enabled = Helpers.IsElevated && is3rdPartyEnabled;
            EthlargementIntegratedPlugin.Instance.ServiceEnabled = ConfigManager.GeneralConfig.UseEthlargement && Helpers.IsElevated && is3rdPartyEnabled;
            // re-init update plugins
            MinerPluginsManager.InitIntegratedPlugins();
        }

        //private void CheckBox_RunEthlargement_CheckedChanged(object sender, EventArgs e)
        //{
        //    ConfigManager.GeneralConfig.UseEthlargement = checkBox_RunEthlargement.Checked;
        //    // update logic
        //    var is3rdPartyEnabled = ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
        //    EthlargementIntegratedPlugin.Instance.ServiceEnabled = ConfigManager.GeneralConfig.UseEthlargement && Helpers.IsElevated && is3rdPartyEnabled;
        //}
    }
}

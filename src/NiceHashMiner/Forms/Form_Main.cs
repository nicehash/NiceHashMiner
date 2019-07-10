using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Forms;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Miners.IdleChecking;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static NiceHashMiner.Translations; // consider using static

namespace NiceHashMiner
{
    using NiceHashMiner.Forms.Components;
    using NiceHashMiner.Plugin;
    using NiceHashMiner.Utils;
    using NHM.Common;
    using NHM.Common.Enums;

    public partial class Form_Main : Form, FormHelpers.ICustomTranslate, IVersionDisplayer, IBalanceBTCDisplayer, IBalanceFiatDisplayer, IGlobalMiningRateDisplayer, IMiningProfitabilityDisplayer, INoInternetConnectionDisplayer
    {
        private bool _showWarningNiceHashData;

        private bool _exitCalled = false;

        private Form_MinerPlugins _minerPluginsForm;

        //private bool _isDeviceDetectionInitialized = false;

        // this is TEMP remove it as soon as possible
        private DevicesListViewSpeedControl devicesListViewEnableControl1;

        private bool _isManuallyStarted = false;

        public Form_Main()
        {
            InitializeComponent();
            CenterToScreen();
            Icon = Properties.Resources.logo;
            errorWarningProvider2.Icon = new IconEx(IconEx.SystemIcons.Warning, new Size(16, 16)).Icon; // SystemIcons.Warning;
            labelWarningNotProfitableOrNoIntenret.Visible = false;
            InitElevationWarning();
            this.TopMost = ConfigManager.GeneralConfig.GUIWindowsAlwaysOnTop;

            devicesListViewEnableControl1 = devicesMainBoard1.SpeedsControl;
            FormHelpers.SubscribeAllControls(this);

            // Hide plugins button and resize
            if (MinerPluginsManager.IntegratedPluginsOnly)
            {
                this.buttonHelp.Location = this.buttonPlugins.Location;
                this.buttonPlugins.Enabled = false;
                this.buttonPlugins.Visible = false;
            }

            Width = ConfigManager.GeneralConfig.MainFormSize.X;
            Height = ConfigManager.GeneralConfig.MainFormSize.Y;

            Text = ApplicationStateManager.Title;

            notifyIcon1.Icon = Properties.Resources.logo;
            notifyIcon1.Text = Application.ProductName + " v" + Application.ProductVersion +
                               "\n" + Tr("Double-click to restore...");

            linkLabelNewVersion.Text = "";

            InitMainConfigGuiData();
            devicesMainBoard1.SecondPanelVisible = false;
            labelDemoMode.DataBindings.AddSafeBinding("Visible", MiningState.Instance, nameof(MiningState.Instance.IsDemoMining), true, DataSourceUpdateMode.OnPropertyChanged);
            labelDemoMode.BringToFront();
            labelDemoMode.VisibleChanged += LabelDemoMode_VisibleChanged;
            InitControlValidators();
            FormHelpers.TranslateFormControls(this);
        }

        private void LabelDemoMode_VisibleChanged(object sender, EventArgs e)
        {
            if (labelDemoMode.Visible)
            {
                errorProvider1.SetError(textBoxBTCAddress, "");
                errorProvider1.SetError(textBoxWorkerName, "");
            }
            else
            {
                textBoxBTCAddress_Validate();
                textBoxWorkerName_Validate();
            }
            
        }

        private void InitDataBindings()
        {
            comboBoxLocation.DataBindings.Add("SelectedIndex", ConfigManager.GeneralConfig, nameof(ConfigManager.GeneralConfig.ServiceLocation), false, DataSourceUpdateMode.OnPropertyChanged);
            textBoxBTCAddress.DataBindings.AddSafeBinding("Text", ConfigManager.CredentialsSettings, nameof(ConfigManager.CredentialsSettings.BitcoinAddress), false, DataSourceUpdateMode.OnPropertyChanged);
            textBoxWorkerName.DataBindings.AddSafeBinding("Text", ConfigManager.CredentialsSettings, nameof(ConfigManager.CredentialsSettings.WorkerName), false, DataSourceUpdateMode.OnPropertyChanged);

            linkLabelCheckStats.DataBindings.AddSafeBinding("Enabled", ConfigManager.CredentialsSettings, nameof(ConfigManager.CredentialsSettings.IsCredentialsValid), false, DataSourceUpdateMode.OnPropertyChanged);

            // mining /benchmarking
            buttonPlugins.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.IsNotBenchmarkingOrMining), false, DataSourceUpdateMode.OnPropertyChanged);
            textBoxBTCAddress.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.IsNotBenchmarkingOrMining), false, DataSourceUpdateMode.OnPropertyChanged);
            textBoxWorkerName.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.IsNotBenchmarkingOrMining), false, DataSourceUpdateMode.OnPropertyChanged);
            comboBoxLocation.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.IsNotBenchmarkingOrMining), false, DataSourceUpdateMode.OnPropertyChanged);
            buttonBenchmark.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.IsNotBenchmarkingOrMining), false, DataSourceUpdateMode.OnPropertyChanged);
            buttonSettings.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.IsNotBenchmarkingOrMining), false, DataSourceUpdateMode.OnPropertyChanged);
            linkLabelAdminPrivs.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.IsNotBenchmarkingOrMining), false, DataSourceUpdateMode.OnPropertyChanged);
            // start stop all
            buttonStartMining.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.AnyDeviceStopped), false, DataSourceUpdateMode.OnPropertyChanged);
            buttonStopMining.DataBindings.AddSafeBinding("Enabled", MiningState.Instance, nameof(MiningState.Instance.AnyDeviceRunning), false, DataSourceUpdateMode.OnPropertyChanged);

            ////labelDemoMode.DataBindings.Add("Enabled", MiningState.Instance, nameof(MiningState.Instance.IsDemoMining), false, DataSourceUpdateMode.OnPropertyChanged);
            //labelDemoMode.DataBindings.Add("Visible", MiningState.Instance, nameof(MiningState.Instance.IsDemoMining), true, DataSourceUpdateMode.OnPropertyChanged);
            
            devicesMainBoard1.DataBindings.AddSafeBinding(nameof(devicesMainBoard1.SecondPanelVisible), MiningState.Instance, nameof(MiningState.Instance.AnyDeviceRunning), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void textBoxBTCAddress_Validate()
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            if (ApplicationStateManager.SetResult.INVALID == result)
            {
                errorProvider1.SetError(textBoxBTCAddress, Tr("Invalid Bitcoin address! {0} will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!", NHMProductInfo.Name));
            }
            else
            {
                errorProvider1.SetError(textBoxBTCAddress, "");
            }
        }

        private void textBoxWorkerName_Validate()
        {
            var trimmedWorkerNameText = textBoxWorkerName.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkerNameText);
            if (ApplicationStateManager.SetResult.INVALID == result)
            {
                errorProvider1.SetError(textBoxWorkerName, Tr("Invalid workername!\n\nPlease enter a valid workername (Aa-Zz, 0-9, up to 15 character long)."));
            }
            else
            {
                errorProvider1.SetError(textBoxWorkerName, "");
            }
        }

        private void InitControlValidators()
        {
            textBoxBTCAddress.TextChanged += (s, e) => textBoxBTCAddress_Validate();
            //textBoxBTCAddress.Validating += textBoxBTCAddress_TextChanged;
            //textBoxBTCAddress.Validated += textBoxBTCAddress_TextChanged;

            textBoxWorkerName.TextChanged += (s, e) => textBoxWorkerName_Validate();
            //textBoxWorkerName.Validating += textBoxWorkerName_ValidateCorrect;
            //textBoxWorkerName.Validated += textBoxWorkerName_ValidateCorrect;
        }

        void FormHelpers.ICustomTranslate.CustomTranslate()
        {
            MessageBoxManager.Unregister();
            MessageBoxManager.Yes = Tr("&Yes");
            MessageBoxManager.No = Tr("&No");
            MessageBoxManager.OK = Tr("&OK");
            MessageBoxManager.Cancel = Tr("&Cancel");
            MessageBoxManager.Retry = Tr("&Retry");
            MessageBoxManager.Register();

            //??? doesn't get translated if we don't translate it directly????
            toolStripStatusLabelGlobalRateText.Text = Tr("Global rate:");
            toolStripStatusLabelBTCDayText.Text =
                "BTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
            toolStripStatusLabelBalanceText.Text = RatesAndStatsStates.Instance.LabelBalanceText;

            devicesListViewEnableControl1.InitLocale();

            // this one here is probably redundant
            labelDemoMode.Text = Tr("{0} is running in DEMO mode!", NHMProductInfo.Name);
            toolTip1.SetToolTip(labelDemoMode, Tr("You have not entered a bitcoin address. {0} will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!", NHMProductInfo.Name));

            SetToolTip(Tr("User's bitcoin address for mining."),
                textBoxBTCAddress, labelBitcoinAddress);

            SetToolTip(Tr("To identify the user's computer."),
                textBoxWorkerName, labelWorkerName);

            SetToolTip(Tr("Sets the mining location. Choosing Hong Kong or Tokyo will add extra latency."),
                comboBoxLocation, labelServiceLocation);
        }

        private void SetToolTip(string text, params Control[] controls)
        {
            foreach (var control in controls)
            {
                toolTip1.SetToolTip(control, text);
            }
        }

        private void InitElevationWarning()
        {
            var isEnabledFeature = false;
            // Enable this only for new platform
#if TESTNET || TESTNETDEV || PRODUCTION_NEW
            isEnabledFeature = true;
#endif
            if (!Helpers.IsElevated && isEnabledFeature && !ConfigManager.GeneralConfig.DisableDevicePowerModeSettings)
            {
                errorWarningProvider2.SetError(linkLabelAdminPrivs, Tr("Disabled NVIDIA power mode settings due to insufficient permissions. If you want to use this feature you need to run as Administrator."));
                linkLabelAdminPrivs.Click += (s, e) =>
                {
                    var dialogResult = MessageBox.Show(Tr("Click yes if you with to run {0} as Administrator.", NHMProductInfo.Name),
                    Tr("Run as Administrator"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                        RunAsAdmin.SelfElevate();                    
                };
            }
            else
            {
                linkLabelAdminPrivs.Visible = false;
            }
        }

        // InitMainConfigGuiData gets called after settings are changed and whatnot but this is a crude and tightly coupled way of doing things
        private void InitMainConfigGuiData()
        {
            textBoxBTCAddress.Text = ConfigManager.CredentialsSettings.BitcoinAddress;
            textBoxWorkerName.Text = ConfigManager.CredentialsSettings.WorkerName;
            _showWarningNiceHashData = true;

            // init active display currency after config load
            ExchangeRateApi.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

            // init factor for Time Unit
            TimeFactor.UpdateTimeUnit(ConfigManager.GeneralConfig.TimeUnit);

            toolStripStatusLabelBalanceDollarValue.Text = "(" + ExchangeRateApi.ActiveDisplayCurrency + ")";
            toolStripStatusLabelBalanceText.Text = RatesAndStatsStates.Instance.LabelBalanceText;


            devicesListViewEnableControl1.SetPayingColumns();
        }
        
        // TODO this has nothing to do with Mian_Form
        private void IdleCheck(object sender, IdleChangedEventArgs e)
        {
            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle || _isManuallyStarted) return;

            // TODO set is mining here
            if (MiningState.Instance.IsCurrentlyMining)
            {
                if (!e.IsIdle)
                {
                    ApplicationStateManager.StopAllDevice();
                    Logger.Info("NICEHASH", "Resumed from idling");
                }
            }
            else if (ApplicationStateManager.CurrentForm == ApplicationStateManager.CurrentFormState.Main && e.IsIdle)
            {
                Logger.Info("NICEHASH", "Entering idling state");
                if (StartMining(false) != StartMiningReturnType.StartMining)
                {
                    ApplicationStateManager.StopAllDevice();
                }
            }
        }

        private void SetChildFormCenter(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(Location.X + (Width - form.Width) / 2, Location.Y + (Height - form.Height) / 2);
        }

        private async void Form_Main_Shown(object sender, EventArgs e)
        {
            //// TODO temporary hooks
            ApplicationStateManager._ratesComunication = devicesListViewEnableControl1;
            // handle these callbacks differently
            NiceHashStats.OnConnectionLost += ConnectionLostCallback;
            NiceHashStats.OnExchangeUpdate += UpdateExchange;

            foreach (Control c in Controls)
            {
                c.Enabled = false;
            }

            using (var loadingControl = new StartupLoadingControl(Tr("Loading, please wait...")))
            {
                Controls.Add(loadingControl);
                var location = new Point((Width - loadingControl.Width) / 2, (int)((Height - loadingControl.Height) * 0.3));
                loadingControl.Location = location;
                loadingControl.BringToFront();

                var progress = new Progress<(string loadMessageText, int perc)>(p =>
                {
                    loadingControl.Progress = p.perc;
                    loadingControl.LoadMessageText = p.loadMessageText;
                });

                var progressDownload = new Progress<(string loadMessageText, int perc)>(p =>
                {
                    loadingControl.ProgressSecond = p.perc;
                    loadingControl.LoadMessageTextSecond = p.loadMessageText;
                });
                await ApplicationStateManager.InitializeManagersAndMiners(loadingControl, progress, progressDownload);
            }
            devicesListViewEnableControl1.SetComputeDevices(AvailableDevices.Devices.ToList());

            foreach (Control c in Controls)
            {
                c.Enabled = true;
            }

            // Data bindings
            InitDataBindings();
            textBoxBTCAddress_Validate();
            textBoxWorkerName_Validate();

            if (ConfigManager.GeneralConfig.AutoStartMining)
            {
                // well this is started manually as we want it to start at runtime
                _isManuallyStarted = true;
                if (StartMining(false) != StartMiningReturnType.StartMining)
                {
                    _isManuallyStarted = false;
                    ApplicationStateManager.StopAllDevice();
                }
            }
        }

        private void UpdateGlobalRate(double totalRate)
        {
            var factorTimeUnit = TimeFactor.TimeUnit;
            var scaleBTC = ConfigManager.GeneralConfig.AutoScaleBTCValues && totalRate < 0.1;
            var totalDisplayRate = totalRate * factorTimeUnit * (scaleBTC ? 1000 : 1);
            var displayTimeUnit = Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());

            toolStripStatusLabelBTCDayText.Text = scaleBTC ? $"mBTC/{displayTimeUnit}" : $"BTC/{displayTimeUnit}";
            toolStripStatusLabelGlobalRateValue.Text = totalDisplayRate.ToString(scaleBTC ? "F5" : "F6", CultureInfo.InvariantCulture);


            toolStripStatusLabelBTCDayValue.Text = ExchangeRateApi
                .ConvertToActiveCurrency((totalRate * factorTimeUnit * ExchangeRateApi.GetUsdExchangeRate()))
                .ToString("F2", CultureInfo.InvariantCulture);
            toolStripStatusLabelBalanceText.Text = RatesAndStatsStates.Instance.LabelBalanceText;
        }

        private void UpdateExchange(object sender, EventArgs e)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                var br = ExchangeRateApi.GetUsdExchangeRate();
                var currencyRate = Tr("N/A");
                if (br > 0)
                {
                    currencyRate = ExchangeRateApi.ConvertToActiveCurrency(br).ToString("F2");
                }

                toolTip1.SetToolTip(statusStrip1, $"1 BTC = {currencyRate} {ExchangeRateApi.ActiveDisplayCurrency}");

                Logger.Info("NICEHASH", $"Current Bitcoin rate: {br.ToString("F2", CultureInfo.InvariantCulture)}");
            });
        }

        private void ConnectionLostCallback(object sender, EventArgs e)
        {
            if (!NHSmaData.HasData && ConfigManager.GeneralConfig.ShowInternetConnectionWarning &&
                _showWarningNiceHashData)
            {
                _showWarningNiceHashData = false;
                var dialogResult = MessageBox.Show(Tr("{0} requires internet connection to run. Please ensure that you are connected to the internet before running {0}. Would you like to continue?", NHMProductInfo.Name),
                    Tr("Check internet connection"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (dialogResult == DialogResult.Yes)
                    return;
                if (dialogResult == DialogResult.No)
                    Application.Exit();
            }
        }

        private void LinkLabelCheckStats_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ConfigManager.CredentialsSettings.IsCredentialsValid == false) return;
            ApplicationStateManager.VisitMiningStatsPage();
        }

        private void LinkLabelNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ApplicationStateManager.VisitNewVersionUrl();
        }

        private void ButtonBenchmark_Click(object sender, EventArgs e)
        {
            bool startMining = false;
            using (var benchmarkForm = new Form_Benchmark())
            {
                SetChildFormCenter(benchmarkForm);
                ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Benchmark;
                benchmarkForm.ShowDialog();
                ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Main;
                startMining = benchmarkForm.StartMiningOnFinish;
            }

            InitMainConfigGuiData();
            if (startMining)
            {
                ButtonStartMining_Click(null, null);
            }
        }


        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            using (var settings = new Form_Settings())
            {
                SetChildFormCenter(settings);
                ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Settings;
                settings.ShowDialog();
                ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Main;

                if (settings.IsRestartNeeded)
                {
                    if (!settings.SetDefaults)
                    {
                        MessageBox.Show(
                        Tr("Settings change requires {0} to restart.", NHMProductInfo.Name),
                        Tr("Restart Notice"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    ApplicationStateManager.RestartProgram();
                    return;
                }
            }
            FormHelpers.TranslateFormControls(this);
            InitMainConfigGuiData();
            // TODO check this later
            IdleCheckManager.StartIdleCheck(ConfigManager.GeneralConfig.IdleCheckType, IdleCheck);
        }

        private void ButtonStartMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = true;
            if (StartMining(true) == StartMiningReturnType.ShowNoMining)
            {
                _isManuallyStarted = false;
                ApplicationStateManager.StopAllDevice();
                MessageBox.Show(Tr("{0} cannot start mining. Make sure you have at least one enabled device that has at least one enabled and benchmarked algorithm.", NHMProductInfo.Name),
                    Tr("Warning!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ButtonStopMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = false;
            ApplicationStateManager.StopAllDevice();
        }

        private void ButtonLogo_Click(object sender, EventArgs e)
        {
            Process.Start(Links.VisitUrl);
        }

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            Process.Start(Links.NhmHelp);
        }

        private void ToolStripStatusLabel10_Click(object sender, EventArgs e)
        {
            Process.Start(Links.NhmPayingFaq);
        }

        private void ToolStripStatusLabel10_MouseHover(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Hand;
        }

        private void ToolStripStatusLabel10_MouseLeave(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Default;
        }

        ///////////////////////////////////////
        // Miner control functions
        private enum StartMiningReturnType
        {
            StartMining,
            ShowNoMining,
            IgnoreMsg
        }

        // TODO this thing needs to be completely removed
        // TODO this will be moved outside of GUI code, replace textBoxBTCAddress.Text with ConfigManager.GeneralConfig.BitcoinAddress
        private StartMiningReturnType StartMining(bool showWarnings)
        {
            //if (ConfigManager.GeneralConfig.BitcoinAddress.Equals(""))
            //{
            //    if (showWarnings)
            //    {
            //        var result = MessageBox.Show(Tr("You have not entered a bitcoin address. {0} will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!"),
            //            Tr("Start mining in DEMO mode?"),
            //            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            //        if (result == DialogResult.Yes)
            //        {
            //            _demoMode = true;
            //            //labelDemoMode.Visible = true;
            //        }
            //        else
            //        {
            //            return StartMiningReturnType.IgnoreMsg;
            //        }
            //    }
            //    else
            //    {
            //        return StartMiningReturnType.IgnoreMsg;
            //    }
            //}
            ////else if (!VerifyMiningAddress(true)) return StartMiningReturnType.IgnoreMsg; // TODO this whole shitty thing

            var hasData = NHSmaData.HasData;

            if (!showWarnings)
            {
                for (var i = 0; i < 10; i++)
                {
                    if (hasData) break;
                    Thread.Sleep(1000);
                    hasData = NHSmaData.HasData;
                    Logger.Info("NICEHASH", $"After {i}s has data: {hasData}");
                }
            }

            if (!hasData)
            {
                Logger.Debug("NICEHASH", "No data received within timeout");
                if (showWarnings)
                {
                    MessageBox.Show(Tr("Unable to get NiceHash profitability data. If you are connected to internet, try again later."),
                        Tr("Error!"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return StartMiningReturnType.IgnoreMsg;
            }

            var isMining = true;
            ApplicationStateManager.StartAllAvailableDevices();

            return isMining ? StartMiningReturnType.StartMining : StartMiningReturnType.ShowNoMining;
        }

        private void Form_Main_ResizeEnd(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.MainFormSize.X = Width;
            ConfigManager.GeneralConfig.MainFormSize.Y = Height;
        }

        // StateDisplay interfaces

        void IVersionDisplayer.DisplayVersion(object sender, string version)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                linkLabelNewVersion.Text = version;
                errorWarningProvider2.SetError(linkLabelNewVersion, version);
            });
        }

        // TODO this might need some formatters?
        void IBalanceBTCDisplayer.DisplayBTCBalance(object sender, double btcBalance)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                if (ConfigManager.GeneralConfig.AutoScaleBTCValues && btcBalance < 0.1)
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "mBTC";
                    toolStripStatusLabelBalanceBTCValue.Text =
                        (btcBalance * 1000).ToString("F5", CultureInfo.InvariantCulture);
                }
                else
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "BTC";
                    toolStripStatusLabelBalanceBTCValue.Text = btcBalance.ToString("F6", CultureInfo.InvariantCulture);
                }
            });
        }

        void IBalanceFiatDisplayer.DisplayFiatBalance(object sender, (double fiatBalance, string fiatCurrencySymbol) args)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                toolStripStatusLabelBalanceDollarText.Text = args.fiatBalance.ToString("F2", CultureInfo.InvariantCulture);
                toolStripStatusLabelBalanceDollarValue.Text = $"({args.fiatCurrencySymbol})";
            });
        }

        void IGlobalMiningRateDisplayer.DisplayGlobalMiningRate(object sender, double totalMiningRate)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                UpdateGlobalRate(totalMiningRate);
            });
        }

        //void IStartMiningDisplayer.DisplayMiningStarted(object sender, EventArgs _)
        //{
        //    FormHelpers.SafeInvoke(this, () =>
        //    {
        //        //// Disable profitable notification on start
        //        //_isNotProfitable = false;
        //        HideWarning();
        //    });
        //}

        //void IStopMiningDisplayer.DisplayMiningStopped(object sender, EventArgs _)
        //{
        //    FormHelpers.SafeInvoke(this, () =>
        //    {
        //        //// Disable IFTTT notification before label call
        //        //_isNotProfitable = false;
        //        labelDemoMode.Visible = false;
        //        _demoMode = false; // TODO this is logic

        //        UpdateGlobalRate(0);
        //    });
        //}

        private void ButtonPlugins_Click(object sender, EventArgs e)
        {
            if (_minerPluginsForm == null) _minerPluginsForm = new Form_MinerPlugins();
            SetChildFormCenter(_minerPluginsForm);
            _minerPluginsForm.ShowDialog();
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Right == e.Button)
            {
                contextMenuStrip1.Show();
            } 
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _exitCalled = true;
            Application.Exit();
        }

        private void ToolStripMenuItemShow_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        // Minimize to system tray if MinimizeToTray is set to true
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (ConfigManager.GeneralConfig.MinimizeToTray && FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                Hide();
            }
        }

        // Restore NiceHashMiner from the system tray
        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ConfigManager.GeneralConfig.MinimizeToTray && !_exitCalled)
            {
                notifyIcon1.Visible = true;
                Hide();
                e.Cancel = true;
                return;
            }

            FormHelpers.UnsubscribeAllControls(this);
            ApplicationStateManager.BeforeExit();
            MessageBoxManager.Unregister();
        }

        private bool _isProfitable = true;
        private bool _noInternet = false;

        void IMiningProfitabilityDisplayer.DisplayMiningProfitable(object sender, bool isProfitable)
        {
            if (ConfigManager.GeneralConfig.UseIFTTT)
            {
                if (isProfitable)
                {
                    Ifttt.PostToIfttt("nicehash", "Mining is once again profitable and has resumed.");
                }
                else
                {
                    Ifttt.PostToIfttt("nicehash", "Mining NOT profitable and has stopped.");
                }
            }
            _isProfitable = isProfitable;
            ShowOrHideWarningLabel(_isProfitable, _noInternet);
        }

        void INoInternetConnectionDisplayer.DisplayNoInternetConnection(object sender, bool noInternet)
        {
            _noInternet = noInternet;
            ShowOrHideWarningLabel(_isProfitable, _noInternet);
        }

        private void ShowOrHideWarningLabel(bool isProfitable, bool noInternet)
        {
            FormHelpers.SafeInvoke(this, () => {
                if (!isProfitable || noInternet)
                {
                    var text = "";
                    if (!isProfitable)
                    {
                        text += Environment.NewLine + Tr("CURRENTLY MINING NOT PROFITABLE.");
                    }
                    if (noInternet)
                    {
                        text += Environment.NewLine + Tr("CURRENTLY NOT MINING. NO INTERNET CONNECTION.");
                    }
                    labelWarningNotProfitableOrNoIntenret.Text = text;
                    labelWarningNotProfitableOrNoIntenret.Visible = true;
                }
                else
                {
                    labelWarningNotProfitableOrNoIntenret.Visible = false;
                }
            });
        }
    }
}

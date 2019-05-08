using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Forms;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.IdleChecking;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using NiceHashMiner.Utils;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using static NiceHashMiner.Translations; // consider using static

namespace NiceHashMiner
{
    using NiceHashMiner.Forms.Components;
    using NiceHashMiner.Plugin;
    using NiceHashMinerLegacy.Common;
    using System.IO;
    using System.Threading.Tasks;

    public partial class Form_Main : Form, IBTCDisplayer, IWorkerNameDisplayer, IServiceLocationDisplayer, IVersionDisplayer, IBalanceBTCDisplayer, IBalanceFiatDisplayer, IGlobalMiningRateDisplayer, IStartMiningDisplayer, IStopMiningDisplayer
    {
        private bool _showWarningNiceHashData;
        private bool _demoMode;

        private bool _exitCalled = false;

        private Form_Benchmark _benchmarkForm;
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

            textBoxBTCAddress.TextChanged += textBoxBTCAddress_TextChanged;
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

            InitLocalization();
            Text += ApplicationStateManager.Title;

            notifyIcon1.Icon = Properties.Resources.logo;
            notifyIcon1.Text = Application.ProductName + " v" + Application.ProductVersion +
                               "\nDouble-click to restore..";

            InitMainConfigGuiData();
            FormHelpers.TranslateFormControls(this);
        }

        private void InitLocalization()
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
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";

            devicesListViewEnableControl1.InitLocale();
        }

        // InitMainConfigGuiData gets called after settings are changed and whatnot but this is a crude and tightly coupled way of doing things
        private void InitMainConfigGuiData()
        {
            _showWarningNiceHashData = true;
            _demoMode = false;

            // init active display currency after config load
            ExchangeRateApi.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

            // init factor for Time Unit
            TimeFactor.UpdateTimeUnit(ConfigManager.GeneralConfig.TimeUnit);

            toolStripStatusLabelBalanceDollarValue.Text = "(" + ExchangeRateApi.ActiveDisplayCurrency + ")";
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";


            devicesListViewEnableControl1.SetPayingColumns();
        }
        
        // TODO this has nothing to do with Mian_Form
        private void IdleCheck(object sender, IdleChangedEventArgs e)
        {
            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle || _isManuallyStarted) return;

            // TODO set is mining here
            if (ApplicationStateManager.IsCurrentlyMining)
            {
                if (!e.IsIdle)
                {
                    ApplicationStateManager.StopAllDevice();
                    Logger.Info("NICEHASH", "Resumed from idling");
                }
            }
            else if (_benchmarkForm == null && e.IsIdle)
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
            else
            {
                buttonStopMining.Enabled = false;
            }
        }

        //public void ShowNotProfitable(string msg)
        //{
        //    if (ConfigManager.GeneralConfig.UseIFTTT)
        //    {
        //        if (!_isNotProfitable)
        //        {
        //            Ifttt.PostToIfttt("nicehash", msg);
        //            _isNotProfitable = true;
        //        }
        //    }

        //    if (InvokeRequired)
        //    {
        //        Invoke((Action) delegate
        //        {
        //            ShowNotProfitable(msg);
        //        });
        //    }
        //    else
        //    {
        //        //label_NotProfitable.Visible = true;
        //        //label_NotProfitable.Text = msg;
        //        //label_NotProfitable.Invalidate();
        //    }
        //}

        private void ShowWarning(string msg)
        {
            // Doesn't exist enymore but this was used for showing mining not profitable and internet connection drop
            //label_NotProfitable.Visible = true;
            //label_NotProfitable.Text = msg;
            //label_NotProfitable.Invalidate();
        }

        private void HideWarning()
        {
            // Doesn't exist enymore but this was used for showing mining not profitable and internet connection drop
            //label_NotProfitable.Visible = false;
            //label_NotProfitable.Invalidate();
        }

        //public void HideNotProfitable()
        //{
        //    if (ConfigManager.GeneralConfig.UseIFTTT)
        //    {
        //        if (_isNotProfitable)
        //        {
        //            Ifttt.PostToIfttt("nicehash", "Mining is once again profitable and has resumed.");
        //            _isNotProfitable = false;
        //        }
        //    }

        //    if (InvokeRequired)
        //    {
        //        Invoke((Action) HideNotProfitable);
        //    }
        //    else
        //    {
        //        //label_NotProfitable.Visible = false;
        //        //label_NotProfitable.Invalidate();
        //    }
        //}

        public void ForceMinerStatsUpdate()
        {
            try
            {
                BeginInvoke((Action) (async () =>
                {
                    await MinersManager.MinerStatsCheck();
                }));
            }
            catch (Exception e)
            {
                Logger.Error("NiceHash", e.Message);
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
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";
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
                var dialogResult = MessageBox.Show(Tr("NiceHash Miner Legacy requires internet connection to run. Please ensure that you are connected to the internet before running NiceHash Miner Legacy. Would you like to continue?"),
                    Tr("Check internet connection"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (dialogResult == DialogResult.Yes)
                    return;
                if (dialogResult == DialogResult.No)
                    Application.Exit();
            }
        }

        private bool VerifyMiningAddress(bool showError)
        {
            if (!BitcoinAddress.ValidateBitcoinAddress(textBoxBTCAddress.Text.Trim()) && showError)
            {
                var result = MessageBox.Show(Tr("Invalid Bitcoin address!\n\nPlease enter a valid Bitcoin address or choose Yes to create one."),
                    Tr("Error!"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                    Process.Start(Links.NhmBtcWalletFaq);

                textBoxBTCAddress.Focus();
                return false;
            }
            if (!BitcoinAddress.ValidateWorkerName(textBoxWorkerName.Text.Trim()) && showError)
            {
                var result = MessageBox.Show(Tr("Invalid workername!\n\nPlease enter a valid workername (Aa-Zz, 0-9, up to 15 character long)."),
                    Tr("Error!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                textBoxWorkerName.Focus();
                return false;
            }

            return true;
        }

        private void LinkLabelCheckStats_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!VerifyMiningAddress(true)) return;

            //in testnet there is no option to see stats without logging in
#if TESTNET || TESTNETDEV
            Process.Start(Links.CheckStats);
#else
            Process.Start(Links.CheckStats + textBoxBTCAddress.Text.Trim());
#endif
        }


        private void LinkLabelChooseBTCWallet_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Links.NhmBtcWalletFaq);
        }

        private void LinkLabelNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ApplicationStateManager.VisitNewVersionUrl();
        }

        private void ButtonBenchmark_Click(object sender, EventArgs e)
        {
            _benchmarkForm = new Form_Benchmark();
            SetChildFormCenter(_benchmarkForm);
            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Benchmark;
            _benchmarkForm.ShowDialog();
            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Main;
            ApplicationStateManager.ToggleActiveInactiveDisplay();
            var startMining = _benchmarkForm.StartMiningOnFinish;
            _benchmarkForm = null;

            InitMainConfigGuiData();
            if (startMining)
            {
                ButtonStartMining_Click(null, null);
            }
        }


        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            var settings = new Form_Settings();
            SetChildFormCenter(settings);
            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Settings;
            settings.ShowDialog();
            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Main;

            if (settings.IsChange && settings.IsChangeSaved && settings.IsRestartNeeded)
            {
                MessageBox.Show(
                    Tr("Settings change requires NiceHash Miner Legacy to restart."),
                    Tr("Restart Notice"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ApplicationStateManager.RestartProgram();
            }
            else if (settings.IsChange && settings.IsChangeSaved)
            {
                InitLocalization();
                FormHelpers.TranslateFormControls(this);
                InitMainConfigGuiData();
                // TODO check this later
                IdleCheckManager.StartIdleCheck(ConfigManager.GeneralConfig.IdleCheckType, IdleCheck);
            }
        }

        private void ButtonStartMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = true;
            if (StartMining(true) == StartMiningReturnType.ShowNoMining)
            {
                _isManuallyStarted = false;
                ApplicationStateManager.StopAllDevice();
                MessageBox.Show(Tr("NiceHash Miner Legacy cannot start mining. Make sure you have at least one enabled device that has at least one enabled and benchmarked algorithm."),
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

        private void textBoxBTCAddress_Leave(object sender, EventArgs e)
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            // TODO GUI stuff get back to this
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    //var dialogResult = MessageBox.Show(Tr("Invalid Bitcoin address!\n\nPlease enter a valid Bitcoin address or choose Yes to create one."),
                    //Tr("Error!"),
                    //MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    //if (dialogResult == DialogResult.Yes)
                    //    Process.Start(Links.NhmBtcWalletFaq);

                    //textBoxBTCAddress.Focus();
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        private void textBoxWorkerName_Leave(object sender, EventArgs e)
        {
            var trimmedWorkerNameText = textBoxWorkerName.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkerNameText);
            // TODO GUI stuff get back to this
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    // TODO workername invalid handling
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        private void comboBoxLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var locationIndex = comboBoxLocation.SelectedIndex;
            var result = ApplicationStateManager.SetServiceLocationIfValidOrDifferent(locationIndex);
            // TODO GUI stuff get back to this, here we can't really break anything
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        ///////////////////////////////////////
        // Miner control functions
        private enum StartMiningReturnType
        {
            StartMining,
            ShowNoMining,
            IgnoreMsg
        }

        // TODO this will be moved outside of GUI code, replace textBoxBTCAddress.Text with ConfigManager.GeneralConfig.BitcoinAddress
        private StartMiningReturnType StartMining(bool showWarnings)
        {
            if (ConfigManager.GeneralConfig.BitcoinAddress.Equals(""))
            {
                if (showWarnings)
                {
                    var result = MessageBox.Show(Tr("You have not entered a bitcoin address. NiceHash Miner Legacy will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!"),
                        Tr("Start mining in DEMO mode?"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        _demoMode = true;
                        labelDemoMode.Visible = true;
                        labelDemoMode.Text = Tr("NiceHash Miner Legacy is running in DEMO mode!");
                    }
                    else
                    {
                        return StartMiningReturnType.IgnoreMsg;
                    }
                }
                else
                {
                    return StartMiningReturnType.IgnoreMsg;
                }
            }
            else if (!VerifyMiningAddress(true)) return StartMiningReturnType.IgnoreMsg;

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


            // TODO we will not prompt any warning if there are no benchmarks available, we will just start mining and benchmarking whatever needs benchmarking 
            //// Check if there are unbenchmakred algorithms
            //var isMining = MinersManager.StartInitialize(username);
            var isMining = true;
            ApplicationStateManager.IsDemoMining = _demoMode;
            ApplicationStateManager.StartAllAvailableDevices();

            return isMining ? StartMiningReturnType.StartMining : StartMiningReturnType.ShowNoMining;
        }

        private void Form_Main_ResizeEnd(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.MainFormSize.X = Width;
            ConfigManager.GeneralConfig.MainFormSize.Y = Height;
        }

        // StateDisplay interfaces
        void IBTCDisplayer.DisplayBTC(object sender, string btc)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                textBoxBTCAddress.Text = btc;
            });
        }

        void IWorkerNameDisplayer.DisplayWorkerName(object sender, string workerName)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                textBoxWorkerName.Text = workerName;
            });
        }

        void IServiceLocationDisplayer.DisplayServiceLocation(object sender, int serviceLocation)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                comboBoxLocation.SelectedIndex = serviceLocation;
            });
        }

        void IVersionDisplayer.DisplayVersion(object sender, string version)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                linkLabelNewVersion.Text = version;
            });
        }

        // TODO this might need some formatters?
        void IBalanceBTCDisplayer.DisplayBTCBalance(object sender, double btcBalance)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                if (ConfigManager.GeneralConfig.AutoScaleBTCValues && btcBalance< 0.1)
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

        void IStartMiningDisplayer.DisplayMiningStarted(object sender, EventArgs _)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                textBoxBTCAddress.Enabled = false;
                textBoxWorkerName.Enabled = false;
                comboBoxLocation.Enabled = false;
                buttonBenchmark.Enabled = false;
                buttonStartMining.Enabled = false;
                buttonSettings.Enabled = false;
                buttonStopMining.Enabled = true;
                //// Disable profitable notification on start
                //_isNotProfitable = false;
                HideWarning();
                // TODO put this in App State Manager
                if (ApplicationStateManager.AnyInMiningState())
                {
                    devicesMainBoard1.ShowPanel2();
                }
            });
        }

        void IStopMiningDisplayer.DisplayMiningStopped(object sender, EventArgs _)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                //// Disable IFTTT notification before label call
                //_isNotProfitable = false;

                textBoxBTCAddress.Enabled = true;
                textBoxWorkerName.Enabled = true;
                comboBoxLocation.Enabled = true;
                buttonBenchmark.Enabled = true;
                buttonStartMining.Enabled = true;
                buttonSettings.Enabled = true;
                buttonStopMining.Enabled = false;
                labelDemoMode.Visible = false;
                _demoMode = false; // TODO this is logic

                UpdateGlobalRate(0);
                devicesMainBoard1.HidePanel2();
            });
        }

        private void textBoxBTCAddress_TextChanged(object sender, EventArgs e)
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            // TODO GUI stuff get back to this
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    //var dialogResult = MessageBox.Show(Tr("Invalid Bitcoin address!\n\nPlease enter a valid Bitcoin address or choose Yes to create one."),
                    //Tr("Error!"),
                    //MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    //if (dialogResult == DialogResult.Yes)
                    //    Process.Start(Links.NhmBtcWalletFaq);

                    //textBoxBTCAddress.Focus();
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

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
    }
}

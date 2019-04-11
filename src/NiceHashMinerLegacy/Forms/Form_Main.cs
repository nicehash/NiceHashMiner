// TESTNET
#if TESTNET || TESTNETDEV
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
    using System.IO;

    public partial class Form_Main : Form, Form_Loading.IAfterInitializationCaller, IBTCDisplayer, IWorkerNameDisplayer, IServiceLocationDisplayer, IVersionDisplayer, IBalanceBTCDisplayer, IBalanceFiatDisplayer, IGlobalMiningRateDisplayer, IStartMiningDisplayer, IStopMiningDisplayer
    {
        private Timer _startupTimer;

        private bool _showWarningNiceHashData;
        private bool _demoMode;

        private Form_Loading _loadingScreen;
        private Form_Benchmark _benchmarkForm;

        //private bool _isDeviceDetectionInitialized = false;

        // this is TEMP remove it as soon as possible
        private DevicesListViewSpeedControl devicesListViewEnableControl1;

        private bool _isManuallyStarted = false;

        public Form_Main()
        {
            InitializeComponent();
            textBoxBTCAddress.TextChanged += textBoxBTCAddress_TextChanged;
            devicesListViewEnableControl1 = devicesMainBoard1.SpeedsControl;
            FormHelpers.SubscribeAllControls(this);

            Width = ConfigManager.GeneralConfig.MainFormSize.X;
            Height = ConfigManager.GeneralConfig.MainFormSize.Y;
            Icon = Properties.Resources.logo;

            InitLocalization();

            ComputeDeviceManager.SystemSpecs.QueryAndLog();

            // Log the computer's amount of Total RAM and Page File Size
            var moc = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem").Get();
            foreach (ManagementObject mo in moc)
            {
                var totalRam = long.Parse(mo["TotalVisibleMemorySize"].ToString()) / 1024;
                var pageFileSize = (long.Parse(mo["TotalVirtualMemorySize"].ToString()) / 1024) - totalRam;
                Helpers.ConsolePrint("NICEHASH", "Total RAM: " + totalRam + "MB");
                Helpers.ConsolePrint("NICEHASH", "Page File Size: " + pageFileSize + "MB");
            }

            Text += ApplicationStateManager.Title;

            InitMainConfigGuiData();
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

            labelServiceLocation.Text = Tr("Service location") + ":";
            {
                // TODO keep in mind the localizations
                var i = 0;
                foreach (var loc in StratumService.MiningLocations)
                {
                    comboBoxLocation.Items[i] = Tr((string)StratumService.MiningLocationNames[i]);
                    i++;
                }
            }
            labelBitcoinAddress.Text = Tr("Bitcoin Address") + ":";
            labelWorkerName.Text = Tr("Worker Name") + ":";

            linkLabelCheckStats.Text = Tr("Check my stats online!");
            linkLabelChooseBTCWallet.Text = Tr("Help me choose my Bitcoin wallet");

            toolStripStatusLabelGlobalRateText.Text = Tr("Global rate") + ":";
            toolStripStatusLabelBTCDayText.Text =
                "BTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";

            devicesListViewEnableControl1.InitLocale();

            buttonBenchmark.Text = Tr("&Benchmark");
            buttonSettings.Text = Tr("S&ettings");
            buttonStartMining.Text = Tr("&Start All");
            buttonStopMining.Text = Tr("&Stop All");
            buttonHelp.Text = Tr("&Help");
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
            //BalanceCallback(null, null); // update currency changes

            //if (_isDeviceDetectionInitialized)
            //{
            //    devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Available.Devices);
            //}

            devicesListViewEnableControl1.SetPayingColumns();
        }

        public void AfterLoadComplete()
        {
            _loadingScreen = null;
            Enabled = true;

            IdleCheckManager.StartIdleCheck(ConfigManager.GeneralConfig.IdleCheckType, IdleCheck);
        }
        
        private void IdleCheck(object sender, IdleChangedEventArgs e)
        {
            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle || _isManuallyStarted) return;

            // TODO set is mining here
            if (ApplicationStateManager.IsCurrentlyMining)
            {
                if (!e.IsIdle)
                {
                    ApplicationStateManager.StopAllDevice();
                    Helpers.ConsolePrint("NICEHASH", "Resumed from idling");
                }
            }
            else if (_benchmarkForm == null && e.IsIdle)
            {
                Helpers.ConsolePrint("NICEHASH", "Entering idling state");
                if (StartMining(false) != StartMiningReturnType.StartMining)
                {
                    ApplicationStateManager.StopAllDevice();
                }
            }
        }

        // This is a single shot _benchmarkTimer
        private void StartupTimer_Tick(object sender, EventArgs e)
        {
            _startupTimer.Stop();
            _startupTimer = null;

            //// TODO temporary hooks
            ApplicationStateManager._ratesComunication = devicesListViewEnableControl1;

            // Internals Init
            // TODO add loading step
            MinersSettingsManager.Init();

            // Query Available ComputeDevices
            ComputeDeviceManager.Query.QueryDevices(_loadingScreen);
            //_isDeviceDetectionInitialized = true;

            /////////////////////////////////////////////
            /////// from here on we have our devices and Miners initialized
            ApplicationStateManager.AfterDeviceQueryInitialization();
            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Saving config..."));

            // All devices settup should be initialized in AllDevices
            devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Available.Devices);
            //// set properties after
            //devicesListViewEnableControl1.SaveToGeneralConfig = true;

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Tr("Checking for latest version..."));

            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Getting NiceHash SMA information..."));
            // Init ws connection
            NiceHashStats.OnConnectionLost += ConnectionLostCallback;
            NiceHashStats.OnExchangeUpdate += UpdateExchange;
            NiceHashStats.StartConnection(Links.NhmSocketAddress);

            // increase timeout
            if (Globals.IsFirstNetworkCheckTimeout)
            {
                while (!Helpers.WebRequestTestGoogle() && Globals.FirstNetworkCheckTimeoutTries > 0)
                {
                    --Globals.FirstNetworkCheckTimeoutTries;
                }
            }

            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Getting Bitcoin exchange rate..."));
            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Setting environment variables..."));
            Helpers.SetDefaultEnvironmentVariables();
            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Setting Windows error reporting..."));

            Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);

            _loadingScreen.IncreaseLoadCounter();
            if (ConfigManager.GeneralConfig.NVIDIAP0State)
            {
                _loadingScreen.SetInfoMsg(Tr("Changing all supported NVIDIA GPUs to P0 state..."));
                Helpers.SetNvidiaP0State();
            }

            _loadingScreen.FinishLoad();

            var runVCRed = !MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit;
            // standard miners check scope
            {
                // check if download needed
                if (!MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit)
                {
                    var downloadUnzipForm =
                        new Form_Loading(new MinersDownloader(MinersDownloadManager.StandardDlSetup));
                    SetChildFormCenter(downloadUnzipForm);
                    downloadUnzipForm.ShowDialog();
                }
                // check if files are mising
                if (!MinersExistanceChecker.IsMinersBinsInit())
                {
                    var result = MessageBox.Show(Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. NiceHash Miner Legacy might not work properly without missing files. Click Yes to reinitialize NiceHash Miner Legacy to try to fix this issue."),
                        Tr("Warning!"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        ConfigManager.GeneralConfig.DownloadInit = false;
                        ConfigManager.GeneralConfigFileCommit();
                        ApplicationStateManager.RestartProgram();
                        return;
                    }
                }
                else if (!ConfigManager.GeneralConfig.DownloadInit)
                {
                    // all good
                    ConfigManager.GeneralConfig.DownloadInit = true;
                    ConfigManager.GeneralConfigFileCommit();
                }
            }
            // 3rdparty miners check scope #2
            {
                // check if download needed
                if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES)
                {
                    if (!MinersExistanceChecker.IsMiners3rdPartyBinsInit() &&
                        !ConfigManager.GeneralConfig.DownloadInit3rdParty)
                    {
                        var download3rdPartyUnzipForm =
                            new Form_Loading(new MinersDownloader(MinersDownloadManager.ThirdPartyDlSetup));
                        SetChildFormCenter(download3rdPartyUnzipForm);
                        download3rdPartyUnzipForm.ShowDialog();
                    }
                    // check if files are mising
                    if (!MinersExistanceChecker.IsMiners3rdPartyBinsInit())
                    {
                        var result = MessageBox.Show(Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. NiceHash Miner Legacy might not work properly without missing files. Click Yes to reinitialize NiceHash Miner Legacy to try to fix this issue."),
                            Tr("Warning!"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            ConfigManager.GeneralConfig.DownloadInit3rdParty = false;
                            ConfigManager.GeneralConfigFileCommit();
                            ApplicationStateManager.RestartProgram();
                            return;
                        }
                    }
                    else if (!ConfigManager.GeneralConfig.DownloadInit3rdParty)
                    {
                        // all good
                        ConfigManager.GeneralConfig.DownloadInit3rdParty = true;
                        ConfigManager.GeneralConfigFileCommit();
                    }
                }
            }

            if (runVCRed)
            {
                Helpers.InstallVcRedist();
            }

            ApplicationStateManager.InitFinished();
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

        private void SetChildFormCenter(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(Location.X + (Width - form.Width) / 2, Location.Y + (Height - form.Height) / 2);
        }

        private void Form_Main_Shown(object sender, EventArgs e)
        {
            //new StateDumpForm().Show();
            // general loading indicator
            const int totalLoadSteps = 11;
            _loadingScreen = new Form_Loading(this,
                Tr("Loading, please wait..."),
                Tr("Querying CPU devices..."), totalLoadSteps);
            SetChildFormCenter(_loadingScreen);
            _loadingScreen.Show();

            _startupTimer = new Timer();
            _startupTimer.Tick += StartupTimer_Tick;
            _startupTimer.Interval = 200;
            _startupTimer.Start();
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
                Helpers.ConsolePrint("NiceHash", e.ToString());
            }
        }

        private void UpdateGlobalRate(double totalRate)
        {
            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && totalRate < 0.1)
            {
                toolStripStatusLabelBTCDayText.Text =
                    "mBTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * 1000 * TimeFactor.TimeUnit).ToString("F5", CultureInfo.InvariantCulture);
            }
            else
            {
                toolStripStatusLabelBTCDayText.Text =
                    "BTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * TimeFactor.TimeUnit).ToString("F6", CultureInfo.InvariantCulture);
            }

            toolStripStatusLabelBTCDayValue.Text = ExchangeRateApi
                .ConvertToActiveCurrency((totalRate * TimeFactor.TimeUnit * ExchangeRateApi.GetUsdExchangeRate()))
                .ToString("F2", CultureInfo.InvariantCulture);
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
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

                Helpers.ConsolePrint("NICEHASH",
                    "Current Bitcoin rate: " + br.ToString("F2", CultureInfo.InvariantCulture));
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

            Process.Start(Links.CheckStats + textBoxBTCAddress.Text.Trim());
        }


        private void LinkLabelChooseBTCWallet_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Links.NhmBtcWalletFaq);
        }

        private void LinkLabelNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ApplicationStateManager.VisitNewVersionUrl();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormHelpers.UnsubscribeAllControls(this);
            ApplicationStateManager.BeforeExit();
            MessageBoxManager.Unregister();
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
                InitMainConfigGuiData();
                AfterLoadComplete();
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

        // Minimize to system tray if MinimizeToTray is set to true
        private void Form1_Resize(object sender, EventArgs e)
        {
            notifyIcon1.Icon = Properties.Resources.logo;
            notifyIcon1.Text = Application.ProductName + " v" + Application.ProductVersion +
                               "\nDouble-click to restore..";

            //devicesMainBoard1.Size = devicesListViewEnableControl1.Size;

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
                    Helpers.ConsolePrint("NICEHASH", $"After {i}s has data: {hasData}");
                }
            }

            if (!hasData)
            {
                Helpers.ConsolePrint("NICEHASH", "No data received within timeout");
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
            var username = _demoMode ? Globals.DemoUser : Globals.GetUsername(); // TODO we get username from here
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
    }
}
#endif

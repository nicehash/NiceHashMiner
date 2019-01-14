using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Forms;
using NiceHashMiner.Forms.Components;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Miners;
using NiceHashMiner.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using NiceHashMiner.Miners.IdleChecking;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;
using SystemTimer = System.Timers.Timer;
using Timer = System.Windows.Forms.Timer;

namespace NiceHashMiner
{
    using System.IO;

    public partial class Form_Main : Form, Form_Loading.IAfterInitializationCaller, IGlobalRatesUpdate, IDataVisualizer, IBTCDisplayer, IWorkerNameDisplayer, IServiceLocationDisplayer, IVersionDisplayer, IBalanceBTCDisplayer, IBalanceFiatDisplayer
    {
        private Timer _startupTimer;
        private Timer _idleCheck;
        private SystemTimer _computeDevicesCheckTimer;

        private bool _showWarningNiceHashData;
        private bool _demoMode;

        private Form_Loading _loadingScreen;
        private Form_Benchmark _benchmarkForm;

        private int _flowLayoutPanelVisibleCount = 0;
        private int _flowLayoutPanelRatesIndex = 0;

        

        private bool _isDeviceDetectionInitialized = false;

        private bool _isManuallyStarted = false;
        private bool _isNotProfitable = false;

        //private bool _isSmaUpdated = false;

        private readonly int _mainFormHeight = 0;
        private readonly int _emtpyGroupPanelHeight = 0;

        public Form_Main()
        {
            InitializeComponent();
            ApplicationStateManager.SubscribeStateDisplayer(this);

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

            //label_NotProfitable.Visible = false;

            InitMainConfigGuiData();
        }

        ~Form_Main()
        {
            ApplicationStateManager.UnsubscribeStateDisplayer(this);
        }

        private void InitLocalization()
        {
            MessageBoxManager.Unregister();
            MessageBoxManager.Yes = Translations.Tr("&Yes");
            MessageBoxManager.No = Translations.Tr("&No");
            MessageBoxManager.OK = Translations.Tr("&OK");
            MessageBoxManager.Cancel = Translations.Tr("&Cancel");
            MessageBoxManager.Retry = Translations.Tr("&Retry");
            MessageBoxManager.Register();

            labelServiceLocation.Text = Translations.Tr("Service location") + ":";
            {
                // TODO keep in mind the localizations
                var i = 0;
                foreach (var loc in StratumService.MiningLocations)
                {
                    comboBoxLocation.Items[i] = Translations.Tr((string)StratumService.MiningLocationNames[i]);
                    i++;
                }
            }
            labelBitcoinAddress.Text = Translations.Tr("Bitcoin Address") + ":";
            labelWorkerName.Text = Translations.Tr("Worker Name") + ":";

            linkLabelCheckStats.Text = Translations.Tr("Check my stats online!");
            linkLabelChooseBTCWallet.Text = Translations.Tr("Help me choose my Bitcoin wallet");

            toolStripStatusLabelGlobalRateText.Text = Translations.Tr("Global rate") + ":";
            toolStripStatusLabelBTCDayText.Text =
                "BTC/" + Translations.Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Translations.Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Translations.Tr("Balance") + ":";

            devicesListViewEnableControl1.InitLocale();

            buttonBenchmark.Text = Translations.Tr("&Benchmark");
            buttonSettings.Text = Translations.Tr("S&ettings");
            buttonStartMining.Text = Translations.Tr("&Start");
            buttonStopMining.Text = Translations.Tr("St&op");
            buttonHelp.Text = Translations.Tr("&Help");

            //label_NotProfitable.Text = Translations.Tr("CURRENTLY MINING NOT PROFITABLE.");
            //groupBox1.Text = Translations.Tr("Group/Device Rates:");
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
                                                   Translations.Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Translations.Tr("Balance") + ":";
            //BalanceCallback(null, null); // update currency changes

            if (_isDeviceDetectionInitialized)
            {
                devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Available.Devices);
            }

            devicesListViewEnableControl1.SetPayingColumns();
            devicesListViewEnableControl1.GlobalRates = this;
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
                    StopMining(true);
                    Helpers.ConsolePrint("NICEHASH", "Resumed from idling");
                }
            }
            else if (_benchmarkForm == null && e.IsIdle)
            {
                Helpers.ConsolePrint("NICEHASH", "Entering idling state");
                if (StartMining(false) != StartMiningReturnType.StartMining)
                {
                    StopMining(true);
                }
            }
        }

        // This is a single shot _benchmarkTimer
        private void StartupTimer_Tick(object sender, EventArgs e)
        {
            _startupTimer.Stop();
            _startupTimer = null;

            // Internals Init
            // TODO add loading step
            MinersSettingsManager.Init();

            // Query Available ComputeDevices
            ComputeDeviceManager.Query.QueryDevices(_loadingScreen);
            _isDeviceDetectionInitialized = true;

            /////////////////////////////////////////////
            /////// from here on we have our devices and Miners initialized
            ConfigManager.AfterDeviceQueryInitialization();
            _loadingScreen.IncreaseLoadCounterAndMessage(Translations.Tr("Saving config..."));

            // All devices settup should be initialized in AllDevices
            devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Available.Devices);
            // set properties after
            devicesListViewEnableControl1.SaveToGeneralConfig = true;

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Translations.Tr("Checking for latest version..."));

            _loadingScreen.IncreaseLoadCounterAndMessage(Translations.Tr("Getting NiceHash SMA information..."));
            // Init ws connection
            NiceHashStats.OnConnectionLost += ConnectionLostCallback;
            NiceHashStats.OnVersionBurn += VersionBurnCallback;
            NiceHashStats.OnExchangeUpdate += ExchangeCallback;
            NiceHashStats.StartConnection(Links.NhmSocketAddress, this, devicesListViewEnableControl1);

            // increase timeout
            if (Globals.IsFirstNetworkCheckTimeout)
            {
                while (!Helpers.WebRequestTestGoogle() && Globals.FirstNetworkCheckTimeoutTries > 0)
                {
                    --Globals.FirstNetworkCheckTimeoutTries;
                }
            }

            _loadingScreen.IncreaseLoadCounterAndMessage(Translations.Tr("Getting Bitcoin exchange rate..."));
            _loadingScreen.IncreaseLoadCounterAndMessage(
                Translations.Tr("Setting environment variables..."));
            Helpers.SetDefaultEnvironmentVariables();

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Translations.Tr("Setting Windows error reporting..."));

            Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);

            _loadingScreen.IncreaseLoadCounter();
            if (ConfigManager.GeneralConfig.NVIDIAP0State)
            {
                _loadingScreen.SetInfoMsg(Translations.Tr("Changing all supported NVIDIA GPUs to P0 state..."));
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
                    var result = MessageBox.Show(Translations.Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. NiceHash Miner Legacy might not work properly without missing files. Click Yes to reinitialize NiceHash Miner Legacy to try to fix this issue."),
                        Translations.Tr("Warning!"),
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
                        var result = MessageBox.Show(Translations.Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. NiceHash Miner Legacy might not work properly without missing files. Click Yes to reinitialize NiceHash Miner Legacy to try to fix this issue."),
                            Translations.Tr("Warning!"),
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


            if (ConfigManager.GeneralConfig.AutoStartMining)
            {
                // well this is started manually as we want it to start at runtime
                _isManuallyStarted = true;
                if (StartMining(false) != StartMiningReturnType.StartMining)
                {
                    _isManuallyStarted = false;
                    StopMining(true);
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
            new StateDumpForm().Show();
            // general loading indicator
            const int totalLoadSteps = 11;
            _loadingScreen = new Form_Loading(this,
                Translations.Tr("Loading, please wait..."),
                Translations.Tr("Querying CPU devices..."), totalLoadSteps);
            SetChildFormCenter(_loadingScreen);
            _loadingScreen.Show();

            _startupTimer = new Timer();
            _startupTimer.Tick += StartupTimer_Tick;
            _startupTimer.Interval = 200;
            _startupTimer.Start();
        }

//        [Obsolete("Deprecated in favour of AlgorithmSwitchingManager timer")]
//       private async void SMAMinerCheck_Tick(object sender, EventArgs e)
//        {
//            _smaMinerCheck.Interval = ConfigManager.GeneralConfig.SwitchMinSecondsFixed * 1000 +
//                                      R.Next(ConfigManager.GeneralConfig.SwitchMinSecondsDynamic * 1000);
//            if (ComputeDeviceManager.Group.ContainsAmdGpus)
//            {
//                _smaMinerCheck.Interval =
//                    (ConfigManager.GeneralConfig.SwitchMinSecondsAMD +
//                     ConfigManager.GeneralConfig.SwitchMinSecondsFixed) * 1000 +
//                    R.Next(ConfigManager.GeneralConfig.SwitchMinSecondsDynamic * 1000);
//            }

//#if (SWITCH_TESTING)
//            SMAMinerCheck.Interval = MiningDevice.SMAMinerCheckInterval;
//#endif
//            if (_isSmaUpdated)
//            {
//                // Don't bother checking for new profits unless SMA has changed
//                _isSmaUpdated = false;
//                await MinersManager.SwichMostProfitableGroupUpMethod();
//            }
//        }

        private static void ComputeDevicesCheckTimer_Tick(object sender, EventArgs e)
        {
            if (ComputeDeviceManager.Query.CheckVideoControllersCountMismath())
            {
                // less GPUs than before, ACT!
                try
                {
                    var onGpusLost = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\OnGPUsLost.bat")
                    {
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    Process.Start(onGpusLost);
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("NICEHASH", "OnGPUsMismatch.bat error: " + ex.Message);
                }
            }
        }

        private void InitFlowPanelStart()
        {
            //flowLayoutPanelRates.Controls.Clear();
            //// add for every cdev a 
            //foreach (var cdev in ComputeDeviceManager.Available.Devices)
            //{
            //    if (cdev.Enabled)
            //    {
            //        var newGroupProfitControl = new GroupProfitControl
            //        {
            //            Visible = false
            //        };
            //        flowLayoutPanelRates.Controls.Add(newGroupProfitControl);
            //    }
            //}
        }

        public void ClearRatesAll()
        {
            HideNotProfitable();
        }

        public void AddRateInfo(string groupName, string deviceStringInfo, ApiData iApiData, double paying,
            bool isApiGetException)
        {
            var apiGetExceptionString = isApiGetException ? "**" : "";

            var speedString =
                Helpers.FormatDualSpeedOutput(iApiData.Speed, iApiData.SecondarySpeed, iApiData.AlgorithmID) +
                iApiData.AlgorithmName + apiGetExceptionString;
            //var rateBtcString = FormatPayingOutput(paying);
            var rateCurrencyString = ExchangeRateApi
                                         .ConvertToActiveCurrency(paying * ExchangeRateApi.GetUsdExchangeRate() * TimeFactor.TimeUnit)
                                         .ToString("F2", CultureInfo.InvariantCulture)
                                     + $" {ExchangeRateApi.ActiveDisplayCurrency}/" +
                                     Translations.Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());

            //try
            //{
            //    // flowLayoutPanelRatesIndex may be OOB, so catch
            //    ((GroupProfitControl) flowLayoutPanelRates.Controls[_flowLayoutPanelRatesIndex++])
            //        .UpdateProfitStats(groupName, deviceStringInfo, speedString, rateBtcString, rateCurrencyString);
            //}
            //catch { }

            UpdateGlobalRate();
        }

        public void ShowNotProfitable(string msg)
        {
            if (ConfigManager.GeneralConfig.UseIFTTT)
            {
                if (!_isNotProfitable)
                {
                    Ifttt.PostToIfttt("nicehash", msg);
                    _isNotProfitable = true;
                }
            }

            if (InvokeRequired)
            {
                Invoke((Action) delegate
                {
                    ShowNotProfitable(msg);
                });
            }
            else
            {
                //label_NotProfitable.Visible = true;
                //label_NotProfitable.Text = msg;
                //label_NotProfitable.Invalidate();
            }
        }

        public void HideNotProfitable()
        {
            if (ConfigManager.GeneralConfig.UseIFTTT)
            {
                if (_isNotProfitable)
                {
                    Ifttt.PostToIfttt("nicehash", "Mining is once again profitable and has resumed.");
                    _isNotProfitable = false;
                }
            }

            if (InvokeRequired)
            {
                Invoke((Action) HideNotProfitable);
            }
            else
            {
                //label_NotProfitable.Visible = false;
                //label_NotProfitable.Invalidate();
            }
        }

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

        public void UpdateGlobalRate()
        {
            var totalRate = MinersManager.GetTotalRate();

            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && totalRate < 0.1)
            {
                toolStripStatusLabelBTCDayText.Text =
                    "mBTC/" + Translations.Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * 1000 * TimeFactor.TimeUnit).ToString("F5", CultureInfo.InvariantCulture);
            }
            else
            {
                toolStripStatusLabelBTCDayText.Text =
                    "BTC/" + Translations.Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * TimeFactor.TimeUnit).ToString("F6", CultureInfo.InvariantCulture);
            }

            toolStripStatusLabelBTCDayValue.Text = ExchangeRateApi
                .ConvertToActiveCurrency((totalRate * TimeFactor.TimeUnit * ExchangeRateApi.GetUsdExchangeRate()))
                .ToString("F2", CultureInfo.InvariantCulture);
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Translations.Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Translations.Tr("Balance") + ":";
        }

        private void ExchangeCallback(object sender, EventArgs e)
        {
            //// We are getting data from socket so stop checking manually
            //_bitcoinExchangeCheck?.Stop();
            //Helpers.ConsolePrint("NICEHASH", "Bitcoin rate get");
            if (InvokeRequired)
            {
                Invoke((MethodInvoker) UpdateExchange);
            }
            else
            {
                UpdateExchange();
            }
        }

        private void UpdateExchange()
        {
            var br = ExchangeRateApi.GetUsdExchangeRate();
            var currencyRate = Translations.Tr("N/A");
            if (br > 0)
            {
                currencyRate = ExchangeRateApi.ConvertToActiveCurrency(br).ToString("F2");
            }

            toolTip1.SetToolTip(statusStrip1, $"1 BTC = {currencyRate} {ExchangeRateApi.ActiveDisplayCurrency}");

            Helpers.ConsolePrint("NICEHASH",
                "Current Bitcoin rate: " + br.ToString("F2", CultureInfo.InvariantCulture));
        }

        private void VersionBurnCallback(object sender, SocketEventArgs e)
        {
            BeginInvoke((Action) (() =>
            {
                StopMining(true);
                _benchmarkForm?.StopBenchmark();
                var dialogResult = MessageBox.Show(e.Message, Translations.Tr("Error!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }));
        }


        private void ConnectionLostCallback(object sender, EventArgs e)
        {
            if (!NHSmaData.HasData && ConfigManager.GeneralConfig.ShowInternetConnectionWarning &&
                _showWarningNiceHashData)
            {
                _showWarningNiceHashData = false;
                var dialogResult = MessageBox.Show(Translations.Tr("NiceHash Miner Legacy requires internet connection to run. Please ensure that you are connected to the internet before running NiceHash Miner Legacy. Would you like to continue?"),
                    Translations.Tr("Check internet connection"),
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
                var result = MessageBox.Show(Translations.Tr("Invalid Bitcoin address!\n\nPlease enter a valid Bitcoin address or choose Yes to create one."),
                    Translations.Tr("Error!"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                    Process.Start(Links.NhmBtcWalletFaq);

                textBoxBTCAddress.Focus();
                return false;
            }
            if (!BitcoinAddress.ValidateWorkerName(textBoxWorkerName.Text.Trim()) && showError)
            {
                var result = MessageBox.Show(Translations.Tr("Invalid workername!\n\nPlease enter a valid workername (Aa-Zz, 0-9, up to 15 character long)."),
                    Translations.Tr("Error!"),
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
            MinersManager.StopAllMiners(true);

            MessageBoxManager.Unregister();
        }

        private void ButtonBenchmark_Click(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.ServiceLocation = comboBoxLocation.SelectedIndex;

            _benchmarkForm = new Form_Benchmark();
            SetChildFormCenter(_benchmarkForm);
            _benchmarkForm.ShowDialog();
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
            settings.ShowDialog();

            if (settings.IsChange && settings.IsChangeSaved && settings.IsRestartNeeded)
            {
                MessageBox.Show(
                    Translations.Tr("Settings change requires NiceHash Miner Legacy to restart."),
                    Translations.Tr("Restart Notice"),
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
                StopMining(false);
                MessageBox.Show(Translations.Tr("NiceHash Miner Legacy cannot start mining. Make sure you have at least one enabled device that has at least one enabled and benchmarked algorithm."),
                    Translations.Tr("Warning!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ButtonStopMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = false;
            StopMining(false);
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
                    //var dialogResult = MessageBox.Show(Translations.Tr("Invalid Bitcoin address!\n\nPlease enter a valid Bitcoin address or choose Yes to create one."),
                    //Translations.Tr("Error!"),
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
                    var result = MessageBox.Show(Translations.Tr("You have not entered a bitcoin address. NiceHash Miner Legacy will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!"),
                        Translations.Tr("Start mining in DEMO mode?"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        _demoMode = true;
                        labelDemoMode.Visible = true;
                        labelDemoMode.Text = Translations.Tr("NiceHash Miner Legacy is running in DEMO mode!");
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
                    MessageBox.Show(Translations.Tr("Unable to get NiceHash profitability data. If you are connected to internet, try again later."),
                        Translations.Tr("Error!"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return StartMiningReturnType.IgnoreMsg;
            }


            // Check if there are unbenchmakred algorithms
            var isBenchInit = true;
            foreach (var cdev in ComputeDeviceManager.Available.Devices)
            {
                if (cdev.Enabled)
                {
                    if (cdev.GetAlgorithmSettings().Where(algo => algo.Enabled).Any(algo => algo.BenchmarkSpeed == 0))
                    {
                        isBenchInit = false;
                    }
                }
            }
            // Check if the user has run benchmark first
            if (!isBenchInit)
            {
                var result = DialogResult.No;
                if (showWarnings)
                {
                    result = MessageBox.Show(Translations.Tr("There are unbenchmarked algorithms for selected enabled devices. Click Yes to benchmark and start mining, No to skip benchmark and continue mining, Cancel to abort"),
                        Translations.Tr("Warning!"),
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                }
                if (result == DialogResult.Yes)
                {
                    _benchmarkForm = new Form_Benchmark(
                        BenchmarkPerformanceType.Standard,
                        true);
                    SetChildFormCenter(_benchmarkForm);
                    _benchmarkForm.ShowDialog();
                    _benchmarkForm = null;
                    InitMainConfigGuiData();
                }
                else if (result == DialogResult.No)
                {
                    // check devices without benchmarks
                    foreach (var cdev in ComputeDeviceManager.Available.Devices)
                    {
                        if (cdev.Enabled)
                        {
                            var enabled = cdev.GetAlgorithmSettings().Any(algo => algo.BenchmarkSpeed > 0);
                            cdev.Enabled = enabled;
                        }
                    }
                }
                else
                {
                    return StartMiningReturnType.IgnoreMsg;
                }
            }

            // TODO Globals.GetWorkerName(), Globals.GetBitcoinUser()
            var btcAdress = _demoMode ? Globals.DemoUser : ConfigManager.GeneralConfig.BitcoinAddress;
            var isMining = MinersManager.StartInitialize(devicesListViewEnableControl1, StratumService.MiningLocations[comboBoxLocation.SelectedIndex],
                textBoxWorkerName.Text.Trim(), btcAdress);

            StartMiningGui();
            // TODO TEMP
            ApplicationStateManager.StartMining();

            return isMining ? StartMiningReturnType.StartMining : StartMiningReturnType.ShowNoMining;
        }

        public void StartMiningGui()
        {
            if (InvokeRequired)
            {
                Invoke((Action) StartMiningGui);
            }
            else
            {
                textBoxBTCAddress.Enabled = false;
                textBoxWorkerName.Enabled = false;
                comboBoxLocation.Enabled = false;
                buttonBenchmark.Enabled = false;
                buttonStartMining.Enabled = false;
                buttonSettings.Enabled = false;
                devicesListViewEnableControl1.SetIsMining(true);
                buttonStopMining.Enabled = true;

                // Disable profitable notification on start
                _isNotProfitable = false;

                InitFlowPanelStart();
                ClearRatesAll();

                //_minerStatsCheck.Start();

                if (!ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost) return;
                _computeDevicesCheckTimer = new SystemTimer();
                _computeDevicesCheckTimer.Elapsed += ComputeDevicesCheckTimer_Tick;
                _computeDevicesCheckTimer.Interval = 60000;

                _computeDevicesCheckTimer.Start();
            }
        }

        private void StopMining(bool headless)
        {
            MinersManager.StopAllMiners(headless);
            StopMiningGui();
            // TODO TEMP
            ApplicationStateManager.StopMining();
        }

        public void StopMiningGui()
        {
            if (InvokeRequired)
            {
                Invoke((Action) StopMiningGui);
            }
            else
            {
                //_minerStatsCheck.Stop();
                _computeDevicesCheckTimer?.Stop();

                // Disable IFTTT notification before label call
                _isNotProfitable = false;
                
                textBoxBTCAddress.Enabled = true;
                textBoxWorkerName.Enabled = true;
                comboBoxLocation.Enabled = true;
                buttonBenchmark.Enabled = true;
                buttonStartMining.Enabled = true;
                buttonSettings.Enabled = true;
                devicesListViewEnableControl1.SetIsMining(false);
                buttonStopMining.Enabled = false;

                if (_demoMode)
                {
                    _demoMode = false;
                    labelDemoMode.Visible = false;
                }

                UpdateGlobalRate();
            }
        }

        private void Form_Main_ResizeEnd(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.MainFormSize.X = Width;
            ConfigManager.GeneralConfig.MainFormSize.Y = Height;
        }

        // StateDisplay interfaces
        void IBTCDisplayer.DisplayBTC(string btc)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                textBoxBTCAddress.Text = btc;
            });
        }

        void IWorkerNameDisplayer.DisplayWorkerName(string workerName)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                textBoxWorkerName.Text = workerName;
            });
        }

        void IServiceLocationDisplayer.DisplayServiceLocation(int serviceLocation)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                comboBoxLocation.SelectedIndex = serviceLocation;
            });
        }

        void IVersionDisplayer.DisplayVersion(string version)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                linkLabelNewVersion.Text = version;
            });
        }

        // TODO this might need some formatters?
        void IBalanceBTCDisplayer.DisplayBTCBalance(double btcBalance)
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

        void IBalanceFiatDisplayer.DisplayFiatBalance(double fiatBalance, string fiatCurrencySymbol)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                toolStripStatusLabelBalanceDollarText.Text = fiatBalance.ToString("F2", CultureInfo.InvariantCulture);
                toolStripStatusLabelBalanceDollarValue.Text = $"({fiatCurrencySymbol})";
            });
        }
    }
}

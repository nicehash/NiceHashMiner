using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Forms;
using NiceHashMiner.Forms.Components;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
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
using static NiceHashMiner.Translations;
using Timer = System.Windows.Forms.Timer;

namespace NiceHashMiner
{
    public partial class Form_Main : Form, Form_Loading.IAfterInitializationCaller, IMainFormRatesComunication
    {
        private string _visitUrlNew = Links.VisitUrlNew;

        private Timer _minerStatsCheck;
        //private Timer _smaMinerCheck;
        //private Timer _bitcoinExchangeCheck;
        private Timer _startupTimer;
        private Timer _idleCheck;

        private bool _showWarningNiceHashData;
        private bool _demoMode;

        private readonly Random R;

        private Form_Loading _loadingScreen;
        private Form_Benchmark _benchmarkForm;

        private int _flowLayoutPanelVisibleCount = 0;
        private int _flowLayoutPanelRatesIndex = 0;

        private const string BetaAlphaPostfixString = "";

        private bool _isDeviceDetectionInitialized = false;

        private bool _isManuallyStarted = false;
        private bool _isNotProfitable = false;

        //private bool _isSmaUpdated = false;

        private double _factorTimeUnit = 1.0;

        private readonly int _mainFormHeight = 0;
        private readonly int _emtpyGroupPanelHeight = 0;

        private CudaDeviceChecker _cudaChecker;

        public Form_Main()
        {
            InitializeComponent();
            Icon = Properties.Resources.logo;

            InitLocalization();

            // Log the computer's amount of Total RAM and Page File Size
            var moc = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem").Get();
            foreach (ManagementObject mo in moc)
            {
                var totalRam = long.Parse(mo["TotalVisibleMemorySize"].ToString()) / 1024;
                var pageFileSize = (long.Parse(mo["TotalVirtualMemorySize"].ToString()) / 1024) - totalRam;
                Helpers.ConsolePrint("NICEHASH", "Total RAM: " + totalRam + "MB");
                Helpers.ConsolePrint("NICEHASH", "Page File Size: " + pageFileSize + "MB");
            }

            R = new Random((int) DateTime.Now.Ticks);

            Text += " v" + Application.ProductVersion + BetaAlphaPostfixString;

            label_NotProfitable.Visible = false;

            InitMainConfigGuiData();

            // for resizing
            InitFlowPanelStart();

            if (groupBox1.Size.Height > 0 && Size.Height > 0)
            {
                _emtpyGroupPanelHeight = groupBox1.Size.Height;
                _mainFormHeight = Size.Height - _emtpyGroupPanelHeight;
            }
            else
            {
                _emtpyGroupPanelHeight = 59;
                _mainFormHeight = 330 - _emtpyGroupPanelHeight;
            }
            ClearRatesAll();
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

            //todo make this dinamically
            labelServiceLocation.Text = Tr("Service location") + ":";
            {
                comboBoxLocation.Items[0] = Tr("Europe - Amsterdam");
                comboBoxLocation.Items[1] = Tr("USA - San Jose");
                comboBoxLocation.Items[2] = Tr("China - Hong Kong");
                comboBoxLocation.Items[3] = Tr("Japan - Tokyo");
                comboBoxLocation.Items[4] = Tr("India - Chennai");
                comboBoxLocation.Items[5] = Tr("Brazil - Sao Paulo");
            }
            labelBitcoinAddress.Text = Tr("Bitcoin Address") + ":";
            labelWorkerName.Text = Tr("Worker Name") + ":";

            linkLabelCheckStats.Text = Tr("Check my stats online!");
            linkLabelChooseBTCWallet.Text = Tr("Help me choose my Bitcoin wallet");

            toolStripStatusLabelGlobalRateText.Text = Tr("Global rate") + ":";
            toolStripStatusLabelBTCDayText.Text =
                "BTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";

            devicesListViewEnableControl1.InitLocale();

            buttonBenchmark.Text = Tr("&Benchmark");
            buttonSettings.Text = Tr("S&ettings");
            buttonStartMining.Text = Tr("&Start");
            buttonStopMining.Text = Tr("St&op");
            buttonHelp.Text = Tr("&Help");

            label_NotProfitable.Text = Tr("CURRENTLY MINING NOT PROFITABLE.");
            groupBox1.Text = Tr("Group/Device Rates:");
        }

        private void InitMainConfigGuiData()
        {
            if (ConfigManager.GeneralConfig.ServiceLocation >= 0 &&
                ConfigManager.GeneralConfig.ServiceLocation < Globals.MiningLocation.Length)
                comboBoxLocation.SelectedIndex = ConfigManager.GeneralConfig.ServiceLocation;
            else
                comboBoxLocation.SelectedIndex = 0;

            textBoxBTCAddress.Text = ConfigManager.GeneralConfig.BitcoinAddress;
            textBoxWorkerName.Text = ConfigManager.GeneralConfig.WorkerName;

            _showWarningNiceHashData = true;
            _demoMode = false;

            // init active display currency after config load
            ExchangeRateApi.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

            // init factor for Time Unit
            switch (ConfigManager.GeneralConfig.TimeUnit)
            {
                case TimeUnitType.Hour:
                    _factorTimeUnit = 1.0 / 24.0;
                    break;
                case TimeUnitType.Day:
                    _factorTimeUnit = 1;
                    break;
                case TimeUnitType.Week:
                    _factorTimeUnit = 7;
                    break;
                case TimeUnitType.Month:
                    _factorTimeUnit = 30;
                    break;
                case TimeUnitType.Year:
                    _factorTimeUnit = 365;
                    break;
            }

            toolStripStatusLabelBalanceDollarValue.Text = "(" + ExchangeRateApi.ActiveDisplayCurrency + ")";
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";
            BalanceCallback(null, null); // update currency changes

            if (_isDeviceDetectionInitialized)
            {
                devicesListViewEnableControl1.ResetComputeDevices(AvailableDevices.Devices);
            }
        }

        public void AfterLoadComplete()
        {
            _loadingScreen = null;
            Enabled = true;

            _idleCheck = new Timer();
            _idleCheck.Tick += IdleCheck_Tick;
            _idleCheck.Interval = 500;
            _idleCheck.Start();
        }


        private void IdleCheck_Tick(object sender, EventArgs e)
        {
            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle || _isManuallyStarted) return;

            var msIdle = Helpers.GetIdleTime();

            if (_minerStatsCheck.Enabled)
            {
                if (msIdle < (ConfigManager.GeneralConfig.MinIdleSeconds * 1000))
                {
                    StopMining();
                    Helpers.ConsolePrint("NICEHASH", "Resumed from idling");
                }
            }
            else
            {
                if (_benchmarkForm == null && (msIdle > (ConfigManager.GeneralConfig.MinIdleSeconds * 1000)))
                {
                    Helpers.ConsolePrint("NICEHASH", "Entering idling state");
                    if (StartMining(false) != StartMiningReturnType.StartMining)
                    {
                        StopMining();
                    }
                }
            }
        }

        // This is a single shot _benchmarkTimer
        private async void StartupTimer_Tick(object sender, EventArgs e)
        {
            _startupTimer.Stop();
            _startupTimer = null;

            // Internals Init
            // TODO add loading step
            MinersSettingsManager.Init();

            if (!Helpers.Is45NetOrHigher())
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy requires .NET Framework 4.5 or higher to work properly. Please install Microsoft .NET Framework 4.5"),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);

                Close();
                return;
            }

            if (!Helpers.Is64BitOperatingSystem)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy supports only x64 platforms. You will not be able to use NiceHash Miner Legacy with x86"),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);

                Close();
                return;
            }

            // 3rdparty miners check scope #1
            {
                // check if setting set
                if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.NOT_SET)
                {
                    // Show TOS
                    Form tos = new Form_3rdParty_TOS();
                    tos.ShowDialog(this);
                }
            }

            // Query Available ComputeDevices
            ComputeDeviceManager.OnProgressUpdate += _loadingScreen.SetMessageAndIncrementStep;
            var query = await ComputeDeviceManager.QueryDevicesAsync();
            ComputeDeviceManager.OnProgressUpdate -= _loadingScreen.SetMessageAndIncrementStep;
            ShowQueryWarnings(query);

            _isDeviceDetectionInitialized = true;

            /////////////////////////////////////////////
            /////// from here on we have our devices and Miners initialized
            ConfigManager.AfterDeviceQueryInitialization();
            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Saving config..."));

            // All devices settup should be initialized in AllDevices
            devicesListViewEnableControl1.ResetComputeDevices(AvailableDevices.Devices);
            // set properties after
            devicesListViewEnableControl1.SaveToGeneralConfig = true;

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Tr("Checking for latest version..."));

            _minerStatsCheck = new Timer();
            _minerStatsCheck.Tick += MinerStatsCheck_Tick;
            _minerStatsCheck.Interval = ConfigManager.GeneralConfig.MinerAPIQueryInterval * 1000;

            //_smaMinerCheck = new Timer();
            //_smaMinerCheck.Tick += SMAMinerCheck_Tick;
            //_smaMinerCheck.Interval = ConfigManager.GeneralConfig.SwitchMinSecondsFixed * 1000 +
            //                          R.Next(ConfigManager.GeneralConfig.SwitchMinSecondsDynamic * 1000);
            //if (ComputeDeviceManager.Group.ContainsAmdGpus)
            //{
            //    _smaMinerCheck.Interval =
            //        (ConfigManager.GeneralConfig.SwitchMinSecondsAMD +
            //         ConfigManager.GeneralConfig.SwitchMinSecondsFixed) * 1000 +
            //        R.Next(ConfigManager.GeneralConfig.SwitchMinSecondsDynamic * 1000);
            //}

            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Getting NiceHash SMA information..."));
            // Init ws connection
            NiceHashStats.OnBalanceUpdate += BalanceCallback;
            NiceHashStats.OnSmaUpdate += SmaCallback;
            NiceHashStats.OnVersionUpdate += VersionUpdateCallback;
            NiceHashStats.OnConnectionLost += ConnectionLostCallback;
            NiceHashStats.OnConnectionEstablished += ConnectionEstablishedCallback;
            NiceHashStats.OnVersionBurn += VersionBurnCallback;
            NiceHashStats.OnExchangeUpdate += ExchangeCallback;
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

            //// Don't start timer if socket is giving data
            //if (ExchangeRateApi.ExchangesFiat == null)
            //{
            //    // Wait a bit and check again
            //    Thread.Sleep(1000);
            //    if (ExchangeRateApi.ExchangesFiat == null)
            //    {
            //        Helpers.ConsolePrint("NICEHASH", "No exchange from socket yet, getting manually");
            //        _bitcoinExchangeCheck = new Timer();
            //        _bitcoinExchangeCheck.Tick += BitcoinExchangeCheck_Tick;
            //        _bitcoinExchangeCheck.Interval = 1000 * 3601; // every 1 hour and 1 second
            //        _bitcoinExchangeCheck.Start();
            //        BitcoinExchangeCheck_Tick(null, null);
            //    }
            //}

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Tr("Setting environment variables..."));
            Helpers.SetDefaultEnvironmentVariables();

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Tr("Setting Windows error reporting..."));

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
                        var pHandle = new Process
                        {
                            StartInfo =
                            {
                                FileName = Application.ExecutablePath
                            }
                        };
                        pHandle.Start();
                        Close();
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
                            var pHandle = new Process
                            {
                                StartInfo =
                                {
                                    FileName = Application.ExecutablePath
                                }
                            };
                            pHandle.Start();
                            Close();
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
                    StopMining();
                }
            }
        }

        private void ShowQueryWarnings(QueryResult query)
        {
            if (query.FailedMinNVDriver)
            {
                MessageBox.Show(string.Format(
                        Tr(
                            "We have detected that your system has Nvidia GPUs, but your driver is older than {0}. In order for NiceHash Miner Legacy to work correctly you should upgrade your drivers to recommended {1} or newer. If you still see this warning after updating the driver please uninstall all your Nvidia drivers and make a clean install of the latest official driver from http://www.nvidia.com."),
                        query.MinDriverString,
                        query.RecommendedDriverString),
                    Tr("Nvidia Recomended driver"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (query.FailedRecommendedNVDriver)
            {
                MessageBox.Show(string.Format(
                        Tr(
                            "We have detected that your Nvidia Driver is older than {0}{1}. We recommend you to update to {2} or newer."),
                        query.RecommendedDriverString,
                        query.CurrentDriverString,
                        query.RecommendedDriverString),
                    Tr("Nvidia Recomended driver"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (query.NoDevices)
            {
                var result = MessageBox.Show(Tr("No supported devices are found. Select the OK button for help or cancel to continue."),
                    Tr("No Supported Devices"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    Process.Start(Links.NhmNoDevHelp);
                }
            }

            if (query.FailedRamCheck)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy recommends increasing virtual memory size so that all algorithms would work fine."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);
            }

            if (query.FailedVidControllerStatus)
            {
                var msg = Tr("We have detected a Video Controller that is not working properly. NiceHash Miner Legacy will not be able to use this Video Controller for mining. We advise you to restart your computer, or reinstall your Video Controller drivers.");
                msg += '\n' + query.FailedVidControllerInfo;
                MessageBox.Show(msg,
                    Tr("Warning! Video Controller not operating correctly"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (query.FailedAmdDriverCheck)
            {
                var warningDialog = new DriverVersionConfirmationDialog();
                warningDialog.ShowDialog();
            }

            if (query.FailedCpu64Bit)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy works only on 64-bit version of OS for CPU mining. CPU mining will be disabled."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (query.FailedCpuCount)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy does not support more than 64 virtual cores. CPU mining will be disabled."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetChildFormCenter(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(Location.X + (Width - form.Width) / 2, Location.Y + (Height - form.Height) / 2);
        }

        private void Form_Main_Shown(object sender, EventArgs e)
        {
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

        private static async void MinerStatsCheck_Tick(object sender, EventArgs e)
        {
            await MinersManager.MinerStatsCheck();
        }

        private void InitFlowPanelStart()
        {
            flowLayoutPanelRates.Controls.Clear();
            // add for every cdev a 
            foreach (var cdev in AvailableDevices.Devices)
            {
                if (cdev.Enabled)
                {
                    var newGroupProfitControl = new GroupProfitControl
                    {
                        Visible = false
                    };
                    flowLayoutPanelRates.Controls.Add(newGroupProfitControl);
                }
            }
        }

        public void ClearRatesAll()
        {
            HideNotProfitable();
            ClearRates(-1);
        }

        public void ClearRates(int groupCount)
        {
            if (InvokeRequired)
            {
                Invoke((Action) delegate { ClearRates(groupCount); });
                return;
            }
            if (_flowLayoutPanelVisibleCount != groupCount)
            {
                _flowLayoutPanelVisibleCount = groupCount;
                // hide some Controls
                var hideIndex = 0;
                foreach (var control in flowLayoutPanelRates.Controls)
                {
                    ((GroupProfitControl) control).Visible = hideIndex < groupCount;
                    ++hideIndex;
                }
            }
            _flowLayoutPanelRatesIndex = 0;
            var visibleGroupCount = 1;
            if (groupCount > 0) visibleGroupCount += groupCount;

            var groupBox1Height = _emtpyGroupPanelHeight;
            if (flowLayoutPanelRates.Controls.Count > 0)
            {
                var control = flowLayoutPanelRates.Controls[0];
                var panelHeight = ((GroupProfitControl) control).Size.Height * 1.2f;
                groupBox1Height = (int) ((visibleGroupCount) * panelHeight);
            }

            groupBox1.Size = new Size(groupBox1.Size.Width, groupBox1Height);
            // set new height
            Size = new Size(Size.Width, _mainFormHeight + groupBox1Height);
        }

        public void AddRateInfo(string groupName, string deviceStringInfo, ApiData iApiData, double paying,
            bool isApiGetException)
        {
            var apiGetExceptionString = isApiGetException ? "**" : "";

            var speedString =
                Helpers.FormatDualSpeedOutput(iApiData.Speed, iApiData.SecondarySpeed, iApiData.AlgorithmID) +
                iApiData.AlgorithmName + apiGetExceptionString;
            var rateBtcString = FormatPayingOutput(paying);
            var rateCurrencyString = ExchangeRateApi
                                         .ConvertToActiveCurrency(paying * ExchangeRateApi.GetUsdExchangeRate() * _factorTimeUnit)
                                         .ToString("F2", CultureInfo.InvariantCulture)
                                     + $" {ExchangeRateApi.ActiveDisplayCurrency}/" +
                                     Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());

            try
            {
                // flowLayoutPanelRatesIndex may be OOB, so catch
                ((GroupProfitControl) flowLayoutPanelRates.Controls[_flowLayoutPanelRatesIndex++])
                    .UpdateProfitStats(groupName, deviceStringInfo, speedString, rateBtcString, rateCurrencyString);
            }
            catch { }

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
                label_NotProfitable.Visible = true;
                label_NotProfitable.Text = msg;
                label_NotProfitable.Invalidate();
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
                label_NotProfitable.Visible = false;
                label_NotProfitable.Invalidate();
            }
        }

        public void ForceMinerStatsUpdate()
        {
            try
            {
                BeginInvoke((Action) (() =>
                {
                    MinerStatsCheck_Tick(null, null);
                }));
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("NiceHash", e.ToString());
            }
        }

        private void UpdateGlobalRate()
        {
            var totalRate = MinersManager.GetTotalRate();

            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && totalRate < 0.1)
            {
                toolStripStatusLabelBTCDayText.Text =
                    "mBTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * 1000 * _factorTimeUnit).ToString("F5", CultureInfo.InvariantCulture);
            }
            else
            {
                toolStripStatusLabelBTCDayText.Text =
                    "BTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * _factorTimeUnit).ToString("F6", CultureInfo.InvariantCulture);
            }

            toolStripStatusLabelBTCDayValue.Text = ExchangeRateApi
                .ConvertToActiveCurrency((totalRate * _factorTimeUnit * ExchangeRateApi.GetUsdExchangeRate()))
                .ToString("F2", CultureInfo.InvariantCulture);
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";
        }


        private void BalanceCallback(object sender, EventArgs e)
        {
            Helpers.ConsolePrint("NICEHASH", "Balance update");
            var balance = NiceHashStats.Balance;
            if (balance > 0)
            {
                if (ConfigManager.GeneralConfig.AutoScaleBTCValues && balance < 0.1)
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "mBTC";
                    toolStripStatusLabelBalanceBTCValue.Text =
                        (balance * 1000).ToString("F5", CultureInfo.InvariantCulture);
                }
                else
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "BTC";
                    toolStripStatusLabelBalanceBTCValue.Text = balance.ToString("F6", CultureInfo.InvariantCulture);
                }

                //Helpers.ConsolePrint("CurrencyConverter", "Using CurrencyConverter" + ConfigManager.Instance.GeneralConfig.DisplayCurrency);
                var amount = (balance * ExchangeRateApi.GetUsdExchangeRate());
                amount = ExchangeRateApi.ConvertToActiveCurrency(amount);
                toolStripStatusLabelBalanceDollarText.Text = amount.ToString("F2", CultureInfo.InvariantCulture);
                toolStripStatusLabelBalanceDollarValue.Text = $"({ExchangeRateApi.ActiveDisplayCurrency})";
            }
        }


        //private void BitcoinExchangeCheck_Tick(object sender, EventArgs e)
        //{
        //    Helpers.ConsolePrint("NICEHASH", "Bitcoin rate get");
        //    ExchangeRateApi.UpdateApi(textBoxWorkerName.Text.Trim());
        //    UpdateExchange();
        //}

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
            var currencyRate = Tr("N/A");
            if (br > 0)
            {
                currencyRate = ExchangeRateApi.ConvertToActiveCurrency(br).ToString("F2");
            }

            toolTip1.SetToolTip(statusStrip1, $"1 BTC = {currencyRate} {ExchangeRateApi.ActiveDisplayCurrency}");

            Helpers.ConsolePrint("NICEHASH",
                "Current Bitcoin rate: " + br.ToString("F2", CultureInfo.InvariantCulture));
        }

        private void SmaCallback(object sender, EventArgs e)
        {
            Helpers.ConsolePrint("NICEHASH", "SMA Update");
            //_isSmaUpdated = true;
        }

        private void VersionBurnCallback(object sender, SocketEventArgs e)
        {
            BeginInvoke((Action) (() =>
            {
                StopMining();
                _benchmarkForm?.StopBenchmark();
                var dialogResult = MessageBox.Show(e.Message, Tr("Error!"),
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
                var dialogResult = MessageBox.Show(Tr("NiceHash Miner Legacy requires internet connection to run. Please ensure that you are connected to the internet before running NiceHash Miner Legacy. Would you like to continue?"),
                    Tr("Check internet connection"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (dialogResult == DialogResult.Yes)
                    return;
                if (dialogResult == DialogResult.No)
                    Application.Exit();
            }
        }

        private void ConnectionEstablishedCallback(object sender, EventArgs e)
        {
            // send credentials
            NiceHashStats.SetCredentials(textBoxBTCAddress.Text.Trim(), textBoxWorkerName.Text.Trim());
        }

        private void VersionUpdateCallback(object sender, EventArgs e)
        {
            var ver = NiceHashStats.Version;
            if (ver == null) return;

            var programVersion = new Version(Application.ProductVersion);
            var onlineVersion = new Version(ver);
            var ret = programVersion.CompareTo(onlineVersion);

            if (ret < 0 || (ret == 0 && BetaAlphaPostfixString != ""))
            {
                SetVersionLabel(string.Format(Tr("IMPORTANT! New version v{0} has\r\nbeen released. Click here to download it."), ver));
                _visitUrlNew = Links.VisitUrlNew + ver;
            }
        }

        private delegate void SetVersionLabelCallback(string text);

        private void SetVersionLabel(string text)
        {
            if (linkLabelNewVersion.InvokeRequired)
            {
                var d = new SetVersionLabelCallback(SetVersionLabel);
                Invoke(d, new object[] {text});
            }
            else
            {
                linkLabelNewVersion.Text = text;
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
            Process.Start(_visitUrlNew);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MinersManager.StopAllMiners();

            MessageBoxManager.Unregister();
        }

        private void ButtonBenchmark_Click(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.ServiceLocation = comboBoxLocation.SelectedIndex;

            _benchmarkForm = new Form_Benchmark();
            SetChildFormCenter(_benchmarkForm);
            _benchmarkForm.ShowDialog();
            var startMining = _benchmarkForm.StartMining;
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
                    Tr("Settings change requires NiceHash Miner Legacy to restart."),
                    Tr("Restart Notice"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                var pHandle = new Process
                {
                    StartInfo =
                    {
                        FileName = Application.ExecutablePath
                    }
                };
                pHandle.Start();
                Close();
            }
            else if (settings.IsChange && settings.IsChangeSaved)
            {
                InitLocalization();
                InitMainConfigGuiData();
            }
        }

        private void ButtonStartMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = true;
            if (StartMining(true) == StartMiningReturnType.ShowNoMining)
            {
                _isManuallyStarted = false;
                StopMining();
                MessageBox.Show(Tr("NiceHash Miner Legacy cannot start mining. Make sure you have at least one enabled device that has at least one enabled and benchmarked algorithm."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void ButtonStopMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = false;
            StopMining();
        }

        private string FormatPayingOutput(double paying)
        {
            string ret;

            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && paying < 0.1)
                ret = (paying * 1000 * _factorTimeUnit).ToString("F5", CultureInfo.InvariantCulture) + " mBTC/" +
                      Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
            else
                ret = (paying * _factorTimeUnit).ToString("F6", CultureInfo.InvariantCulture) + " BTC/" +
                      Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());

            return ret;
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

        private void TextBoxCheckBoxMain_Leave(object sender, EventArgs e)
        {
            if (VerifyMiningAddress(false))
            {
                if (ConfigManager.GeneralConfig.BitcoinAddress != textBoxBTCAddress.Text.Trim()
                    || ConfigManager.GeneralConfig.WorkerName != textBoxWorkerName.Text.Trim())
                {
                    // Reset credentials
                    NiceHashStats.SetCredentials(textBoxBTCAddress.Text.Trim(), textBoxWorkerName.Text.Trim());
                }
                // Commit to config.json
                ConfigManager.GeneralConfig.BitcoinAddress = textBoxBTCAddress.Text.Trim();
                ConfigManager.GeneralConfig.WorkerName = textBoxWorkerName.Text.Trim();
                ConfigManager.GeneralConfig.ServiceLocation = comboBoxLocation.SelectedIndex;
                ConfigManager.GeneralConfigFileCommit();
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

        private StartMiningReturnType StartMining(bool showWarnings)
        {
            if (textBoxBTCAddress.Text.Equals(""))
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


            // Check if there are unbenchmakred algorithms
            var isBenchInit = true;
            foreach (var cdev in AvailableDevices.Devices)
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
                    result = MessageBox.Show(Tr("There are unbenchmarked algorithms for selected enabled devices. Click Yes to benchmark and start mining, No to skip benchmark and continue mining, Cancel to abort"),
                        Tr("Warning!"),
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
                    foreach (var cdev in AvailableDevices.Devices)
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

            textBoxBTCAddress.Enabled = false;
            textBoxWorkerName.Enabled = false;
            comboBoxLocation.Enabled = false;
            buttonBenchmark.Enabled = false;
            buttonStartMining.Enabled = false;
            buttonSettings.Enabled = false;
            devicesListViewEnableControl1.IsMining = true;
            buttonStopMining.Enabled = true;

            // Disable profitable notification on start
            _isNotProfitable = false;

            ConfigManager.GeneralConfig.BitcoinAddress = textBoxBTCAddress.Text.Trim();
            ConfigManager.GeneralConfig.WorkerName = textBoxWorkerName.Text.Trim();
            ConfigManager.GeneralConfig.ServiceLocation = comboBoxLocation.SelectedIndex;

            InitFlowPanelStart();
            ClearRatesAll();

            var btcAdress = _demoMode ? Globals.DemoUser : textBoxBTCAddress.Text.Trim();
            var isMining = MinersManager.StartInitialize(this, Globals.MiningLocation[comboBoxLocation.SelectedIndex],
                textBoxWorkerName.Text.Trim(), btcAdress);

            if (!_demoMode) ConfigManager.GeneralConfigFileCommit();

            //_isSmaUpdated = true; // Always check profits on mining start
            //_smaMinerCheck.Interval = 100;
            //_smaMinerCheck.Start();
            _minerStatsCheck.Start();

            // TODO move this
            if (_cudaChecker == null && ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost)
            {
                _cudaChecker = new CudaDeviceChecker();
                _cudaChecker.Start();
            }

            return isMining ? StartMiningReturnType.StartMining : StartMiningReturnType.ShowNoMining;
        }

        private void StopMining()
        {
            _minerStatsCheck.Stop();
            //_smaMinerCheck.Stop();
            _cudaChecker?.Stop();

            // Disable IFTTT notification before label call
            _isNotProfitable = false;

            MinersManager.StopAllMiners();

            textBoxBTCAddress.Enabled = true;
            textBoxWorkerName.Enabled = true;
            comboBoxLocation.Enabled = true;
            buttonBenchmark.Enabled = true;
            buttonStartMining.Enabled = true;
            buttonSettings.Enabled = true;
            devicesListViewEnableControl1.IsMining = false;
            buttonStopMining.Enabled = false;

            if (_demoMode)
            {
                _demoMode = false;
                labelDemoMode.Visible = false;
            }

            UpdateGlobalRate();
        }

        private void TextBoxBTCAddress_Enter(object sender, EventArgs e)
        {
            //var btc = ConfigManager.GeneralConfig.BitcoinAddress.Trim();
            //if (btc == "")
            //{
            //    var loginForm = new LoginForm();
            //    this.SetChildFormCenter(loginForm);
            //    loginForm.ShowDialog();
            //    if (BitcoinAddress.ValidateBitcoinAddress(loginForm.Btc))
            //    {
            //        ConfigManager.GeneralConfig.BitcoinAddress = loginForm.Btc;
            //        ConfigManager.GeneralConfigFileCommit();
            //        this.textBoxBTCAddress.Text = loginForm.Btc;
            //    }
            //}
        }
    }
}

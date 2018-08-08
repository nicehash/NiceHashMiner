using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Forms;
using NiceHashMiner.Forms.Components;
using NiceHashMiner.Interfaces;
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
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;
using SystemTimer = System.Timers.Timer;
using Timer = System.Windows.Forms.Timer;

namespace NiceHashMiner
{
    using System.IO;

    public partial class Form_Main : Form, Form_Loading.IAfterInitializationCaller, IGlobalRatesUpdate
    {
        private string _visitUrlNew = Links.VisitUrlNew;

        private Timer _minerStatsCheck;
        //private Timer _smaMinerCheck;
        //private Timer _bitcoinExchangeCheck;
        private Timer _startupTimer;
        private Timer _idleCheck;
        private SystemTimer _computeDevicesCheckTimer;

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

        private readonly int _mainFormHeight = 0;
        private readonly int _emtpyGroupPanelHeight = 0;

        public Form_Main()
        {
            InitializeComponent();
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

            R = new Random((int) DateTime.Now.Ticks);

            Text += " v" + Application.ProductVersion + BetaAlphaPostfixString;

            //label_NotProfitable.Visible = false;

            InitMainConfigGuiData();

            //// for resizing
            //InitFlowPanelStart();

            //if (groupBox1.Size.Height > 0 && Size.Height > 0)
            //{
            //    _emtpyGroupPanelHeight = groupBox1.Size.Height;
            //    _mainFormHeight = Size.Height - _emtpyGroupPanelHeight;
            //}
            //else
            //{
            //    _emtpyGroupPanelHeight = 59;
            //    _mainFormHeight = 330 - _emtpyGroupPanelHeight;
            //}
            //ClearRatesAll();
        }

        private void InitLocalization()
        {
            MessageBoxManager.Unregister();
            MessageBoxManager.Yes = International.GetText("Global_Yes");
            MessageBoxManager.No = International.GetText("Global_No");
            MessageBoxManager.OK = International.GetText("Global_OK");
            MessageBoxManager.Cancel = International.GetText("Global_Cancel");
            MessageBoxManager.Retry = International.GetText("Global_Retry");
            MessageBoxManager.Register();

            labelServiceLocation.Text = International.GetText("Service_Location") + ":";
            {
                var i = 0;
                foreach (var loc in Globals.MiningLocation)
                    comboBoxLocation.Items[i++] = International.GetText("LocationName_" + loc);
            }
            labelBitcoinAddress.Text = International.GetText("BitcoinAddress") + ":";
            labelWorkerName.Text = International.GetText("WorkerName") + ":";

            linkLabelCheckStats.Text = International.GetText("Form_Main_check_stats");
            linkLabelChooseBTCWallet.Text = International.GetText("Form_Main_choose_bitcoin_wallet");

            toolStripStatusLabelGlobalRateText.Text = International.GetText("Form_Main_global_rate") + ":";
            toolStripStatusLabelBTCDayText.Text =
                "BTC/" + International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   International.GetText(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   International.GetText("Form_Main_balance") + ":";

            devicesListViewEnableControl1.InitLocale();

            buttonBenchmark.Text = International.GetText("Form_Main_benchmark");
            buttonSettings.Text = International.GetText("Form_Main_settings");
            buttonStartMining.Text = International.GetText("Form_Main_start");
            buttonStopMining.Text = International.GetText("Form_Main_stop");
            buttonHelp.Text = International.GetText("Form_Main_help");

            //label_NotProfitable.Text = International.GetText("Form_Main_MINING_NOT_PROFITABLE");
            //groupBox1.Text = International.GetText("Form_Main_Group_Device_Rates");
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
            TimeFactor.UpdateTimeUnit(ConfigManager.GeneralConfig.TimeUnit);

            toolStripStatusLabelBalanceDollarValue.Text = "(" + ExchangeRateApi.ActiveDisplayCurrency + ")";
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   International.GetText(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   International.GetText("Form_Main_balance") + ":";
            BalanceCallback(null, null); // update currency changes

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
        private void StartupTimer_Tick(object sender, EventArgs e)
        {
            _startupTimer.Stop();
            _startupTimer = null;

            // Internals Init
            // TODO add loading step
            MinersSettingsManager.Init();

            if (!Helpers.Is45NetOrHigher())
            {
                MessageBox.Show(International.GetText("NET45_Not_Installed_msg"),
                    International.GetText("Warning_with_Exclamation"),
                    MessageBoxButtons.OK);

                Close();
                return;
            }

            if (!Helpers.Is64BitOperatingSystem)
            {
                MessageBox.Show(International.GetText("Form_Main_x64_Support_Only"),
                    International.GetText("Warning_with_Exclamation"),
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
            ComputeDeviceManager.Query.QueryDevices(_loadingScreen);
            _isDeviceDetectionInitialized = true;

            /////////////////////////////////////////////
            /////// from here on we have our devices and Miners initialized
            ConfigManager.AfterDeviceQueryInitialization();
            _loadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_SaveConfig"));

            // All devices settup should be initialized in AllDevices
            devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Available.Devices);
            // set properties after
            devicesListViewEnableControl1.SaveToGeneralConfig = true;

            _loadingScreen.IncreaseLoadCounterAndMessage(
                International.GetText("Form_Main_loadtext_CheckLatestVersion"));

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

            _loadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_GetNiceHashSMA"));
            // Init ws connection
            NiceHashStats.OnBalanceUpdate += BalanceCallback;
            NiceHashStats.OnSmaUpdate += SmaCallback;
            NiceHashStats.OnVersionUpdate += VersionUpdateCallback;
            NiceHashStats.OnConnectionLost += ConnectionLostCallback;
            NiceHashStats.OnConnectionEstablished += ConnectionEstablishedCallback;
            NiceHashStats.OnVersionBurn += VersionBurnCallback;
            NiceHashStats.OnExchangeUpdate += ExchangeCallback;
            NiceHashStats.StartConnection(Links.NhmSocketAddress, this);

            // increase timeout
            if (Globals.IsFirstNetworkCheckTimeout)
            {
                while (!Helpers.WebRequestTestGoogle() && Globals.FirstNetworkCheckTimeoutTries > 0)
                {
                    --Globals.FirstNetworkCheckTimeoutTries;
                }
            }

            _loadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_GetBTCRate"));

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
                International.GetText("Form_Main_loadtext_SetEnvironmentVariable"));
            Helpers.SetDefaultEnvironmentVariables();

            _loadingScreen.IncreaseLoadCounterAndMessage(
                International.GetText("Form_Main_loadtext_SetWindowsErrorReporting"));

            Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);

            _loadingScreen.IncreaseLoadCounter();
            if (ConfigManager.GeneralConfig.NVIDIAP0State)
            {
                _loadingScreen.SetInfoMsg(International.GetText("Form_Main_loadtext_NVIDIAP0State"));
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
                    var result = MessageBox.Show(International.GetText("Form_Main_bins_folder_files_missing"),
                        International.GetText("Warning_with_Exclamation"),
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
                        var result = MessageBox.Show(International.GetText("Form_Main_bins_folder_files_missing"),
                            International.GetText("Warning_with_Exclamation"),
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
                International.GetText("Form_Loading_label_LoadingText"),
                International.GetText("Form_Main_loadtext_CPU"), totalLoadSteps);
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
            ClearRates(-1);
        }

        public void ClearRates(int groupCount)
        {
            //if (InvokeRequired)
            //{
            //    Invoke((Action) delegate { ClearRates(groupCount); });
            //    return;
            //}
            //if (_flowLayoutPanelVisibleCount != groupCount)
            //{
            //    _flowLayoutPanelVisibleCount = groupCount;
            //    // hide some Controls
            //    var hideIndex = 0;
            //    foreach (var control in flowLayoutPanelRates.Controls)
            //    {
            //        ((GroupProfitControl) control).Visible = hideIndex < groupCount;
            //        ++hideIndex;
            //    }
            //}
            //_flowLayoutPanelRatesIndex = 0;
            //var visibleGroupCount = 1;
            //if (groupCount > 0) visibleGroupCount += groupCount;

            //var groupBox1Height = _emtpyGroupPanelHeight;
            //if (flowLayoutPanelRates.Controls.Count > 0)
            //{
            //    var control = flowLayoutPanelRates.Controls[0];
            //    var panelHeight = ((GroupProfitControl) control).Size.Height * 1.2f;
            //    groupBox1Height = (int) ((visibleGroupCount) * panelHeight);
            //}

            //groupBox1.Size = new Size(groupBox1.Size.Width, groupBox1Height);
            //// set new height
            //Size = new Size(Size.Width, _mainFormHeight + groupBox1Height);
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
                                     International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());

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

        public void UpdateGlobalRate()
        {
            var totalRate = MinersManager.GetTotalRate();

            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && totalRate < 0.1)
            {
                toolStripStatusLabelBTCDayText.Text =
                    "mBTC/" + International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * 1000 * TimeFactor.TimeUnit).ToString("F5", CultureInfo.InvariantCulture);
            }
            else
            {
                toolStripStatusLabelBTCDayText.Text =
                    "BTC/" + International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * TimeFactor.TimeUnit).ToString("F6", CultureInfo.InvariantCulture);
            }

            toolStripStatusLabelBTCDayValue.Text = ExchangeRateApi
                .ConvertToActiveCurrency((totalRate * TimeFactor.TimeUnit * ExchangeRateApi.GetUsdExchangeRate()))
                .ToString("F2", CultureInfo.InvariantCulture);
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   International.GetText(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   International.GetText("Form_Main_balance") + ":";
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
            var currencyRate = International.GetText("BenchmarkRatioRateN_A");
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
                var dialogResult = MessageBox.Show(e.Message, International.GetText("Error_with_Exclamation"),
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
                var dialogResult = MessageBox.Show(International.GetText("Form_Main_msgbox_NoInternetMsg"),
                    International.GetText("Form_Main_msgbox_NoInternetTitle"),
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
            // NiceHashStats.SetCredentials(textBoxBTCAddress.Text.Trim(), textBoxWorkerName.Text.Trim());
        }

        private void VersionUpdateCallback(object sender, EventArgs e)
        {
            var ver = NiceHashStats.Version;
            if (ver == null) return;

            var programVersion = new Version(Application.ProductVersion);
            var onlineVersion = new Version(ver);
            var ret = programVersion.CompareTo(onlineVersion);

            var link = NiceHashStats.VersionLink;
            if (string.IsNullOrWhiteSpace(link)) return;

            if (ret < 0 || (ret == 0 && BetaAlphaPostfixString != ""))
            {
                SetVersionLabel(string.Format(International.GetText("Form_Main_new_version_released"), ver));
                _visitUrlNew = link;
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
                var result = MessageBox.Show(International.GetText("Form_Main_msgbox_InvalidBTCAddressMsg"),
                    International.GetText("Error_with_Exclamation"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                    Process.Start(Links.NhmBtcWalletFaq);

                textBoxBTCAddress.Focus();
                return false;
            }
            if (!BitcoinAddress.ValidateWorkerName(textBoxWorkerName.Text.Trim()) && showError)
            {
                var result = MessageBox.Show(International.GetText("Form_Main_msgbox_InvalidWorkerNameMsg"),
                    International.GetText("Error_with_Exclamation"),
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
                    International.GetText("Form_Main_Restart_Required_Msg"),
                    International.GetText("Form_Main_Restart_Required_Title"),
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
                MessageBox.Show(International.GetText("Form_Main_StartMiningReturnedFalse"),
                    International.GetText("Warning_with_Exclamation"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void ButtonStopMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = false;
            StopMining();
        }

        //private string FormatPayingOutput(double paying)
        //{
        //    string ret;

        //    if (ConfigManager.GeneralConfig.AutoScaleBTCValues && paying < 0.1)
        //        ret = (paying * 1000 * _factorTimeUnit).ToString("F5", CultureInfo.InvariantCulture) + " mBTC/" +
        //              International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
        //    else
        //        ret = (paying * _factorTimeUnit).ToString("F6", CultureInfo.InvariantCulture) + " BTC/" +
        //              International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());

        //    return ret;
        //}


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
                    var result = MessageBox.Show(International.GetText("Form_Main_DemoModeMsg"),
                        International.GetText("Form_Main_DemoModeTitle"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        _demoMode = true;
                        labelDemoMode.Visible = true;
                        labelDemoMode.Text = International.GetText("Form_Main_DemoModeLabel");
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
                    MessageBox.Show(International.GetText("Form_Main_msgbox_NullNiceHashDataMsg"),
                        International.GetText("Error_with_Exclamation"),
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
                    result = MessageBox.Show(International.GetText("EnabledUnbenchmarkedAlgorithmsWarning"),
                        International.GetText("Warning_with_Exclamation"),
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


            ConfigManager.GeneralConfig.BitcoinAddress = textBoxBTCAddress.Text.Trim();
            ConfigManager.GeneralConfig.WorkerName = textBoxWorkerName.Text.Trim();
            ConfigManager.GeneralConfig.ServiceLocation = comboBoxLocation.SelectedIndex;

            var btcAdress = _demoMode ? Globals.DemoUser : textBoxBTCAddress.Text.Trim();
            var isMining = MinersManager.StartInitialize(this, Globals.MiningLocation[comboBoxLocation.SelectedIndex],
                textBoxWorkerName.Text.Trim(), btcAdress);

            StartMiningGui();

            if (!_demoMode) ConfigManager.GeneralConfigFileCommit();

            //_isSmaUpdated = true; // Always check profits on mining start
            //_smaMinerCheck.Interval = 100;
            //_smaMinerCheck.Start();

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
                devicesListViewEnableControl1.IsMining = true;
                buttonStopMining.Enabled = true;

                // Disable profitable notification on start
                _isNotProfitable = false;

                InitFlowPanelStart();
                ClearRatesAll();

                _minerStatsCheck.Start();

                if (!ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost) return;
                _computeDevicesCheckTimer = new SystemTimer();
                _computeDevicesCheckTimer.Elapsed += ComputeDevicesCheckTimer_Tick;
                _computeDevicesCheckTimer.Interval = 60000;

                _computeDevicesCheckTimer.Start();
            }
        }

        private void StopMining()
        {
            MinersManager.StopAllMiners();
            StopMiningGui();
        }

        public void StopMiningGui()
        {
            if (InvokeRequired)
            {
                Invoke((Action) StopMiningGui);
            }
            else
            {
                _minerStatsCheck.Stop();
                _computeDevicesCheckTimer?.Stop();

                // Disable IFTTT notification before label call
                _isNotProfitable = false;
                
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
        }

        private void Form_Main_ResizeEnd(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.MainFormSize.X = Width;
            ConfigManager.GeneralConfig.MainFormSize.Y = Height;
        }
    }
}

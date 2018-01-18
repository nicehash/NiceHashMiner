using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Enums;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    using Miners.Grouping;

    public partial class Form_Benchmark : Form, IListItemCheckColorSetter, IBenchmarkComunicator, IBenchmarkCalculation
    {
        private bool _inBenchmark = false;
        private int _bechmarkCurrentIndex = 0;
        private int _bechmarkedSuccessCount = 0;
        private int _benchmarkAlgorithmsCount = 0;

        private AlgorithmBenchmarkSettingsType _algorithmOption =
            AlgorithmBenchmarkSettingsType.SelectedUnbenchmarkedAlgorithms;

        private readonly List<Miner> _benchmarkMiners;
        private Miner _currentMiner;
        private List<Tuple<ComputeDevice, Queue<Algorithm>>> _benchmarkDevicesAlgorithmQueue;

        private readonly bool _exitWhenFinished = false;
        //private AlgorithmType _singleBenchmarkType = AlgorithmType.NONE;

        private readonly Timer _benchmarkingTimer;
        private int _dotCount = 0;

        public bool StartMining { get; private set; }

        private struct DeviceAlgo
        {
            public string Device { get; set; }
            public string Algorithm { get; set; }
        }

        private List<DeviceAlgo> _benchmarkFailedAlgoPerDev;

        private enum BenchmarkSettingsStatus : int
        {
            NONE = 0,
            TODO,
            DISABLED_NONE,
            DISABLED_TODO
        }

        private Dictionary<string, BenchmarkSettingsStatus> _benchmarkDevicesAlgorithmStatus;
        private ComputeDevice _currentDevice;
        private Algorithm _currentAlgorithm;

        private string _currentAlgoName;

        // CPU benchmarking helpers
        private class CpuBenchmarkStatus
        {
            private class benchmark
            {
                public benchmark(int lt, double bench)
                {
                    LessTreads = lt;
                    Benchmark = bench;
                }

                public readonly int LessTreads;
                public readonly double Benchmark;
            }

            public CpuBenchmarkStatus(int maxThreads)
            {
                _maxThreads = maxThreads;
            }

            public bool HasTest()
            {
                return LessTreads < _maxThreads;
            }

            public void SetNextSpeed(double speed)
            {
                if (HasTest())
                {
                    _benchmarks.Add(new benchmark(LessTreads, speed));
                    ++LessTreads;
                }
            }

            public void FindFastest()
            {
                _benchmarks.Sort((a, b) => -a.Benchmark.CompareTo(b.Benchmark));
            }

            public double GetBestSpeed()
            {
                return _benchmarks[0].Benchmark;
            }

            public int GetLessThreads()
            {
                return _benchmarks[0].LessTreads;
            }

            private readonly int _maxThreads;
            private readonly List<benchmark> _benchmarks = new List<benchmark>();
            public int LessTreads { get; private set; } = 0;
            public int Time;
        }

        private CpuBenchmarkStatus _cpuBenchmarkStatus = null;

        private class ClaymoreZcashStatus
        {
            private const int MaxBench = 2;
            private readonly string[] _asmModes = {" -asm 1", " -asm 0"};

            private readonly double[] _speeds = {0.0d, 0.0d};
            private int _curIndex = 0;
            private readonly string _originalExtraParams;

            public ClaymoreZcashStatus(string oep)
            {
                _originalExtraParams = oep;
            }

            public bool HasTest()
            {
                return _curIndex < MaxBench;
            }

            public void SetSpeed(double speed)
            {
                if (HasTest())
                {
                    _speeds[_curIndex] = speed;
                }
            }

            public void SetNext()
            {
                _curIndex += 1;
            }

            public string GetTestExtraParams()
            {
                if (HasTest())
                {
                    return _originalExtraParams + _asmModes[_curIndex];
                }
                return _originalExtraParams;
            }

            private int FastestIndex()
            {
                var maxIndex = 0;
                var maxValue = _speeds[maxIndex];
                for (var i = 1; i < _speeds.Length; ++i)
                {
                    if (_speeds[i] > maxValue)
                    {
                        maxIndex = i;
                        maxValue = _speeds[i];
                    }
                }

                return 0;
            }

            public string GetFastestExtraParams()
            {
                return _originalExtraParams + _asmModes[FastestIndex()];
            }

            public double GetFastestTime()
            {
                return _speeds[FastestIndex()];
            }

            public int Time = 180;
        }

        private ClaymoreZcashStatus _claymoreZcashStatus = null;

        // CPU sweet spots
        private readonly List<AlgorithmType> _cpuAlgos = new List<AlgorithmType>()
        {
            AlgorithmType.CryptoNight
        };

        private static readonly Color DisabledColor = Color.DarkGray;
        private static readonly Color BenchmarkedColor = Color.LightGreen;
        private static readonly Color UnbenchmarkedColor = Color.LightBlue;

        public void LviSetColor(ListViewItem lvi)
        {
            if (lvi.Tag is ComputeDevice cDevice && _benchmarkDevicesAlgorithmStatus != null)
            {
                var uuid = cDevice.Uuid;
                if (!cDevice.Enabled)
                {
                    lvi.BackColor = DisabledColor;
                }
                else
                {
                    switch (_benchmarkDevicesAlgorithmStatus[uuid])
                    {
                        case BenchmarkSettingsStatus.TODO:
                        case BenchmarkSettingsStatus.DISABLED_TODO:
                            lvi.BackColor = UnbenchmarkedColor;
                            break;
                        case BenchmarkSettingsStatus.NONE:
                        case BenchmarkSettingsStatus.DISABLED_NONE:
                            lvi.BackColor = BenchmarkedColor;
                            break;
                    }
                }
                //// enable disable status, NOT needed
                //if (cdvo.IsEnabled && _benchmarkDevicesAlgorithmStatus[uuid] >= BenchmarkSettingsStatus.DISABLED_NONE) {
                //    _benchmarkDevicesAlgorithmStatus[uuid] -= 2;
                //} else if (!cdvo.IsEnabled && _benchmarkDevicesAlgorithmStatus[uuid] <= BenchmarkSettingsStatus.TODO) {
                //    _benchmarkDevicesAlgorithmStatus[uuid] += 2;
                //}
            }
        }

        public Form_Benchmark(BenchmarkPerformanceType benchmarkPerformanceType = BenchmarkPerformanceType.Standard,
            bool autostart = false)
        {
            InitializeComponent();
            Icon = Properties.Resources.logo;

            StartMining = false;

            // clear prev pending statuses
            foreach (var dev in ComputeDeviceManager.Avaliable.AllAvaliableDevices)
            {
                foreach (var algo in dev.GetAlgorithmSettings())
                {
                    algo.ClearBenchmarkPendingFirst();
                }
            }

            benchmarkOptions1.SetPerformanceType(benchmarkPerformanceType);

            // benchmark only unique devices
            devicesListViewEnableControl1.SetIListItemCheckColorSetter(this);
            devicesListViewEnableControl1.SetComputeDevices(ComputeDeviceManager.Avaliable.AllAvaliableDevices);

            // use this to track miner benchmark statuses
            _benchmarkMiners = new List<Miner>();

            InitLocale();

            _benchmarkingTimer = new Timer();
            _benchmarkingTimer.Tick += BenchmarkingTimer_Tick;
            _benchmarkingTimer.Interval = 1000; // 1s

            //// name, UUID
            //Dictionary<string, string> benchNamesUUIDs = new Dictionary<string, string>();
            //// initialize benchmark settings for same cards to only copy settings
            //foreach (var cDev in ComputeDeviceManager.Avaliable.AllAvaliableDevices) {
            //    var plainDevName = cDev.Name;
            //    if (benchNamesUUIDs.ContainsKey(plainDevName)) {
            //        cDev.Enabled = false;
            //        cDev.BenchmarkCopyUUID = benchNamesUUIDs[plainDevName];
            //    } else if (cDev.Enabled == true) {
            //        benchNamesUUIDs.Add(plainDevName, cDev.UUID);
            //        //cDev.Enabled = true; // enable benchmark
            //        cDev.BenchmarkCopyUUID = null;
            //    }
            //}

            //groupBoxAlgorithmBenchmarkSettings.Enabled = _singleBenchmarkType == AlgorithmType.NONE;
            devicesListViewEnableControl1.Enabled = true;
            devicesListViewEnableControl1.SetDeviceSelectionChangedCallback(DevicesListView1_ItemSelectionChanged);

            devicesListViewEnableControl1.SetAlgorithmsListView(algorithmsListView1);
            devicesListViewEnableControl1.IsBenchmarkForm = true;
            devicesListViewEnableControl1.IsSettingsCopyEnabled = true;

            ResetBenchmarkProgressStatus();
            CalcBenchmarkDevicesAlgorithmQueue();
            devicesListViewEnableControl1.ResetListItemColors();

            // to update laclulation status
            devicesListViewEnableControl1.BenchmarkCalculation = this;
            algorithmsListView1.BenchmarkCalculation = this;

            // set first device selected {
            if (ComputeDeviceManager.Avaliable.AllAvaliableDevices.Count > 0)
            {
                var firstComputedevice = ComputeDeviceManager.Avaliable.AllAvaliableDevices[0];
                algorithmsListView1.SetAlgorithms(firstComputedevice, firstComputedevice.Enabled);
            }

            if (autostart)
            {
                _exitWhenFinished = true;
                StartStopBtn_Click(null, null);
            }
        }

        private void CopyBenchmarks()
        {
            Helpers.ConsolePrint("CopyBenchmarks", "Checking for benchmarks to copy");
            foreach (var cDev in ComputeDeviceManager.Avaliable.AllAvaliableDevices)
            {
                // check if copy
                if (!cDev.Enabled && cDev.BenchmarkCopyUuid != null)
                {
                    var copyCdevSettings = ComputeDeviceManager.Avaliable.GetDeviceWithUuid(cDev.BenchmarkCopyUuid);
                    if (copyCdevSettings != null)
                    {
                        Helpers.ConsolePrint("CopyBenchmarks", $"Copy from {cDev.Uuid} to {cDev.BenchmarkCopyUuid}");
                        cDev.CopyBenchmarkSettingsFrom(copyCdevSettings);
                    }
                }
            }
        }

        private void BenchmarkingTimer_Tick(object sender, EventArgs e)
        {
            if (_inBenchmark)
            {
                algorithmsListView1.SetSpeedStatus(_currentDevice, _currentAlgorithm, GetDotsWaitString());
            }
        }

        private string GetDotsWaitString()
        {
            ++_dotCount;
            if (_dotCount > 3) _dotCount = 1;
            return new string('.', _dotCount);
        }

        private void InitLocale()
        {
            Text = International.GetText("Form_Benchmark_title"); //International.GetText("SubmitResultDialog_title");
            //labelInstruction.Text = International.GetText("SubmitResultDialog_labelInstruction");
            StartStopBtn.Text = International.GetText("SubmitResultDialog_StartBtn");
            CloseBtn.Text = International.GetText("SubmitResultDialog_CloseBtn");

            // TODO fix locale for benchmark enabled label
            devicesListViewEnableControl1.InitLocale();
            benchmarkOptions1.InitLocale();
            algorithmsListView1.InitLocale();
            groupBoxBenchmarkProgress.Text = International.GetText("FormBenchmark_Benchmark_GroupBoxStatus");
            radioButton_SelectedUnbenchmarked.Text =
                International.GetText("FormBenchmark_Benchmark_All_Selected_Unbenchmarked");
            radioButton_RE_SelectedUnbenchmarked.Text =
                International.GetText("FormBenchmark_Benchmark_All_Selected_ReUnbenchmarked");
            checkBox_StartMiningAfterBenchmark.Text =
                International.GetText("Form_Benchmark_checkbox_StartMiningAfterBenchmark");
        }

        private void StartStopBtn_Click(object sender, EventArgs e)
        {
            if (_inBenchmark)
            {
                StopButonClick();
                BenchmarkStoppedGuiSettings();
            }
            else if (StartButonClick())
            {
                StartStopBtn.Text = International.GetText("Form_Benchmark_buttonStopBenchmark");
            }
        }

        public void StopBenchmark()
        {
            if (_inBenchmark)
            {
                StopButonClick();
                BenchmarkStoppedGuiSettings();
            }
        }

        private void BenchmarkStoppedGuiSettings()
        {
            StartStopBtn.Text = International.GetText("Form_Benchmark_buttonStartBenchmark");
            // clear benchmark pending status
            _currentAlgorithm?.ClearBenchmarkPending();
            foreach (var deviceAlgosTuple in _benchmarkDevicesAlgorithmQueue)
            {
                foreach (var algo in deviceAlgosTuple.Item2)
                {
                    algo.ClearBenchmarkPending();
                }
            }
            ResetBenchmarkProgressStatus();
            CalcBenchmarkDevicesAlgorithmQueue();
            benchmarkOptions1.Enabled = true;

            algorithmsListView1.IsInBenchmark = false;
            devicesListViewEnableControl1.IsInBenchmark = false;
            if (_currentDevice != null)
            {
                algorithmsListView1.RepaintStatus(_currentDevice.Enabled, _currentDevice.Uuid);
            }

            CloseBtn.Enabled = true;
        }

        // TODO add list for safety and kill all miners
        private void StopButonClick()
        {
            _benchmarkingTimer.Stop();
            _inBenchmark = false;
            Helpers.ConsolePrint("FormBenchmark", "StopButonClick() benchmark routine stopped");
            //// copy benchmarked
            //CopyBenchmarks();
            if (_currentMiner != null)
            {
                _currentMiner.BenchmarkSignalQuit = true;
                _currentMiner.InvokeBenchmarkSignalQuit();
            }
            if (_exitWhenFinished)
            {
                Close();
            }
        }

        private bool StartButonClick()
        {
            CalcBenchmarkDevicesAlgorithmQueue();
            // device selection check scope
            {
                var noneSelected = ComputeDeviceManager.Avaliable.AllAvaliableDevices.All(cDev => !cDev.Enabled);
                if (noneSelected)
                {
                    MessageBox.Show(International.GetText("FormBenchmark_No_Devices_Selected_Msg"),
                        International.GetText("FormBenchmark_No_Devices_Selected_Title"),
                        MessageBoxButtons.OK);
                    return false;
                }
            }
            // device todo benchmark check scope
            {
                var nothingToBench =
                    _benchmarkDevicesAlgorithmStatus.All(statusKpv => statusKpv.Value != BenchmarkSettingsStatus.TODO);
                if (nothingToBench)
                {
                    MessageBox.Show(International.GetText("FormBenchmark_Nothing_to_Benchmark_Msg"),
                        International.GetText("FormBenchmark_Nothing_to_Benchmark_Title"),
                        MessageBoxButtons.OK);
                    return false;
                }
            }

            // current failed new list
            _benchmarkFailedAlgoPerDev = new List<DeviceAlgo>();
            // disable gui controls
            benchmarkOptions1.Enabled = false;
            CloseBtn.Enabled = false;
            algorithmsListView1.IsInBenchmark = true;
            devicesListViewEnableControl1.IsInBenchmark = true;
            // set benchmark pending status
            foreach (var deviceAlgosTuple in _benchmarkDevicesAlgorithmQueue)
            {
                foreach (var algo in deviceAlgosTuple.Item2)
                {
                    algo.SetBenchmarkPending();
                }
            }
            if (_currentDevice != null)
            {
                algorithmsListView1.RepaintStatus(_currentDevice.Enabled, _currentDevice.Uuid);
            }

            StartBenchmark();

            return true;
        }

        public void CalcBenchmarkDevicesAlgorithmQueue()
        {
            _benchmarkAlgorithmsCount = 0;
            _benchmarkDevicesAlgorithmStatus = new Dictionary<string, BenchmarkSettingsStatus>();
            _benchmarkDevicesAlgorithmQueue = new List<Tuple<ComputeDevice, Queue<Algorithm>>>();
            foreach (var cDev in ComputeDeviceManager.Avaliable.AllAvaliableDevices)
            {
                var algorithmQueue = new Queue<Algorithm>();
                foreach (var algo in cDev.GetAlgorithmSettings())
                {
                    if (ShoulBenchmark(algo))
                    {
                        algorithmQueue.Enqueue(algo);
                        algo.SetBenchmarkPendingNoMsg();
                    }
                    else
                    {
                        algo.ClearBenchmarkPending();
                    }
                }


                BenchmarkSettingsStatus status;
                if (cDev.Enabled)
                {
                    _benchmarkAlgorithmsCount += algorithmQueue.Count;
                    status = algorithmQueue.Count == 0 ? BenchmarkSettingsStatus.NONE : BenchmarkSettingsStatus.TODO;
                    _benchmarkDevicesAlgorithmQueue.Add(
                        new Tuple<ComputeDevice, Queue<Algorithm>>(cDev, algorithmQueue)
                    );
                }
                else
                {
                    status = algorithmQueue.Count == 0
                        ? BenchmarkSettingsStatus.DISABLED_NONE
                        : BenchmarkSettingsStatus.DISABLED_TODO;
                }
                _benchmarkDevicesAlgorithmStatus[cDev.Uuid] = status;
            }
            // GUI stuff
            progressBarBenchmarkSteps.Maximum = _benchmarkAlgorithmsCount;
            progressBarBenchmarkSteps.Value = 0;
            SetLabelBenchmarkSteps(0, _benchmarkAlgorithmsCount);
        }

        private bool ShoulBenchmark(Algorithm algorithm)
        {
            var isBenchmarked = algorithm.BenchmarkSpeed > 0;
            switch (_algorithmOption)
            {
                case AlgorithmBenchmarkSettingsType.SelectedUnbenchmarkedAlgorithms when !isBenchmarked &&
                                                                                         algorithm.Enabled:
                    return true;
                case AlgorithmBenchmarkSettingsType.UnbenchmarkedAlgorithms when !isBenchmarked:
                    return true;
                case AlgorithmBenchmarkSettingsType.ReBecnhSelectedAlgorithms when algorithm.Enabled:
                    return true;
                case AlgorithmBenchmarkSettingsType.AllAlgorithms:
                    return true;
            }

            return false;
        }

        private void StartBenchmark()
        {
            _inBenchmark = true;
            _bechmarkCurrentIndex = -1;
            NextBenchmark();
        }

        private void NextBenchmark()
        {
            if (_bechmarkCurrentIndex > -1)
            {
                StepUpBenchmarkStepProgress();
            }
            ++_bechmarkCurrentIndex;
            if (_bechmarkCurrentIndex >= _benchmarkAlgorithmsCount)
            {
                EndBenchmark();
                return;
            }

            while (_benchmarkDevicesAlgorithmQueue.Count > 0)
            {
                var currentDeviceAlgosTuple = _benchmarkDevicesAlgorithmQueue[0];
                _currentDevice = currentDeviceAlgosTuple.Item1;
                var algorithmBenchmarkQueue = currentDeviceAlgosTuple.Item2;
                if (algorithmBenchmarkQueue.Count != 0)
                {
                    _currentAlgorithm = algorithmBenchmarkQueue.Dequeue();
                    break;
                }
                _benchmarkDevicesAlgorithmQueue.RemoveAt(0);
            }

            if (_currentDevice != null && _currentAlgorithm != null)
            {
                _currentMiner = MinerFactory.CreateMiner(_currentDevice, _currentAlgorithm);
                if (_currentAlgorithm.MinerBaseType == MinerBaseType.XmrStackCPU &&
                    _currentAlgorithm.NiceHashID == AlgorithmType.CryptoNight &&
                    string.IsNullOrEmpty(_currentAlgorithm.ExtraLaunchParameters) &&
                    _currentAlgorithm.ExtraLaunchParameters.Contains("enable_ht=true") == false)
                {
                    _cpuBenchmarkStatus = new CpuBenchmarkStatus(Globals.ThreadsPerCpu);
                    _currentAlgorithm.LessThreads = _cpuBenchmarkStatus.LessTreads;
                }
                else
                {
                    _cpuBenchmarkStatus = null;
                }
                if (_currentAlgorithm.MinerBaseType == MinerBaseType.Claymore &&
                    _currentAlgorithm.NiceHashID == AlgorithmType.Equihash &&
                    _currentAlgorithm.ExtraLaunchParameters != null &&
                    !_currentAlgorithm.ExtraLaunchParameters.Contains("-asm"))
                {
                    _claymoreZcashStatus = new ClaymoreZcashStatus(_currentAlgorithm.ExtraLaunchParameters);
                    _currentAlgorithm.ExtraLaunchParameters = _claymoreZcashStatus.GetTestExtraParams();
                }
                else
                {
                    _claymoreZcashStatus = null;
                }
            }

            if (_currentMiner != null && _currentAlgorithm != null)
            {
                _benchmarkMiners.Add(_currentMiner);
                _currentAlgoName = AlgorithmNiceHashNames.GetName(_currentAlgorithm.NiceHashID);
                _currentMiner.InitBenchmarkSetup(new MiningPair(_currentDevice, _currentAlgorithm));

                if (_currentDevice != null)
                {
                    var time = ConfigManager.GeneralConfig.BenchmarkTimeLimits
                        .GetBenchamrktime(benchmarkOptions1.PerformanceType, _currentDevice.DeviceGroupType);
                    //currentConfig.TimeLimit = time;
                    if (_cpuBenchmarkStatus != null)
                    {
                        _cpuBenchmarkStatus.Time = time;
                    }
                    if (_claymoreZcashStatus != null)
                    {
                        _claymoreZcashStatus.Time = time;
                    }

                    // dagger about 4 minutes
                    var showWaitTime = _currentAlgorithm.NiceHashID == AlgorithmType.DaggerHashimoto ? 4 * 60 : time;

                    _dotCount = 0;
                    _benchmarkingTimer.Start();

                    _currentMiner.BenchmarkStart(time, this);
                }
                algorithmsListView1.SetSpeedStatus(_currentDevice, _currentAlgorithm,
                    GetDotsWaitString());
            }
            else
            {
                NextBenchmark();
            }
        }

        private void EndBenchmark()
        {
            _benchmarkingTimer.Stop();
            _inBenchmark = false;
            Helpers.ConsolePrint("FormBenchmark", "EndBenchmark() benchmark routine finished");

            //CopyBenchmarks();

            BenchmarkStoppedGuiSettings();
            // check if all ok
            if (_benchmarkFailedAlgoPerDev.Count == 0 && StartMining == false)
            {
                MessageBox.Show(
                    International.GetText("FormBenchmark_Benchmark_Finish_Succes_MsgBox_Msg"),
                    International.GetText("FormBenchmark_Benchmark_Finish_MsgBox_Title"),
                    MessageBoxButtons.OK);
            }
            else if (StartMining == false)
            {
                var result = MessageBox.Show(
                    International.GetText("FormBenchmark_Benchmark_Finish_Fail_MsgBox_Msg"),
                    International.GetText("FormBenchmark_Benchmark_Finish_MsgBox_Title"),
                    MessageBoxButtons.RetryCancel);

                if (result == DialogResult.Retry)
                {
                    StartButonClick();
                    return;
                }
                // get unbenchmarked from criteria and disable
                CalcBenchmarkDevicesAlgorithmQueue();
                foreach (var deviceAlgoQueue in _benchmarkDevicesAlgorithmQueue)
                {
                    foreach (var algorithm in deviceAlgoQueue.Item2)
                    {
                        algorithm.Enabled = false;
                    }
                }
            }
            if (_exitWhenFinished || StartMining)
            {
                Close();
            }
        }


        public void SetCurrentStatus(string status)
        {
            Invoke((MethodInvoker) delegate
            {
                algorithmsListView1.SetSpeedStatus(_currentDevice, _currentAlgorithm, GetDotsWaitString());
            });
        }

        public void OnBenchmarkComplete(bool success, string status)
        {
            if (!_inBenchmark) return;
            Invoke((MethodInvoker) delegate
            {
                _bechmarkedSuccessCount += success ? 1 : 0;
                var rebenchSame = false;
                if (success && _cpuBenchmarkStatus != null && _cpuAlgos.Contains(_currentAlgorithm.NiceHashID) &&
                    _currentAlgorithm.MinerBaseType == MinerBaseType.XmrStackCPU)
                {
                    _cpuBenchmarkStatus.SetNextSpeed(_currentAlgorithm.BenchmarkSpeed);
                    rebenchSame = _cpuBenchmarkStatus.HasTest();
                    _currentAlgorithm.LessThreads = _cpuBenchmarkStatus.LessTreads;
                    if (rebenchSame == false)
                    {
                        _cpuBenchmarkStatus.FindFastest();
                        _currentAlgorithm.BenchmarkSpeed = _cpuBenchmarkStatus.GetBestSpeed();
                        _currentAlgorithm.LessThreads = _cpuBenchmarkStatus.GetLessThreads();
                    }
                }

                if (_claymoreZcashStatus != null && _currentAlgorithm.MinerBaseType == MinerBaseType.Claymore &&
                    _currentAlgorithm.NiceHashID == AlgorithmType.Equihash)
                {
                    if (_claymoreZcashStatus.HasTest())
                    {
                        _currentMiner = MinerFactory.CreateMiner(_currentDevice, _currentAlgorithm);
                        rebenchSame = true;
                        //System.Threading.Thread.Sleep(1000*60*5);
                        _claymoreZcashStatus.SetSpeed(_currentAlgorithm.BenchmarkSpeed);
                        _claymoreZcashStatus.SetNext();
                        _currentAlgorithm.ExtraLaunchParameters = _claymoreZcashStatus.GetTestExtraParams();
                        Helpers.ConsolePrint("ClaymoreAMD_Equihash", _currentAlgorithm.ExtraLaunchParameters);
                        _currentMiner.InitBenchmarkSetup(new MiningPair(_currentDevice, _currentAlgorithm));
                    }

                    if (_claymoreZcashStatus.HasTest() == false)
                    {
                        rebenchSame = false;
                        // set fastest mode
                        _currentAlgorithm.BenchmarkSpeed = _claymoreZcashStatus.GetFastestTime();
                        _currentAlgorithm.ExtraLaunchParameters = _claymoreZcashStatus.GetFastestExtraParams();
                    }
                }

                if (!rebenchSame)
                {
                    _benchmarkingTimer.Stop();
                }

                if (!success && !rebenchSame)
                {
                    // add new failed list
                    _benchmarkFailedAlgoPerDev.Add(
                        new DeviceAlgo()
                        {
                            Device = _currentDevice.Name,
                            Algorithm = _currentAlgorithm.AlgorithmName
                        });
                    algorithmsListView1.SetSpeedStatus(_currentDevice, _currentAlgorithm, status);
                }
                else if (!rebenchSame)
                {
                    // set status to empty string it will return speed
                    _currentAlgorithm.ClearBenchmarkPending();
                    algorithmsListView1.SetSpeedStatus(_currentDevice, _currentAlgorithm, "");
                }
                if (rebenchSame)
                {
                    if (_cpuBenchmarkStatus != null)
                    {
                        _currentMiner.BenchmarkStart(_cpuBenchmarkStatus.Time, this);
                    }
                    else if (_claymoreZcashStatus != null)
                    {
                        _currentMiner.BenchmarkStart(_claymoreZcashStatus.Time, this);
                    }
                }
                else
                {
                    NextBenchmark();
                }
            });
        }

        #region Benchmark progress GUI stuff

        private void SetLabelBenchmarkSteps(int current, int max)
        {
            labelBenchmarkSteps.Text =
                string.Format(International.GetText("FormBenchmark_Benchmark_Step"), current, max);
        }

        private void StepUpBenchmarkStepProgress()
        {
            SetLabelBenchmarkSteps(_bechmarkCurrentIndex + 1, _benchmarkAlgorithmsCount);
            progressBarBenchmarkSteps.Value = _bechmarkCurrentIndex + 1;
        }

        private void ResetBenchmarkProgressStatus()
        {
            progressBarBenchmarkSteps.Value = 0;
        }

        #endregion // Benchmark progress GUI stuff

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormBenchmark_New_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_inBenchmark)
            {
                e.Cancel = true;
                return;
            }

            // disable all pending benchmark
            foreach (var cDev in ComputeDeviceManager.Avaliable.AllAvaliableDevices)
            {
                foreach (var algorithm in cDev.GetAlgorithmSettings())
                {
                    algorithm.ClearBenchmarkPending();
                }
            }

            // save already benchmarked algorithms
            ConfigManager.CommitBenchmarks();
            // check devices without benchmarks
            foreach (var cdev in ComputeDeviceManager.Avaliable.AllAvaliableDevices)
            {
                if (cdev.Enabled)
                {
                    var enabled = cdev.GetAlgorithmSettings().Any(algo => algo.BenchmarkSpeed > 0);
                    cdev.Enabled = enabled;
                }
            }
        }

        private void DevicesListView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            //algorithmSettingsControl1.Deselect();
            // show algorithms
            var selectedComputeDevice =
                ComputeDeviceManager.Avaliable.GetCurrentlySelectedComputeDevice(e.ItemIndex, true);
            algorithmsListView1.SetAlgorithms(selectedComputeDevice, selectedComputeDevice.Enabled);
        }

        private void RadioButton_SelectedUnbenchmarked_CheckedChanged_1(object sender, EventArgs e)
        {
            _algorithmOption = AlgorithmBenchmarkSettingsType.SelectedUnbenchmarkedAlgorithms;
            CalcBenchmarkDevicesAlgorithmQueue();
            devicesListViewEnableControl1.ResetListItemColors();
            algorithmsListView1.ResetListItemColors();
        }

        private void RadioButton_RE_SelectedUnbenchmarked_CheckedChanged(object sender, EventArgs e)
        {
            _algorithmOption = AlgorithmBenchmarkSettingsType.ReBecnhSelectedAlgorithms;
            CalcBenchmarkDevicesAlgorithmQueue();
            devicesListViewEnableControl1.ResetListItemColors();
            algorithmsListView1.ResetListItemColors();
        }

        private void CheckBox_StartMiningAfterBenchmark_CheckedChanged(object sender, EventArgs e)
        {
            StartMining = checkBox_StartMiningAfterBenchmark.Checked;
        }
    }
}

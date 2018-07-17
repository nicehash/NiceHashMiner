using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Benchmarking;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Properties;
using NiceHashMinerLegacy.Common.Enums;
using Timer = System.Windows.Forms.Timer;

namespace NiceHashMiner.Forms
{
    public partial class Form_Benchmark : Form, IListItemCheckColorSetter, IBenchmarkForm, IBenchmarkCalculation
    {
        private static readonly Color DisabledColor = Color.DarkGray;
        private static readonly Color BenchmarkedColor = Color.LightGreen;
        private static readonly Color UnbenchmarkedColor = Color.LightBlue;

        private AlgorithmBenchmarkSettingsType _algorithmOption =
            AlgorithmBenchmarkSettingsType.SelectedUnbenchmarkedAlgorithms;

        private int _bechmarkCurrentIndex;
        private int _benchmarkAlgorithmsCount;

        private List<Tuple<ComputeDevice, Queue<Algorithm>>> _benchmarkDevicesAlgorithmQueue;

        private Dictionary<string, BenchmarkSettingsStatus> _benchmarkDevicesAlgorithmStatus;
        //private AlgorithmType _singleBenchmarkType = AlgorithmType.NONE;

        private readonly Timer _benchmarkingTimer;
        private int _dotCount;

        private bool _hasFailedAlgorithms;
        private List<BenchmarkHandler> _runningBenchmarkThreads = new List<BenchmarkHandler>();
        private Dictionary<ComputeDevice, Algorithm> _statusCheckAlgos;

        private readonly bool ExitWhenFinished;

        public bool StartMining { get; private set; }

        public bool InBenchmark { get; private set; }

        public Form_Benchmark(BenchmarkPerformanceType benchmarkPerformanceType = BenchmarkPerformanceType.Standard,
            bool autostart = false)
        {
            InitializeComponent();
            Icon = Resources.logo;

            StartMining = false;

            // clear prev pending statuses
            foreach (var dev in ComputeDeviceManager.Available.Devices)
            foreach (var algo in dev.GetAlgorithmSettings())
                algo.ClearBenchmarkPendingFirst();

            benchmarkOptions1.SetPerformanceType(benchmarkPerformanceType);

            // benchmark only unique devices
            devicesListViewEnableControl1.SetIListItemCheckColorSetter(this);
            devicesListViewEnableControl1.SetComputeDevices(ComputeDeviceManager.Available.Devices);

            InitLocale();

            _benchmarkingTimer = new Timer();
            _benchmarkingTimer.Tick += BenchmarkingTimer_Tick;
            _benchmarkingTimer.Interval = 1000; // 1s

            //// name, UUID
            //Dictionary<string, string> benchNamesUUIDs = new Dictionary<string, string>();
            //// initialize benchmark settings for same cards to only copy settings
            //foreach (var cDev in ComputeDeviceManager.Available.Devices) {
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
            if (ComputeDeviceManager.Available.Devices.Count > 0)
            {
                var firstComputedevice = ComputeDeviceManager.Available.Devices[0];
                algorithmsListView1.SetAlgorithms(firstComputedevice, firstComputedevice.Enabled);
            }

            if (autostart)
            {
                ExitWhenFinished = true;
                StartStopBtn_Click(null, null);
            }
        }

        #region IBenchmarkCalculation methods

        public void CalcBenchmarkDevicesAlgorithmQueue()
        {
            _benchmarkAlgorithmsCount = 0;
            _benchmarkDevicesAlgorithmStatus = new Dictionary<string, BenchmarkSettingsStatus>();
            _benchmarkDevicesAlgorithmQueue = new List<Tuple<ComputeDevice, Queue<Algorithm>>>();
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                var algorithmQueue = new Queue<Algorithm>();
                foreach (var algo in cDev.GetAlgorithmSettings())
                    if (ShoulBenchmark(algo))
                    {
                        algorithmQueue.Enqueue(algo);
                        algo.SetBenchmarkPendingNoMsg();
                    }
                    else
                    {
                        algo.ClearBenchmarkPending();
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
            _bechmarkCurrentIndex = 0;
        }

        #endregion

        #region IBenchmarkForm methods

        public void AddToStatusCheck(ComputeDevice device, Algorithm algorithm)
        {
            Invoke((MethodInvoker) delegate
            {
                _statusCheckAlgos[device] = algorithm;
            });
        }

        public void RemoveFromStatusCheck(ComputeDevice device, Algorithm algorithm)
        {
            Invoke((MethodInvoker) delegate
            {
                _statusCheckAlgos.Remove(device);
            });
        }

        public void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos)
        {
            _hasFailedAlgorithms = failedAlgos || _hasFailedAlgorithms;
            lock (_runningBenchmarkThreads)
            {
                _runningBenchmarkThreads.RemoveAll(x => x.Device == device);

                if (_runningBenchmarkThreads.Count <= 0) 
                    EndBenchmark();
            }
        }


        public void SetCurrentStatus(ComputeDevice device, Algorithm algorithm, string status)
        {
            Invoke((MethodInvoker) delegate
            {
                algorithmsListView1.SetSpeedStatus(device, algorithm, status);
            });
        }

        public void StepUpBenchmarkStepProgress()
        {
            if (InvokeRequired) Invoke((MethodInvoker) StepUpBenchmarkStepProgress);
            else 
            {
                _bechmarkCurrentIndex++;
                SetLabelBenchmarkSteps(_bechmarkCurrentIndex, _benchmarkAlgorithmsCount);
                if (_bechmarkCurrentIndex <= progressBarBenchmarkSteps.Maximum)
                    progressBarBenchmarkSteps.Value = _bechmarkCurrentIndex;
            }
        }

        #endregion

        #region IListItemCheckColorSetter methods

        public void LviSetColor(ListViewItem lvi)
        {
            if (lvi.Tag is ComputeDevice cDevice && _benchmarkDevicesAlgorithmStatus != null)
            {
                var uuid = cDevice.Uuid;
                if (!cDevice.Enabled)
                    lvi.BackColor = DisabledColor;
                else
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
                //// enable disable status, NOT needed
                //if (cdvo.IsEnabled && _benchmarkDevicesAlgorithmStatus[uuid] >= BenchmarkSettingsStatus.DISABLED_NONE) {
                //    _benchmarkDevicesAlgorithmStatus[uuid] -= 2;
                //} else if (!cdvo.IsEnabled && _benchmarkDevicesAlgorithmStatus[uuid] <= BenchmarkSettingsStatus.TODO) {
                //    _benchmarkDevicesAlgorithmStatus[uuid] += 2;
                //}
            }
        }

        #endregion

        private void CopyBenchmarks()
        {
            Helpers.ConsolePrint("CopyBenchmarks", "Checking for benchmarks to copy");
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
                // check if copy
                if (!cDev.Enabled && cDev.BenchmarkCopyUuid != null)
                {
                    var copyCdevSettings = ComputeDeviceManager.Available.GetDeviceWithUuid(cDev.BenchmarkCopyUuid);
                    if (copyCdevSettings != null)
                    {
                        Helpers.ConsolePrint("CopyBenchmarks", $"Copy from {cDev.Uuid} to {cDev.BenchmarkCopyUuid}");
                        cDev.CopyBenchmarkSettingsFrom(copyCdevSettings);
                    }
                }
        }

        private void BenchmarkingTimer_Tick(object sender, EventArgs e)
        {
            if (InBenchmark)
                foreach (var key in _statusCheckAlgos.Keys)
                    algorithmsListView1.SetSpeedStatus(key, _statusCheckAlgos[key], GetDotsWaitString());
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

        #region Start/Stop methods

        private void StartStopBtn_Click(object sender, EventArgs e)
        {
            if (InBenchmark)
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
            if (InBenchmark)
            {
                StopButonClick();
                BenchmarkStoppedGuiSettings();
            }
        }

        private void BenchmarkStoppedGuiSettings()
        {
            StartStopBtn.Text = International.GetText("Form_Benchmark_buttonStartBenchmark");
            foreach (var deviceAlgosTuple in _benchmarkDevicesAlgorithmQueue)
            {
                foreach (var algo in deviceAlgosTuple.Item2) algo.ClearBenchmarkPending();
                algorithmsListView1.RepaintStatus(deviceAlgosTuple.Item1.Enabled, deviceAlgosTuple.Item1.Uuid);
            }

            ResetBenchmarkProgressStatus();
            CalcBenchmarkDevicesAlgorithmQueue();
            benchmarkOptions1.Enabled = true;

            algorithmsListView1.IsInBenchmark = false;
            devicesListViewEnableControl1.IsInBenchmark = false;

            CloseBtn.Enabled = true;
        }

        // TODO add list for safety and kill all miners
        private void StopButonClick()
        {
            _benchmarkingTimer.Stop();
            InBenchmark = false;
            Helpers.ConsolePrint("FormBenchmark", "StopButonClick() benchmark routine stopped");
            //// copy benchmarked
            //CopyBenchmarks();
            lock (_runningBenchmarkThreads)
            {
                foreach (var handler in _runningBenchmarkThreads) handler.InvokeQuit();
            }

            if (ExitWhenFinished) Close();
        }

        private bool StartButonClick()
        {
            CalcBenchmarkDevicesAlgorithmQueue();
            // device selection check scope
            {
                var noneSelected = ComputeDeviceManager.Available.Devices.All(cDev => !cDev.Enabled);
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

            _hasFailedAlgorithms = false;
            _statusCheckAlgos = new Dictionary<ComputeDevice, Algorithm>();
            lock (_runningBenchmarkThreads)
            {
                _runningBenchmarkThreads = new List<BenchmarkHandler>();
            }

            // disable gui controls
            benchmarkOptions1.Enabled = false;
            CloseBtn.Enabled = false;
            algorithmsListView1.IsInBenchmark = true;
            devicesListViewEnableControl1.IsInBenchmark = true;
            // set benchmark pending status
            foreach (var deviceAlgosTuple in _benchmarkDevicesAlgorithmQueue)
            {
                foreach (var algo in deviceAlgosTuple.Item2) algo.SetBenchmarkPending();
                if (deviceAlgosTuple.Item1 != null)
                    algorithmsListView1.RepaintStatus(deviceAlgosTuple.Item1.Enabled, deviceAlgosTuple.Item1.Uuid);
            }

            StartBenchmark();

            return true;
        }

        private bool ShoulBenchmark(Algorithm algorithm)
        {
            var isBenchmarked = !algorithm.BenchmarkNeeded;
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
            InBenchmark = true;
            lock (_runningBenchmarkThreads)
            {
                foreach (var pair in _benchmarkDevicesAlgorithmQueue)
                {
                    var handler = new BenchmarkHandler(pair.Item1, pair.Item2, this, benchmarkOptions1.PerformanceType);
                    _runningBenchmarkThreads.Add(handler);
                }
                // Don't start until list is populated
                foreach (var thread in _runningBenchmarkThreads)
                {
                    thread.Start();
                }
            }

            _benchmarkingTimer.Start();
        }

        private void EndBenchmark()
        {
            Invoke((MethodInvoker) delegate
            {
                _benchmarkingTimer.Stop();
                InBenchmark = false;
                Ethlargement.Stop();
                Helpers.ConsolePrint("FormBenchmark", "EndBenchmark() benchmark routine finished");

                //CopyBenchmarks();

                BenchmarkStoppedGuiSettings();
                // check if all ok
                if (!_hasFailedAlgorithms && StartMining == false)
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
                    foreach (var algorithm in deviceAlgoQueue.Item2)
                        algorithm.Enabled = false;
                }

                if (ExitWhenFinished || StartMining) Close();
            });
        }

        #endregion

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormBenchmark_New_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (InBenchmark)
            {
                e.Cancel = true;
                return;
            }

            // disable all pending benchmark
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            foreach (var algorithm in cDev.GetAlgorithmSettings())
                algorithm.ClearBenchmarkPending();

            // save already benchmarked algorithms
            ConfigManager.CommitBenchmarks();
            // check devices without benchmarks
            foreach (var cdev in ComputeDeviceManager.Available.Devices)
                if (cdev.Enabled)
                {
                    var enabled = cdev.GetAlgorithmSettings().Any(algo => algo.BenchmarkSpeed > 0);
                    cdev.Enabled = enabled;
                }
        }

        private void DevicesListView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            //algorithmSettingsControl1.Deselect();
            // show algorithms
            var selectedComputeDevice =
                ComputeDeviceManager.Available.GetCurrentlySelectedComputeDevice(e.ItemIndex, true);
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

        private enum BenchmarkSettingsStatus
        {
            NONE = 0,
            TODO,
            DISABLED_NONE,
            DISABLED_TODO
        }


        #region Benchmark progress GUI stuff

        private void SetLabelBenchmarkSteps(int current, int max)
        {
            labelBenchmarkSteps.Text =
                string.Format(International.GetText("FormBenchmark_Benchmark_Step"), current, max);
        }

        private void ResetBenchmarkProgressStatus()
        {
            progressBarBenchmarkSteps.Value = 0;
        }

        #endregion // Benchmark progress GUI stuff
    }
}

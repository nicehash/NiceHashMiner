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

using BenchmarkSelection = NiceHashMinerLegacy.Common.Enums.AlgorithmBenchmarkSettingsType;
using NiceHashMinerLegacy.Common;

namespace NiceHashMiner.Forms
{
    public partial class Form_Benchmark : Form, FormHelpers.ICustomTranslate, IListItemCheckColorSetter, IBenchmarkForm, IBenchmarkCalculation
    {
        private static readonly Color DisabledColor = Color.DarkGray;
        private static readonly Color BenchmarkedColor = Color.LightGreen;
        private static readonly Color UnbenchmarkedColor = Color.LightBlue;

        private readonly Timer _benchmarkingTimer;
        private int _dotCount;

        private readonly bool _exitWhenFinished;

        public bool StartMiningOnFinish { get; private set; } = false;

        public Form_Benchmark(BenchmarkPerformanceType benchmarkPerformanceType = BenchmarkPerformanceType.Standard,
            bool autostart = false)
        {
            InitializeComponent();
            Icon = Resources.logo;

            // clear prev pending statuses
            foreach (var dev in AvailableDevices.Devices)
            foreach (var algo in dev.AlgorithmSettings)
                algo.ClearBenchmarkPendingFirst();

            benchmarkOptions1.SetPerformanceType(benchmarkPerformanceType);

            // benchmark only unique devices
            devicesListViewEnableControl1.SetIListItemCheckColorSetter(this);
            devicesListViewEnableControl1.SetComputeDevices(AvailableDevices.Devices.ToList());

            _benchmarkingTimer = new Timer();
            _benchmarkingTimer.Tick += BenchmarkingTimer_Tick;
            _benchmarkingTimer.Interval = 1000; // 1s

            BenchmarkManager.OnAlgoStatusUpdate += SetCurrentStatus;
            BenchmarkManager.OnStepUp += StepUpBenchmarkStepProgress;
            BenchmarkManager.OnBenchmarkEnd += EndBenchmark;

            devicesListViewEnableControl1.Enabled = true;
            devicesListViewEnableControl1.SetDeviceSelectionChangedCallback(DevicesListView1_ItemSelectionChanged);

            devicesListViewEnableControl1.SetAlgorithmsListView(algorithmsListView1);
            devicesListViewEnableControl1.IsSettingsCopyEnabled = true;

            ResetBenchmarkProgressStatus();
            CalcBenchmarkDevicesAlgorithmQueue();
            devicesListViewEnableControl1.ResetListItemColors();

            // to update laclulation status
            devicesListViewEnableControl1.BenchmarkCalculation = this;
            algorithmsListView1.BenchmarkCalculation = this;

            // set first device selected {
            if (AvailableDevices.Devices.Count > 0)
            {
                var firstComputedevice = AvailableDevices.Devices[0];
                algorithmsListView1.SetAlgorithms(firstComputedevice, firstComputedevice.Enabled);
            }

            if (autostart)
            {
                _exitWhenFinished = true;
                StartStopBtn_Click(null, null);
            }

            FormHelpers.TranslateFormControls(this);
        }

        #region IBenchmarkCalculation methods

        public void CalcBenchmarkDevicesAlgorithmQueue()
        {
            var count = BenchmarkManager.CalcBenchDevAlgoQueue();

            // GUI stuff
            progressBarBenchmarkSteps.Maximum = count;
            progressBarBenchmarkSteps.Value = 0;
            SetLabelBenchmarkSteps(0, count);
        }

#endregion

        private void SetCurrentStatus(object sender, AlgoStatusEventArgs e)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                algorithmsListView1.SetSpeedStatus(e.Device, e.Algorithm, e.Status);
            });
        }

        private void StepUpBenchmarkStepProgress(object sender, StepUpEventArgs e)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                SetLabelBenchmarkSteps(e.CurrentIndex, e.AlgorithmCount);
                if (e.CurrentIndex <= progressBarBenchmarkSteps.Maximum)
                    progressBarBenchmarkSteps.Value = e.CurrentIndex;
            });
        }

#region IListItemCheckColorSetter methods

        public void LviSetColor(ListViewItem lvi)
        {
            if (!(lvi.Tag is ComputeDevice cDevice)) return;

            var uuid = cDevice.Uuid;
            if (!cDevice.Enabled)
                lvi.BackColor = DisabledColor;
            else
            {
                lvi.BackColor = BenchmarkManager.IsDevBenchmarked(uuid) ? BenchmarkedColor : UnbenchmarkedColor;
            }
        }

#endregion

        private void CopyBenchmarks()
        {
            Logger.Debug("CopyBenchmarks", "Checking for benchmarks to copy");
            foreach (var cDev in AvailableDevices.Devices)
                // check if copy
                if (!cDev.Enabled && cDev.BenchmarkCopyUuid != null)
                {
                    var copyCdevSettings = AvailableDevices.GetDeviceWithUuid(cDev.BenchmarkCopyUuid);
                    if (copyCdevSettings != null)
                    {
                        Logger.Info("CopyBenchmarks", $"Copy from {cDev.Uuid} to {cDev.BenchmarkCopyUuid}");
                        cDev.CopyBenchmarkSettingsFrom(copyCdevSettings);
                    }
                }
        }

        private void BenchmarkingTimer_Tick(object sender, EventArgs e)
        {
            foreach (var check in BenchmarkManager.GetStatusCheckAlgos())
            {
                algorithmsListView1.SetSpeedStatus(check.Item1, check.Item2, GetDotsWaitString());
            }
        }

        private string GetDotsWaitString()
        {
            ++_dotCount;
            if (_dotCount > 3) _dotCount = 1;
            return new string('.', _dotCount);
        }
        void FormHelpers.ICustomTranslate.CustomTranslate()
        {
            // TODO fix locale for benchmark enabled label
            devicesListViewEnableControl1.InitLocale();
            //benchmarkOptions1.InitLocale();
            //algorithmsListView1.InitLocale();
        }

#region Start/Stop methods

        private void StartStopBtn_Click(object sender, EventArgs e)
        {
            if (BenchmarkManager.InBenchmark)
            {
                StopButonClick();
                BenchmarkStoppedGuiSettings();
            }
            else if (StartButonClick())
            {
                StartStopBtn.Text = Translations.Tr("St&op benchmark");
            }
        }

        public void StopBenchmark()
        {
            if (BenchmarkManager.InBenchmark)
            {
                StopButonClick();
                BenchmarkStoppedGuiSettings();
            }
        }

        private void BenchmarkStoppedGuiSettings()
        {
            StartStopBtn.Text = Translations.Tr("Start &benchmark");
            foreach (var deviceAlgosTuple in BenchmarkManager.BenchDevAlgoQueue)
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
            Logger.Debug("FormBenchmark", "StopButonClick() benchmark routine stopped");
            //// copy benchmarked
            //CopyBenchmarks();

            BenchmarkManager.Stop();

            if (_exitWhenFinished) Close();
        }

        private bool StartButonClick()
        {
            CalcBenchmarkDevicesAlgorithmQueue();
            // device selection check scope
            {
                var noneSelected = AvailableDevices.Devices.All(cDev => !cDev.Enabled);
                if (noneSelected)
                {
                    MessageBox.Show(Translations.Tr("No device has been selected there is nothing to benchmark"),
                        Translations.Tr("No device selected"),
                        MessageBoxButtons.OK);
                    return false;
                }
            }
            // device todo benchmark check scope
            {
                if (!BenchmarkManager.HasWork)
                {
                    MessageBox.Show(Translations.Tr("Current benchmark settings are already executed. There is nothing to do."),
                        Translations.Tr("Nothing to benchmark"),
                        MessageBoxButtons.OK);
                    return false;
                }
            }

            // disable gui controls
            benchmarkOptions1.Enabled = false;
            CloseBtn.Enabled = false;
            algorithmsListView1.IsInBenchmark = true;
            devicesListViewEnableControl1.IsInBenchmark = true;
            // set benchmark pending status
            foreach (var deviceAlgosTuple in BenchmarkManager.BenchDevAlgoQueue)
            {
                foreach (var algo in deviceAlgosTuple.Item2) algo.SetBenchmarkPending();
                if (deviceAlgosTuple.Item1 != null)
                    algorithmsListView1.RepaintStatus(deviceAlgosTuple.Item1.Enabled, deviceAlgosTuple.Item1.Uuid);
            }
            
            BenchmarkManager.Start(benchmarkOptions1.PerformanceType, this);
            _benchmarkingTimer.Start();

            return true;
        }

        public void EndBenchmark(object sender, bool hasFailedAlgos)
        {
            if (ApplicationStateManager.BurnCalled) {
                return;
            }
            FormHelpers.SafeInvoke(this, () => {
                _benchmarkingTimer.Stop();
                Logger.Debug("FormBenchmark", "EndBenchmark() benchmark routine finished");

                //CopyBenchmarks();

                BenchmarkStoppedGuiSettings();
                // check if all ok
                if (!hasFailedAlgos && StartMiningOnFinish == false)
                {
                    MessageBox.Show(
                        Translations.Tr("All benchmarks have been successful"),
                        Translations.Tr("Benchmark finished report"),
                        MessageBoxButtons.OK);
                }
                else if (StartMiningOnFinish == false)
                {
                    var result = MessageBox.Show(
                        //Translations.Tr("Not all benchmarks finished successfully. Retry to re-run the benchmark process against unbenchmarked algos or Cancel to disable unbenchmarked algorithms."),
                        Translations.Tr("Not all benchmarks finished successfully."),
                        Translations.Tr("Benchmark finished report"),
                        MessageBoxButtons.OK);

                    //if (result == DialogResult.Retry)
                    //{
                    //    StartButonClick();
                    //    return;
                    //}

                    // update algos status
                    BenchmarkManager.CalcBenchDevAlgoQueue();
                }

                if (_exitWhenFinished || StartMiningOnFinish) Close();
            });
        }

#endregion

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormBenchmark_New_FormClosing(object sender, FormClosingEventArgs e)
        {
            BenchmarkManager.OnAlgoStatusUpdate -= SetCurrentStatus;
            BenchmarkManager.OnStepUp -= StepUpBenchmarkStepProgress;
            BenchmarkManager.OnBenchmarkEnd -= EndBenchmark;
            if (ApplicationStateManager.BurnCalled) {
                return;
            }
            if (BenchmarkManager.InBenchmark)
            {
                e.Cancel = true;
                return;
            }

            // disable all pending benchmark
            foreach (var cDev in AvailableDevices.Devices)
            {
                foreach (var algorithm in cDev.AlgorithmSettings)
                {
                    algorithm.ClearBenchmarkPending();
                }
            }

            BenchmarkManager.ClearQueue();

            // save already benchmarked algorithms
            ConfigManager.CommitBenchmarks();
        }

        private void DevicesListView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            //algorithmSettingsControl1.Deselect();
            // show algorithms
            var selectedComputeDevice =
                AvailableDevices.GetCurrentlySelectedComputeDevice(e.ItemIndex, true);
            algorithmsListView1.SetAlgorithms(selectedComputeDevice, selectedComputeDevice.Enabled);
        }

        private void RadioButton_SelectedUnbenchmarked_CheckedChanged_1(object sender, EventArgs e)
        {
            BenchmarkManager.Selection = BenchmarkSelection.SelectedUnbenchmarkedAlgorithms;
            CalcBenchmarkDevicesAlgorithmQueue();
            devicesListViewEnableControl1.ResetListItemColors();
            algorithmsListView1.ResetListItemColors();
        }

        private void RadioButton_RE_SelectedUnbenchmarked_CheckedChanged(object sender, EventArgs e)
        {
            BenchmarkManager.Selection = BenchmarkSelection.ReBecnhSelectedAlgorithms;
            CalcBenchmarkDevicesAlgorithmQueue();
            devicesListViewEnableControl1.ResetListItemColors();
            algorithmsListView1.ResetListItemColors();
        }

        private void CheckBox_StartMiningAfterBenchmark_CheckedChanged(object sender, EventArgs e)
        {
            StartMiningOnFinish = checkBox_StartMiningAfterBenchmark.Checked;
        }


#region Benchmark progress GUI stuff

        private void SetLabelBenchmarkSteps(int current, int max)
        {
            labelBenchmarkSteps.Text =
                string.Format(Translations.Tr("Benchmark step ( {0} / {1} )"), current, max);
        }

        private void ResetBenchmarkProgressStatus()
        {
            progressBarBenchmarkSteps.Value = 0;
        }

        #endregion // Benchmark progress GUI stuff
    }
}

using NHM.Common;
using NHMCore.ApplicationState;
using System.Windows;

namespace NiceHashMiner.ViewModels
{
    public class DevicesViewModel : NotifyChangedBase
    {
        public DevicesViewModel()
        {
            MiningState.Instance.PropertyChanged += Instance_PropertyChanged;

            Instance_PropertyChanged(null, null);
        }

        public int RunnableDevices { get; private set; } = 0;
        public int RunningDevices { get; private set; } = 0;


        public string RunnableDevicesDisplayString => $"/ {NHMCore.Mining.AvailableDevices.Devices.Count}";

        private enum WhatCollor
        {
            Warning,
            Error,
            Good
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //var hasDevicesStopped = MiningState.Instance.StoppedDeviceStateCount > 0;
            var hasDevicesMining = MiningState.Instance.MiningDeviceStateCount > 0;
            var hasDevicesBenchmarking = MiningState.Instance.BenchmarkingDeviceStateCount > 0;
            var hasDevicesError = MiningState.Instance.ErrorDeviceStateCount > 0;
            var hasDevicesPending = MiningState.Instance.PendingDeviceStateCount > 0;
            //var hasDevicesDisabled = MiningState.Instance.DisabledDeviceStateCount > 0;

            RunningDevices = MiningState.Instance.BenchmarkingDeviceStateCount + MiningState.Instance.MiningDeviceStateCount;
            RunnableDevices = MiningState.Instance.BenchmarkingDeviceStateCount + MiningState.Instance.MiningDeviceStateCount + MiningState.Instance.StoppedDeviceStateCount;

            OnPropertyChanged(nameof(RunningDevices));
            OnPropertyChanged(nameof(RunnableDevices));
            OnPropertyChanged(nameof(RunnableDevicesDisplayString));

            var disabledErrorPendingCount = MiningState.Instance.ErrorDeviceStateCount + MiningState.Instance.PendingDeviceStateCount + MiningState.Instance.DisabledDeviceStateCount;

            var setColor = WhatCollor.Warning;
            if (RunnableDevices != 0 && hasDevicesMining && RunningDevices == RunnableDevices && !(hasDevicesError || hasDevicesBenchmarking || hasDevicesPending))
            {
                setColor = WhatCollor.Good;
            }
            if ((hasDevicesError || RunnableDevices == 0) && !hasDevicesPending)
            {
                setColor = WhatCollor.Error;
            }

            Application.Current.Resources["MiningDevices"] = Application.Current.FindResource($"MiningDevices.{setColor}");
            Application.Current.Resources["MiningDevicesBackground"] = Application.Current.FindResource($"MiningDevicesBackground.{setColor}");
        }
    }
}

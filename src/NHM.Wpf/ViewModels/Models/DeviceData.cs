using NHM.Common;
using NHM.Common.Enums;
using NHMCore;
using NHMCore.Mining;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace NHM.Wpf.ViewModels.Models
{
    /// <summary>
    /// Wrapper for <see cref="ComputeDevice"/> to convert for device status DataGrid
    /// </summary>
    public class DeviceData : NotifyChangedBase
    {
        public ComputeDevice Dev { get; }

        public bool Enabled
        {
            get => Dev.Enabled;
            set
            {
                ApplicationStateManager.SetDeviceEnabledState(this, (Dev.B64Uuid, value));
                OnPropertyChanged();
            }
        }

        public string AlgoOptions
        {
            get
            {
                var enabledAlgos = Dev.AlgorithmSettings.Count(a => a.Enabled);
                var benchedAlgos = Dev.AlgorithmSettings.Count(a => !a.BenchmarkNeeded);
                return $"{Dev.AlgorithmSettings.Count} / {enabledAlgos} / {benchedAlgos}";
            }
        }

        public string ButtonLabel
        {
            get
            {
                // assume disabled
                var buttonLabel = "N/A";
                if (Dev.State == DeviceState.Stopped)
                {
                    buttonLabel = "Start";
                }
                else if (Dev.State == DeviceState.Mining || Dev.State == DeviceState.Benchmarking)
                {
                    buttonLabel = "Stop";
                }
                return Translations.Tr(buttonLabel);
            }
        }

        public ICommand StartStopCommand { get; }

        public DeviceData(ComputeDevice dev)
        {
            Dev = dev;

            StartStopCommand = new BaseCommand(StartStopClick);

            Dev.PropertyChanged += DevOnPropertyChanged;

            foreach (var algo in Dev.AlgorithmSettings)
            {
                algo.PropertyChanged += AlgoOnPropertyChanged;
            }
        }

        private void AlgoOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlgorithmContainer.Enabled) || e.PropertyName == nameof(AlgorithmContainer.BenchmarkNeeded))
                OnPropertyChanged(nameof(AlgoOptions));
        }

        private void DevOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dev.State))
            {
                OnPropertyChanged(nameof(ButtonLabel));
            }
            else if (e.PropertyName == nameof(Dev.Enabled))
            {
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public void RefreshDiag()
        {
            Dev.OnPropertyChanged(nameof(Dev.Load));
            Dev.OnPropertyChanged(nameof(Dev.Temp));
            Dev.OnPropertyChanged(nameof(Dev.FanSpeed));
        }

        private void StartStopClick(object param)
        {
            switch (Dev.State)
            {
                case DeviceState.Stopped:
                    ApplicationStateManager.StartSingleDevicePublic(Dev);
                    break;
                case DeviceState.Mining:
                case DeviceState.Benchmarking:
                    ApplicationStateManager.StopSingleDevicePublic(Dev);
                    break;
            }
        }

        public static implicit operator DeviceData(ComputeDevice dev)
        {
            return new DeviceData(dev);
        }
    }
}

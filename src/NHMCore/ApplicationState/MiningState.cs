using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Utils;
using System.Linq;

namespace NHMCore.ApplicationState
{
    public class MiningState : NotifyChangedBase
    {
        public static MiningState Instance { get; } = new MiningState();

        private MiningState()
        {
            _boolProps = new NotifyPropertyChangedHelper<bool>(OnPropertyChanged);
            _intProps = new NotifyPropertyChangedHelper<int>(OnPropertyChanged);
            IsDemoMining = false;
            IsCurrentlyMining = false;
        }

        // auto properties don't trigger NotifyPropertyChanged so add this shitty boilerplate
        private readonly NotifyPropertyChangedHelper<bool> _boolProps;
        private readonly NotifyPropertyChangedHelper<int> _intProps;


        public bool IsDemoMining
        {
            get => _boolProps.Get(nameof(IsDemoMining));
            private set => _boolProps.Set(nameof(IsDemoMining), value);
        }

        public bool AllDeviceEnabled
        {
            get => _boolProps.Get(nameof(AllDeviceEnabled));
            private set => _boolProps.Set(nameof(AllDeviceEnabled), value);
        }

        public bool AnyDeviceEnabled
        {
            get => _boolProps.Get(nameof(AnyDeviceEnabled));
            private set => _boolProps.Set(nameof(AnyDeviceEnabled), value);
        }

        public bool AnyDeviceStopped
        {
            get => _boolProps.Get(nameof(AnyDeviceStopped));
            private set => _boolProps.Set(nameof(AnyDeviceStopped), value);
        }

        public bool AnyDeviceRunning
        {
            get => _boolProps.Get(nameof(AnyDeviceRunning));
            private set => _boolProps.Set(nameof(AnyDeviceRunning), value);
        }

        public bool IsNotBenchmarkingOrMining
        {
            get => _boolProps.Get(nameof(IsNotBenchmarkingOrMining));
            private set => _boolProps.Set(nameof(IsNotBenchmarkingOrMining), value);
        }

        public bool IsCurrentlyMining
        {
            get => _boolProps.Get(nameof(IsCurrentlyMining));
            private set => _boolProps.Set(nameof(IsCurrentlyMining), value);
        }

        #region DeviceState Counts
        public int StoppedDeviceStateCount
        {
            get => _intProps.Get(nameof(StoppedDeviceStateCount));
            private set => _intProps.Set(nameof(StoppedDeviceStateCount), value);
        }
        public int MiningDeviceStateCount
        {
            get => _intProps.Get(nameof(MiningDeviceStateCount));
            private set => _intProps.Set(nameof(MiningDeviceStateCount), value);
        }
        public int BenchmarkingDeviceStateCount
        {
            get => _intProps.Get(nameof(BenchmarkingDeviceStateCount));
            private set => _intProps.Set(nameof(BenchmarkingDeviceStateCount), value);
        }
        public int ErrorDeviceStateCount
        {
            get => _intProps.Get(nameof(ErrorDeviceStateCount));
            private set => _intProps.Set(nameof(ErrorDeviceStateCount), value);
        }
        public int PendingDeviceStateCount
        {
            get => _intProps.Get(nameof(PendingDeviceStateCount));
            private set => _intProps.Set(nameof(PendingDeviceStateCount), value);
        }
        public int DisabledDeviceStateCount
        {
            get => _intProps.Get(nameof(DisabledDeviceStateCount));
            private set => _intProps.Set(nameof(DisabledDeviceStateCount), value);
        }
        #endregion DeviceState Counts

        public bool MiningManuallyStarted { get; set; }

        // poor mans way
        public void CalculateDevicesStateChange()
        {
            // DeviceState Counts
            StoppedDeviceStateCount = AvailableDevices.Devices.Count(dev => dev.State == DeviceState.Stopped);
            MiningDeviceStateCount = AvailableDevices.Devices.Count(dev => dev.State == DeviceState.Mining);
            BenchmarkingDeviceStateCount = AvailableDevices.Devices.Count(dev => dev.State == DeviceState.Benchmarking);
            ErrorDeviceStateCount = AvailableDevices.Devices.Count(dev => dev.State == DeviceState.Error);
            PendingDeviceStateCount = AvailableDevices.Devices.Count(dev => dev.State == DeviceState.Pending);
            DisabledDeviceStateCount = AvailableDevices.Devices.Count(dev => dev.State == DeviceState.Disabled);
            // Mining state
            AllDeviceEnabled = AvailableDevices.Devices.All(dev => dev.Enabled);
            AnyDeviceEnabled = AvailableDevices.Devices.Any(dev => dev.Enabled);
            AnyDeviceStopped = AvailableDevices.Devices.Any(dev => dev.State == DeviceState.Stopped && (dev.State != DeviceState.Disabled));
            AnyDeviceRunning = AvailableDevices.Devices.Any(dev => dev.State == DeviceState.Mining || dev.State == DeviceState.Benchmarking);
            IsNotBenchmarkingOrMining = !AnyDeviceRunning;
            IsCurrentlyMining = AnyDeviceRunning;
            IsDemoMining = !CredentialsSettings.Instance.IsBitcoinAddressValid && IsCurrentlyMining;
            if (IsNotBenchmarkingOrMining) MiningManuallyStarted = false;
        }
    }
}

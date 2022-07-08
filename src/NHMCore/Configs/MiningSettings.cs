using NHM.Common;
using NHMCore.Mining;
using System.Collections.Generic;

namespace NHMCore.Configs
{
    public class MiningSettings : NotifyChangedBase
    {
        public static MiningSettings Instance { get; } = new MiningSettings();
        private MiningSettings() { }

        private bool _autoStartMining = false;
        public bool AutoStartMining
        {
            get => _autoStartMining;
            set
            {
                _autoStartMining = value;
                OnPropertyChanged(nameof(AutoStartMining));
            }
        }

        private int _minerAPIQueryInterval = 5;
        public int MinerAPIQueryInterval
        {
            get => _minerAPIQueryInterval;
            set
            {
                _minerAPIQueryInterval = value;
                OnPropertyChanged(nameof(MinerAPIQueryInterval));
            }
        }

        private bool _hideMiningWindows = false;
        public bool HideMiningWindows
        {
            get => _hideMiningWindows;
            set
            {
                _hideMiningWindows = value;
                NHM.MinerPluginToolkitV1.MinerToolkit.HideMiningWindows = value;
                OnPropertyChanged(nameof(HideMiningWindows));
                OnPropertyChanged(nameof(HideMiningWindowsAlertVisible));
            }
        }

        private bool _minimizeMiningWindows = false;
        public bool MinimizeMiningWindows
        {
            get => _minimizeMiningWindows;
            set
            {
                _minimizeMiningWindows = value;
                NHM.MinerPluginToolkitV1.MinerToolkit.MinimizeMiningWindows = value;
                OnPropertyChanged(nameof(MinimizeMiningWindows));
                OnPropertyChanged(nameof(HideMiningWindowsAlertVisible));
            }
        }

        // TODO make this per plugin
        private int _minerRestartDelayMS = 1000;
        public int MinerRestartDelayMS
        {
            get => _minerRestartDelayMS;
            set
            {
                _minerRestartDelayMS = value;
                OnPropertyChanged(nameof(MinerRestartDelayMS));
            }
        }

        private int _apiBindPortPoolStart = 4000;
        public int ApiBindPortPoolStart
        {
            get => _apiBindPortPoolStart;
            set
            {
                _apiBindPortPoolStart = value;
                NHM.MinerPluginToolkitV1.FreePortsCheckerManager.ApiBindPortPoolStart = value;
                OnPropertyChanged(nameof(ApiBindPortPoolStart));
            }
        }

        private bool _pauseMiningWhenGamingMode = false;
        public bool PauseMiningWhenGamingMode
        {
            get => _pauseMiningWhenGamingMode;
            set
            {
                _pauseMiningWhenGamingMode = value;
                OnPropertyChanged(nameof(PauseMiningWhenGamingMode));
            }
        }

        private bool _useScheduler = false;

        public bool UseScheduler
        {
            get => _useScheduler;
            set
            {
                _useScheduler = value;
                OnPropertyChanged(nameof(UseScheduler));
            }
        }

        private bool _enableSSLMining = false;
        public bool EnableSSLMining
        {
            get => _enableSSLMining;
            set
            {
                _enableSSLMining = value;
                NHM.MinerPluginToolkitV1.MinerToolkit.EnableSSLMining = value;
                OnPropertyChanged(nameof(EnableSSLMining));
            }
        }

        public IEnumerable<ComputeDevice> GPUs => AvailableDevices.GPUs;

        private string _deviceToPauseUuid = "";

        public string DeviceToPauseUuid
        {
            get => _deviceToPauseUuid;
            set
            {
                _deviceToPauseUuid = value;
                OnPropertyChanged(nameof(DeviceToPauseUuid));
            }
        }

        public int DeviceIndex
        {
            get => AvailableDevices.GetDeviceIndexFromUuid(DeviceToPauseUuid);
            set
            {
                var newDevice = AvailableDevices.GetDeviceUuidFromIndex(value);
                if (DeviceToPauseUuid != newDevice)
                {
                    DeviceToPauseUuid = newDevice;
                }
                OnPropertyChanged(nameof(DeviceIndex));
            }
        }
        public bool HideMiningWindowsAlertVisible => MinimizeMiningWindows && HideMiningWindows;
    }
}

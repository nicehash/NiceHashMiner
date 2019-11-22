using NHM.Common;

namespace NHMCore.Configs
{
    public class GlobalDeviceSettings : NotifyChangedBase
    {
        public static GlobalDeviceSettings Instance { get; } = new GlobalDeviceSettings();
        private GlobalDeviceSettings() { }

        private bool _runScriptOnCUDA_GPU_Lost = false;
        public bool RunScriptOnCUDA_GPU_Lost {
            get => _runScriptOnCUDA_GPU_Lost;
            set {
                _runScriptOnCUDA_GPU_Lost = value;
                OnPropertyChanged(nameof(RunScriptOnCUDA_GPU_Lost));
            }
        }

        private bool _disableDeviceStatusMonitoring = false;
        public bool DisableDeviceStatusMonitoring
        {
            get => _disableDeviceStatusMonitoring;
            set {
                _disableDeviceStatusMonitoring = value;
                OnPropertyChanged(nameof(DisableDeviceStatusMonitoring));
            }
        }

        private bool _disableDevicePowerModeSettings = true;
        public bool DisableDevicePowerModeSettings
        {
            get => _disableDevicePowerModeSettings;
            set {
                _disableDevicePowerModeSettings = value;
                OnPropertyChanged(nameof(DisableDevicePowerModeSettings));
            }
        }

        private bool _showGPUPCIeBusIDs = false;
        public bool ShowGPUPCIeBusIDs
        {
            get => _showGPUPCIeBusIDs;
            set
            {
                _showGPUPCIeBusIDs = value;
                OnPropertyChanged(nameof(ShowGPUPCIeBusIDs));
            }
        }
    }
}

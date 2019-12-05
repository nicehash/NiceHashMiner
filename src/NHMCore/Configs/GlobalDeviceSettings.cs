using NHM.Common;
using NHM.DeviceMonitoring;

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

        public bool DisableDeviceStatusMonitoring
        {
            get => DeviceMonitorManager.DisableDeviceStatusMonitoring;
            set {
                DeviceMonitorManager.DisableDeviceStatusMonitoring = value;
                OnPropertyChanged(nameof(DisableDeviceStatusMonitoring));
            }
        }

        public bool DisableDevicePowerModeSettings
        {
            get => DeviceMonitorManager.DisableDevicePowerModeSettings;
            set {
                DeviceMonitorManager.DisableDevicePowerModeSettings = value;
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

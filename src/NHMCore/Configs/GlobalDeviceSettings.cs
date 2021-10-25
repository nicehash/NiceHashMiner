using NHM.Common;
using NHM.DeviceMonitoring;

namespace NHMCore.Configs
{
    public class GlobalDeviceSettings : NotifyChangedBase
    {
        public static GlobalDeviceSettings Instance { get; } = new GlobalDeviceSettings();
        private GlobalDeviceSettings() { }

        private bool _checkForMissingGPUs = false;
        public bool CheckForMissingGPUs
        {
            get => _checkForMissingGPUs;
            set
            {
                _checkForMissingGPUs = value;
                OnPropertyChanged(nameof(CheckForMissingGPUs));
            }
        }

        private bool _restartMachineOnLostGPU = false;
        public bool RestartMachineOnLostGPU
        {
            get => _restartMachineOnLostGPU;
            set
            {
                _restartMachineOnLostGPU = value;
                OnPropertyChanged(nameof(RestartMachineOnLostGPU));
            }
        }

        public bool DisableDeviceStatusMonitoring
        {
            get => DeviceMonitorManager.DisableDeviceStatusMonitoring;
            set
            {
                DeviceMonitorManager.DisableDeviceStatusMonitoring = value;
                OnPropertyChanged(nameof(DisableDeviceStatusMonitoring));
            }
        }

        public bool DisableDevicePowerModeSettings
        {
            get => DeviceMonitorManager.DisableDevicePowerModeSettings;
            set
            {
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

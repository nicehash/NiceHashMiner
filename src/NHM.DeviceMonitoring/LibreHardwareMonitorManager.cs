using LibreHardwareMonitor.Hardware;
using NHM.Common;
using System;

namespace NHM.DeviceMonitoring
{
    class LibreHardwareMonitorManager
    {
        public static LibreHardwareMonitorManager Instance { get; } = new LibreHardwareMonitorManager();

        public Computer Computer { get; } = new Computer();

        private LibreHardwareMonitorManager()
        {
            try
            {
                Computer.Open();
                Computer.IsMotherboardEnabled = true;
                Computer.IsCpuEnabled = true;
            }
            catch (Exception e)
            {
                Logger.Error("DeviceMonitorManager", $"LibreHardwareMonitorManager Error: {e.Message}");
            }
        }

        ~LibreHardwareMonitorManager()
        {
            Computer.Close();
        }
    }
}

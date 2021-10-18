using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    class DeviceSensorsMonitor
    {
        public static DeviceSensorsMonitor Instance { get; } = new DeviceSensorsMonitor();

        public Computer computer = new Computer();

        private DeviceSensorsMonitor()
        {
            computer.Open();
            computer.IsMotherboardEnabled = true;
            computer.IsCpuEnabled = true;
        }

        ~DeviceSensorsMonitor()
        {
            computer.Close();
        }
    }
}

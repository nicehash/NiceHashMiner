using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal static class NVIDIA_MON
    {
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern void nhm_nvidia_init();
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern void nhm_nvidia_deinit();
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_memory_info(string uuid, ref ulong free, ref ulong total, ref ulong used);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_load_perc(int bus_number, ref int load_gpu);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_power_usage(string uuid, ref float power_usage);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_temperature(int bus_number, ref ulong temp);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_fan_speed_rpm(int bus_number, ref int fan_speed_rpm);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_fan_speed_percentage(string uuid, ref int fan_speed);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_set_fan_speed_percentage(int bus_number, int set_fan_speed_percentage);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_restore_fan_speed(int bus_number);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_tdp_values(string uuid, ref uint min, ref uint max, ref uint def);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_tdp(int bus_number, ref int tdp);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_set_tdp(string uuid, uint set_tdp);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_core_clocks(int bus_number, ref int core_clocks);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_set_core_clocks_delta(int bus_number, int core_clocks);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_memory_clocks(int bus_number, ref int memory_clocks);
        [DllImport("device_monitoring_nvidia.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_set_memory_clocks_delta(int bus_number, int memory_clocks);

    }
}

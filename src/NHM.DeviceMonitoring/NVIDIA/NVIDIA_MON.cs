using System.Runtime.InteropServices;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal static class NVIDIA_MON
    {
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern void nhm_nvidia_init();
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern void nhm_nvidia_deinit();
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_memory_info(string uuid, ref ulong free, ref ulong total, ref ulong used);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_load_perc(int bus_number);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_power_usage(string uuid);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern long nhm_nvidia_device_get_temperature(int bus_number);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_fan_speed_rpm(int bus_number);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_fan_speed_percentage(string uuid);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern bool nhm_nvidia_device_set_fan_speed_percentage(int bus_number, int set_fan_speed_percentage);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern bool nhm_nvidia_device_restore_fan_speed(int bus_number);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern bool nhm_nvidia_device_get_tdp_values(string uuid, ref uint min, ref uint max, ref uint def);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_tdp(int bus_number);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern bool nhm_nvidia_device_set_tdp(string uuid, uint set_tdp);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_core_clocks(int bus_number);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern bool nhm_nvidia_device_set_core_clocks_delta(int bus_number, int core_clocks);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_device_get_memory_clocks(int bus_number);
        [DllImport("nvidia_monitoring.dll", CharSet = CharSet.Ansi)]
        public static extern bool nhm_nvidia_device_set_memory_clocks_delta(int bus_number, int memory_clocks);
    }
}

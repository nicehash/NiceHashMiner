using System.Runtime.InteropServices;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal static class NVIDIA_MON
    {
        const string dll = "device_monitoring_nvidia.dll";

        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_init();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_deinit();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_restart_driver();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern bool nhm_nvidia_is_nvapi_alive();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern bool nhm_nvidia_is_nvml_alive();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_load_percentage(int bus_number, ref int get_load_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_power_usage(int bus_number, ref int get_power_usage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_temperature(int bus_number, ref ulong get_temperature);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_fan_speed_rpm(int bus_number, ref int get_fan_speed_rpm);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_fan_speed_rpm_v2(int bus_number, ref int get_fan_speed_rpm);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_fan_speed_percentage(int bus_number, ref int get_fan_speed_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_fan_speed_percentage(int bus_number, int set_fan_speed_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_tdp_min_max_default(int bus_number, ref uint min, ref uint max, ref uint defaultV);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_tdp(int bus_number, ref int get_tdp);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_tdp(int bus_number, int set_tdp);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_core_clocks(int bus_number, ref int core_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_core_clocks(int bus_number, int core_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_core_clocks_delta(int bus_number, int core_clocks_delta);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_memory_clocks(int bus_number, ref int memory_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_memory_clocks_delta(int bus_number, int memory_clocks_delta);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_memory_info(int bus_number, ref ulong free, ref ulong total, ref ulong used);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_restore_fan_speed(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_clocks_delta(int bus_number, ref int core_clock, ref int mem_clock);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_oc_limits_delta(int bus_number, ref int delta_core_min, ref int delta_core_max, ref int delta_mem_min, ref int delta_mem_max);

        //Excavator no longer has these functions (or they have been moved)
        //[DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        //public static extern int nhm_amd_device_set_memory_clocks(int bus_number, int memory_clocks);
        //[DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        //public static extern int nhm_amd_device_get_core_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);
        //[DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        //public static extern int nhm_amd_device_get_memory_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);

    }
}

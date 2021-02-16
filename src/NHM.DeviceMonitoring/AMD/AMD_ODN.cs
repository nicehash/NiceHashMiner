using System.Runtime.InteropServices;

namespace NHM.DeviceMonitoring.AMD
{
    internal static class AMD_ODN
    {
        const string dll = "device_monitoring_amd.dll";
        public delegate void log_cb(string error);
        // non ADL_RET
        [DllImport(dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int nhm_amd_reg_log_cb(log_cb cb);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int nhm_amd_set_debug_log_level(int level);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_init();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_deinit();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_has_adapter(int bus_number);

        // ADL_RET(adl_results Result Codes) || NHM_AMD_RESULT()   nhm_amd_xyz(int bus_number, ...);

        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_load_percentage(int bus_number, ref int get_load_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_power_usage(int bus_number, ref int get_power_usage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_temperature(int bus_number, ref int get_temperature);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_fan_speed_rpm(int bus_number, ref int get_fan_speed_rpm);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_fan_speed_percentage(int bus_number, ref int get_fan_speed_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_fan_speed_percentage(int bus_number, int set_fan_speed_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_tdp_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);

        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_tdp(int bus_number, ref int get_tdp);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_tdp(int bus_number, int set_tdp);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_core_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_core_clocks(int bus_number, ref int core_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_core_clocks(int bus_number, int core_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_memory_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_memory_clocks(int bus_number, ref int memory_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_memory_clocks(int bus_number, int memory_clocks);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.AMD
{
    internal static class AMD_ODN
    {
        public delegate void log_cb(string error);
        // non ADL_RET
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int nhm_amd_reg_log_cb(log_cb cb);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int nhm_amd_log(string what);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_init();
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_deinit();
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_has_adapter(int bus_number);

        // ADL_RET(adl_results Result Codes) || NHM_AMD_RESULT()   nhm_amd_xyz(int bus_number, ...);

        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_load_percentage(int bus_number, ref int get_load_percentage);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_power_usage(int bus_number, ref int get_power_usage);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_temperature(int bus_number, ref int get_temperature);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_fan_speed_rpm(int bus_number, ref int get_fan_speed_rpm);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_fan_speed_percentage(int bus_number, ref int get_fan_speed_percentage);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_fan_speed_percentage(int bus_number, int set_fan_speed_percentage);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_tdp_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);

        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_tdp(int bus_number, ref int get_tdp);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_tdp(int bus_number, int set_tdp);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_core_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_core_clocks(int bus_number, ref int core_clocks);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_core_clocks(int bus_number, int core_clocks);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_memory_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_memory_clocks(int bus_number, ref int memory_clocks);
        [DllImport("amd_adl_odn.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_memory_clocks(int bus_number, int memory_clocks);
    }
}

﻿using System.Runtime.InteropServices;

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
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_driver_version(int bus_number, ref ADLVersionsInfoX2 driverVersion);

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
        public static extern int nhm_amd_device_reset_fan_speed_percentage(int bus_number);
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
        public static extern int nhm_amd_device_reset_core_clocks(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_memory_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int defaultV);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_memory_clocks(int bus_number, ref int memory_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_memory_clocks(int bus_number, int memory_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_reset_memory_clocks(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_special_temperatures(int bus_number, ref int hotspot_temp, ref int vram_temp);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_memory_controller_load(int bus_number, ref int mem_ctrl_load);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_voltage_min_max_default(int bus_number, ref int min, ref int max, ref int default_v);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_get_voltage(int bus_number, ref int voltage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_set_voltage(int bus_number, int voltage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_amd_device_reset_voltage(int bus_number);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ADLVersionsInfoX2
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] StrDriverVer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] StrCatalystVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] StrCrimsonVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] StrCatalystWebLink;

            public ADLVersionsInfoX2(char[] strDriverVer, char[] strCatalystVersion, char[] strCrimsonVersion, char[] strCatalystWebLink)
            {
                this.StrDriverVer = strDriverVer;
                this.StrCatalystVersion = strCatalystVersion;
                this.StrCrimsonVersion = strCrimsonVersion;
                this.StrCatalystWebLink = strCatalystWebLink;
            }
        }
    }
}

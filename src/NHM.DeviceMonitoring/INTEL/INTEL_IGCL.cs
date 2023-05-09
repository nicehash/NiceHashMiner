using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.INTEL
{
    internal static class INTEL_IGCL
    {
        const string dll = "device_monitoring_intel.dll";
        public delegate void log_cb(string error);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int nhm_intel_reg_log_cb(log_cb cb);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int nhm_intel_set_debug_log_level(int level);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_init();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_deinit();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_load_percentage(int bus_number, ref int get_load_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_temperature(int bus_number, ref int get_temperature);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_vram_temperature(int bus_number, ref int get_vram_temperature);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_fan_speed_rpm(int bus_number, ref int get_fan_speed_rpm);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_set_fan_speed_rpm(int bus_number, int set_fan_speed_rpm);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_reset_fan_speed_rpm(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_core_clocks_min_max_default_delta(int bus_number, ref int min, ref int max, ref int def);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_core_clocks(int bus_number, ref int get_core_clock);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_core_clocks_delta(int bus_number, ref int get_core_clock_delta);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_set_core_clocks_delta(int bus_number, int set_core_clock_delta_delta);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_set_locked_core_clocks(int bus_number, int set_locked_core_clock, int set_voltage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_reset_locked_core_clocks(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_reset_core_clocks_delta(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_memory_clocks_min_max_default_delta(int bus_number, ref int min, ref int max, ref int def);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_memory_clocks(int bus_number, ref int get_memory_clock);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_memory_clocks_delta(int bus_number, ref int get_memory_clock_delta);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_set_memory_clocks_delta(int bus_number, int set_memory_clock_delta);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_reset_memory_clocks_delta(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_core_voltage_min_max_default_delta(int bus_number, ref int min, ref int max, ref int def);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_core_voltage(int bus_number, ref int get_core_voltage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_core_voltage_delta(int bus_number, ref int get_core_voltage_delta);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_set_core_voltage_delta(int bus_number, int set_core_voltage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_reset_core_voltage_delta(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_power_limit_min_max_default(int bus_number, ref int min, ref int max, ref int def);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_power_usage(int bus_number, ref int get_power_usage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_get_power_limit(int bus_number, ref int get_power_limit);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_set_power_limit(int bus_number, int set_power_limit);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_intel_device_reset_power_limit(int bus_number);
    }
}

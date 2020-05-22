using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace NHMCore.Scripts
{
    public static class LibJSBridge
    {

        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern Int64 nhms_init_runtime_and_context();
        
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern Int64 nhms_commit_javascript_callbacks_to_context();

        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern Int64 nhms_de_init_runtime_and_context();

        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern Int64 nhms_add_js_script(string js_script_code);

        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern Int64 nhms_add_switching_js_script(string js_script_code);

        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern Int64 nhms_remove_js_script(Int64 script_id);

        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern Int64 nhms_js_tick();


        public delegate void js_error_cb(string error, string stack, Int64 script_id);

        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern Int64 nhms_reg_unhandeled_js_error_cb(js_error_cb err_cb);


        public delegate void error_log_cb(string error);

        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern Int64 nhms_reg_runtime_error_log_cb(error_log_cb cb);


        /// JavaScript callbacks registration functions
        public delegate Int64 nhms_protobuf_in_out_cb(IntPtr buffer, long in_size, ref long out_size);
        // out:OutDevicesInfo
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_get_devices_info(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_get_devices_info(nhms_protobuf_in_out_cb cb) => nhms_reg_js_get_devices_info(CreateHandler(nameof(nhms_reg_js_get_devices_info), cb));

        // in:InGetDeviceInfo, out:OutGetDeviceInfoResult
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_get_device_info(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_get_device_info(nhms_protobuf_in_out_cb cb) => nhms_reg_js_get_device_info(CreateHandler(nameof(nhms_reg_js_get_device_info), cb));

        // in:InOutDeviceFanSpeedRPM, out:DeviceSetResult
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_set_device_fan_speed(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_set_device_fan_speed(nhms_protobuf_in_out_cb cb) => nhms_reg_js_set_device_fan_speed(CreateHandler(nameof(nhms_reg_js_set_device_fan_speed), cb));

        // out:SMAInfo
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_get_sma_data(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_get_sma_data(nhms_protobuf_in_out_cb cb) => nhms_reg_js_get_sma_data(CreateHandler(nameof(nhms_reg_js_get_sma_data), cb));

        // out:DevicesAlgorithms
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_get_devices_algorithm_info(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_get_devices_algorithm_info(nhms_protobuf_in_out_cb cb) => nhms_reg_js_get_devices_algorithm_info(CreateHandler(nameof(nhms_reg_js_get_devices_algorithm_info), cb));

        // utils javascript console.log
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_console_print(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_console_print(nhms_protobuf_in_out_cb cb) => nhms_reg_js_console_print(CreateHandler(nameof(nhms_reg_js_console_print), cb));

        // in:UpdateDeviceMiningState, out:DeviceSetResult
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_update_device_mining_state(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_update_device_mining_state(nhms_protobuf_in_out_cb cb) => nhms_reg_js_update_device_mining_state(CreateHandler(nameof(nhms_reg_js_update_device_mining_state), cb));

        // in:SetDeviceEnabledState, out:DeviceSetResult
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_set_device_enabled_state(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_set_device_enabled_state(nhms_protobuf_in_out_cb cb) => nhms_reg_js_set_device_enabled_state(CreateHandler(nameof(nhms_reg_js_set_device_enabled_state), cb));


        // in:StartDevice, out:DeviceSetResult
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_start_device(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_start_device(nhms_protobuf_in_out_cb cb) => nhms_reg_js_start_device(CreateHandler(nameof(nhms_reg_js_start_device), cb));

        // in:StopDevice, out:DeviceSetResult
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_stop_device(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_stop_device(nhms_protobuf_in_out_cb cb) => nhms_reg_js_stop_device(CreateHandler(nameof(nhms_reg_js_stop_device), cb));

        // in:SetDeviceMinerAlgorithmPairEnabledState, out:DeviceSetResult
        [DllImport("libnhms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int64 nhms_reg_js_set_device_miner_algorithm_pair_enabled_state(nhms_protobuf_in_out_cb cb);
        internal static void bridge_nhms_reg_js_set_device_miner_algorithm_pair_enabled_state(nhms_protobuf_in_out_cb cb) => nhms_reg_js_set_device_miner_algorithm_pair_enabled_state(CreateHandler(nameof(nhms_reg_js_set_device_miner_algorithm_pair_enabled_state), cb));

        // keep callbacks in memory
        private static readonly Dictionary<string, Delegate> _callbacks = new Dictionary<string, Delegate> { };
        public static T CreateHandler<T>(string key, T func) where T : Delegate
        {
            // remove 
            var setKey = key.Replace("register_", "");
            if (_callbacks.ContainsKey(setKey)) throw new Exception($"NHMCore.Scripts.LibJSBridge+CreateHandler setKey '{setKey}' allready exists. Use a different jsFuncName.");
            T cb = func;
            // Ensure it doesn't get garbage collected
            _callbacks.Add(setKey, cb);
            return cb;
        }

    }
}

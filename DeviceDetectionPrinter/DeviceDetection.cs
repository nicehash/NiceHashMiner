using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeviceDetectionPrinter
{
    static class DeviceDetection
    {
        [DllImport("cuda_device_detection.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _GetCUDADevices(bool prettyString);

        public static string GetCUDADevices(bool prettyString)
        {
            try
            {
                var a = _GetCUDADevices(prettyString);
                var ret = Marshal.PtrToStringAnsi(a);
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error calling CUDA get error: {e.Message}");
            }
            return "";
        }

        [DllImport("opencl_device_detection.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _GetOpenCLDevices(bool prettyString);

        public static string GetOpenCLDevices(bool prettyString)
        {
            try
            {
                var a = _GetOpenCLDevices(prettyString);
                var ret = Marshal.PtrToStringAnsi(a);
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error calling OpenCL get error: {e.Message}");
            }
            return "";
        }
    }
}

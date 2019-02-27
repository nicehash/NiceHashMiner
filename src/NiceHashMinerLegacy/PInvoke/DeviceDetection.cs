using System;
using System.Runtime.InteropServices;

namespace NiceHashMiner.PInvoke
{
    public static class DeviceDetection
    {
        [DllImport("cuda_device_detection.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _GetCUDADevices(bool prettyString);

        /// <summary>
        /// GetCUDADevices returns CUDA device json string. Can throw Exception
        /// </summary>
        /// <returns></returns>
        public static string GetCUDADevices()
        {
            var a = _GetCUDADevices(false);
            var ret = Marshal.PtrToStringAnsi(a);
            return ret;
        }

        [DllImport("opencl_device_detection.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _GetOpenCLDevices(bool prettyString);


        /// <summary>
        /// GetOpenCLDevices returns OpenCL device json string. Can throw Exception
        /// </summary>
        /// <returns></returns>
        public static string GetOpenCLDevices()
        {
            var a = _GetOpenCLDevices(false);
            var ret = Marshal.PtrToStringAnsi(a);
            return ret;
        }
    }
}

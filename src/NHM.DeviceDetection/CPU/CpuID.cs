using System;
using System.Runtime.InteropServices;

namespace NHM.DeviceDetection.CPU
{
    internal static class CpuID
    {
        const string dll = "device_detection_cpu.dll"; 
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _GetCPUName();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _GetCPUVendor();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SupportsSSE2();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SupportsAVX();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SupportsAVX2();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SupportsAES();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPhysicalProcessorCount();

        public static string GetCpuName()
        {
            var a = _GetCPUName();
            return Marshal.PtrToStringAnsi(a);
        }

        //public static string GetCpuVendor()
        //{
        //    var a = _GetCPUVendor();
        //    return Marshal.PtrToStringAnsi(a);
        //}
    }
}

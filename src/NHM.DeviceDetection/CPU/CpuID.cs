using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.CPU
{
    internal static class CpuID
    {
        [DllImport("cpuid.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _GetCPUName();

        [DllImport("cpuid.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _GetCPUVendor();

        [DllImport("cpuid.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SupportsSSE2();

        [DllImport("cpuid.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SupportsAVX();

        [DllImport("cpuid.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SupportsAVX2();

        [DllImport("cpuid.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SupportsAES();

        [DllImport("cpuid.dll", CallingConvention = CallingConvention.Cdecl)]
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

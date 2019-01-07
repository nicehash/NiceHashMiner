using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NiceHashMiner
{
    public class CpuID
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

        public static string GetCpuVendor()
        {
            var a = _GetCPUVendor();
            return Marshal.PtrToStringAnsi(a);
        }

        public static int GetVirtualCoresCount()
        {
            var coreCount = 0;

            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
            {
                coreCount += int.Parse(item["NumberOfLogicalProcessors"].ToString());
            }

            return coreCount;
        }

        public static int GetNumberOfCores() {
            var coreCount = 0;

            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get()) {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            return coreCount;
        }

        public static bool IsHypeThreadingEnabled() {
            return GetVirtualCoresCount() > GetNumberOfCores();
        }

        public static ulong CreateAffinityMask(int index, int percpu)
        {
            ulong mask = 0;
            const ulong one = 0x0000000000000001;
            for (var i = index * percpu; i < (index + 1) * percpu; i++)
                mask = mask | (one << i);
            return mask;
        }

        public static void AdjustAffinity(int pid, ulong mask)
        {
            var processHandle = new Process
            {
                StartInfo =
                {
                    FileName = "setcpuaff.exe",
                    Arguments = pid + " " + mask,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            processHandle.Start();
        }
    }
}

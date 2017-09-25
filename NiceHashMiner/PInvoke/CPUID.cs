using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NiceHashMiner
{
    class CPUID
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

        public static string GetCPUName()
        {
            IntPtr a = _GetCPUName();
            return Marshal.PtrToStringAnsi(a);
        }

        public static string GetCPUVendor()
        {
            IntPtr a = _GetCPUVendor();
            return Marshal.PtrToStringAnsi(a);
        }

        public static int GetVirtualCoresCount()
        {
            int coreCount = 0;

            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
            {
                coreCount += int.Parse(item["NumberOfLogicalProcessors"].ToString());
            }

            return coreCount;
        }

        public static int GetNumberOfCores() {
            int coreCount = 0;

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
            ulong one = 0x0000000000000001;
            for (int i = index * percpu; i < (index + 1) * percpu; i++)
                mask = mask | (one << i);
            return mask;
        }

        #region ProcessAffinity
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetProcessAffinityMask(IntPtr hProcess, UIntPtr dwProcessAffinityMask);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        public static void AdjustAffinity(int pid, ulong mask)
        {
            IntPtr handle = OpenProcess(ProcessAccessFlags.SetInformation, false, pid);
            SetProcessAffinityMask(handle, (UIntPtr)mask);
            CloseHandle(handle);
        }
        #endregion
    }
}

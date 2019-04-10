using System;
using System.Runtime.InteropServices;

namespace MinerPluginToolkitV1
{
    public static class ProcessHelpers
    {

        // TODO make sure to separate platform specific code
        // #if Windows
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(Int32 dwDesiredAccess, int bInheritHandle, Int32 dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessAffinityMask(IntPtr hProcess, UIntPtr dwProcessAffinityMask);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        public static Tuple<bool, string> AdjustAffinity(int pid, ulong mask)
        {

            // DWORD pid;
            // DWORD_PTR mask;
            // HANDLE hProc;

            // if (argc < 3)
            // {
            //     printf("Usage: %s [PID] [AFFINITY]\n", argv[0]);
            //     return 0;
            // }

            // pid = atoi(argv[1]);
            // mask = atoll(argv[2]);

            // printf("PID=%u, MASK=%llu\n", pid, mask);

            const int FALSE = 0;
            const int PROCESS_SET_INFORMATION = 0x0200;
            var hProc = OpenProcess(PROCESS_SET_INFORMATION, FALSE, pid);
            if (hProc == IntPtr.Zero)
            {
                return Tuple.Create(false, $"Error opening process, code={GetLastError()}");
            }

            if (!SetProcessAffinityMask(hProc, new UIntPtr(mask)))
            {
                return Tuple.Create(false, $"Error setting affinity, code={GetLastError()}");
            }
            CloseHandle(hProc);

            return Tuple.Create(true, "Affinity adjusted");
        }
        // #else

        // #endif
    }
}

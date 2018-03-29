using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
//ReSharper disable All
#pragma warning disable

namespace NiceHashMiner
{
    public class NiceHashProcess
    {
        private const uint CREATE_NEW_CONSOLE = 0x00000010;
        private const uint NORMAL_PRIORITY_CLASS = 0x0020;
        private const uint CREATE_NO_WINDOW = 0x08000000;
        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const short SW_SHOWMINNOACTIVE = 7;
        private const uint INFINITE = 0xFFFFFFFF;
        private const uint STILL_ACTIVE = 259;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public Int32 ProcessId;
            public Int32 ThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe,
            ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        // ctrl+c
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("Kernel32", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        [DllImportAttribute("kernel32.dll", EntryPoint = "AllocConsole")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        // A delegate type to be used as the handler routine 
        // for SetConsoleCtrlHandler.
        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        public delegate void ExitEventDelegate();

        public ProcessStartInfo StartInfo;
        public ExitEventDelegate ExitEvent;
        public uint ExitCode;
        public int Id;

        private Thread _tHandle;
        private bool _bRunning;
        private IntPtr _pHandle;

        public NiceHashProcess()
        {
            StartInfo = new ProcessStartInfo();
        }

        public bool Start()
        {
            var pInfo = new PROCESS_INFORMATION();
            var sInfo = new STARTUPINFO();
            var pSec = new SECURITY_ATTRIBUTES();
            var tSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            uint sflags;
            if (StartInfo.CreateNoWindow)
            {
                sflags = CREATE_NO_WINDOW;
            }
            else if (StartInfo.WindowStyle == ProcessWindowStyle.Minimized)
            {
                sInfo.dwFlags = STARTF_USESHOWWINDOW;
                sInfo.wShowWindow = SW_SHOWMINNOACTIVE;
                sflags = CREATE_NEW_CONSOLE;
            }
            else
            {
                sflags = CREATE_NEW_CONSOLE;
            }

            string workDir = null;
            if (StartInfo.WorkingDirectory.Length > 0)
                workDir = StartInfo.WorkingDirectory;

            var res = CreateProcess(StartInfo.FileName,
                " " + StartInfo.Arguments,
                ref pSec,
                ref tSec,
                false,
                sflags | NORMAL_PRIORITY_CLASS,
                IntPtr.Zero,
                workDir,
                ref sInfo,
                out pInfo);

            if (!res)
            {
                var err = Marshal.GetLastWin32Error();
                throw new Exception("Failed to start process, err=" + err);
            }

            CloseHandle(sInfo.hStdError);
            CloseHandle(sInfo.hStdInput);
            CloseHandle(sInfo.hStdOutput);

            _pHandle = pInfo.hProcess;
            CloseHandle(pInfo.hThread);

            Id = pInfo.ProcessId;

            if (ExitEvent != null)
            {
                _bRunning = true;
                _tHandle = new Thread(CThread);
                _tHandle.Start();
            }

            return true;
        }

        public void Kill()
        {
            if (_pHandle == IntPtr.Zero) return;

            if (_tHandle != null)
            {
                _bRunning = false;
                _tHandle.Join();
            }

            TerminateProcess(_pHandle, 0);
            CloseHandle(_pHandle);
            _pHandle = IntPtr.Zero;
        }

        public void Close()
        {
            if (_pHandle == IntPtr.Zero) return;

            if (_tHandle != null)
            {
                _bRunning = false;
                _tHandle.Join();
            }

            CloseHandle(_pHandle);
            _pHandle = IntPtr.Zero;
        }

        private bool SignalCtrl(uint thisConsoleId, uint dwProcessId, CtrlTypes dwCtrlEvent)
        {
            var success = false;
            //uint thisConsoleId = GetCurrentProcessId();
            // Leave current console if it exists
            // (otherwise AttachConsole will return ERROR_ACCESS_DENIED)
            var consoleDetached = FreeConsole();

            if (AttachConsole(dwProcessId))
            {
                // Add a fake Ctrl-C handler for avoid instant kill is this console
                // WARNING: do not revert it or current program will be also killed
                SetConsoleCtrlHandler(null, true);
                success = GenerateConsoleCtrlEvent(dwCtrlEvent, 0);
                FreeConsole();
                // wait for termination so we don't terminate NHM
                WaitForSingleObject(_pHandle, 10000);
            }

            if (consoleDetached)
            {
                // Create a new console if previous was deleted by OS
                if (AttachConsole(thisConsoleId) == false)
                {
                    var errorCode = GetLastError();
                    if (errorCode == 31)
                    {
                        // 31=ERROR_GEN_FAILURE
                        AllocConsole();
                    }
                }
                SetConsoleCtrlHandler(null, false);
            }
            return success;
        }

        public void SendCtrlC(uint thisConsoleId)
        {
            if (_pHandle == IntPtr.Zero) return;

            if (_tHandle != null)
            {
                _bRunning = false;
                _tHandle.Join();
            }
            SignalCtrl(thisConsoleId, (uint) Id, CtrlTypes.CTRL_C_EVENT);
            _pHandle = IntPtr.Zero;
        }

        private void CThread()
        {
            while (_bRunning)
            {
                if (WaitForSingleObject(_pHandle, 10) == 0)
                {
                    GetExitCodeProcess(_pHandle, out ExitCode);
                    if (ExitCode != STILL_ACTIVE)
                    {
                        CloseHandle(_pHandle);
                        _pHandle = IntPtr.Zero;
                        ExitEvent();
                        return;
                    }
                }
            }
        }
    }
}

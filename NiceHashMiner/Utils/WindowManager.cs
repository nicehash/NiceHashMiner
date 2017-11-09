using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NiceHashMiner.Utils
{
    class WindowManager
    {
        // Move window to specified coordinates
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        // Find window location and dimensions
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        // Rectangle structure used by GetWindowRect pinvoke call
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static Point GetMonitorCorrectedFormPoints(int monitorNum, int windowX, int windowY)
        {
            Point retVal = new Point(windowX, windowX);
            if (monitorNum > -1)
            {
                Screen[] myScreens = Screen.AllScreens;
                if (myScreens.GetLength(0) >= monitorNum)
                {
                    retVal = new Point(Screen.AllScreens[monitorNum].WorkingArea.X + windowX,
                        Screen.AllScreens[monitorNum].WorkingArea.Y + windowY);
                }
            }
            return retVal;
        }

        public static Point GetCenteredOnMonitorFormPoints(int monitorNum, int windowWidth, int windowHeight)
        {
            Point retVal = new Point(0, 0);
            if (monitorNum < 0) monitorNum = 0;

            Screen[] myScreens = Screen.AllScreens;
            if (myScreens.GetLength(0) >= monitorNum)
            {
                int StartPosX = Screen.AllScreens[monitorNum].WorkingArea.X;
                StartPosX = StartPosX + (Screen.AllScreens[monitorNum].WorkingArea.Width / 2);
                StartPosX = StartPosX - (windowWidth / 2);
                int StartPosY = Screen.AllScreens[monitorNum].WorkingArea.Y;
                StartPosY = StartPosY + (Screen.AllScreens[monitorNum].WorkingArea.Height / 2);
                StartPosY = StartPosY - (windowHeight / 2);
                retVal = new Point(StartPosX, StartPosY);
            }
            return retVal;
        }

        public static void MoveConsoleWindow(int processId, int monitorNum, int xCoord, int yCoord)
        {
            Process consoleProcess = Process.GetProcessById(processId);
            if (consoleProcess != null)
            {
                RECT rect = new RECT();
                int chkCtr = 0;
                do
                {
                    // Get current console window location and dimensions, all zeros until window is initialized. Loop is 2500ms
                    GetWindowRect(consoleProcess.MainWindowHandle, out rect);
                    if (!(rect.Left == 0 && rect.Right == 0 && rect.Top == 0 && rect.Bottom == 0)) break;
                    Thread.Sleep(50);
                    chkCtr++;
                } while (chkCtr < 50);
                // If we couldn't get reliable coordinates it may have not started or crashed, don't continue
                if (rect.Left == 0 && rect.Right == 0 && rect.Top == 0 && rect.Bottom == 0) return;

                // If the monitor number is <= -1, we assume the primary monitor is used
                if (monitorNum < 0) monitorNum = 0;
                // If the x or y coordinates are <= -1, then we inherit the window's current x/y coordinates
                if (xCoord < 0) xCoord = rect.Left;
                if (yCoord < 0) yCoord = rect.Top;

                Screen[] myScreens = Screen.AllScreens;
                if (myScreens.Count() > monitorNum)
                {
                    // Check coordinates against screen boundaries, correct if needed
                    if (xCoord + (rect.Right - rect.Left) > myScreens[monitorNum].Bounds.Width)
                        xCoord = xCoord - ((xCoord + (rect.Right - rect.Left)) - myScreens[monitorNum].Bounds.Width);
                    if (yCoord + (rect.Bottom - rect.Top) > myScreens[monitorNum].Bounds.Height)
                        yCoord = yCoord - ((yCoord + (rect.Bottom - rect.Top)) - myScreens[monitorNum].Bounds.Height);
                    // Apply config values as offset on screen boundaries
                    xCoord = xCoord + myScreens[monitorNum].Bounds.X;
                    yCoord = yCoord + myScreens[monitorNum].Bounds.Y;
                }
                // Move window to specified coordinates
                MoveWindow(consoleProcess.MainWindowHandle, xCoord, yCoord, (rect.Right - rect.Left), (rect.Bottom - rect.Top), true);
            }
        }
    }
}

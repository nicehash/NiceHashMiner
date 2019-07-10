using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NiceHashMiner.Forms
{
    // https://stackoverflow.com/questions/24695976/resize-system-icon-in-c-sharp
    class IconEx : IDisposable
    {
        public enum SystemIcons
        {
            Application = 100,
            Asterisk = 104,
            Error = 103,
            Exclamation = 101,
            Hand = 103,
            Information = 104,
            Question = 102,
            Shield = 106,
            Warning = 101,
            WinLogo = 100
        }

        public IconEx(string path, Size size)
        {
            IntPtr hIcon = LoadImage(IntPtr.Zero, path, IMAGE_ICON, size.Width, size.Height, LR_LOADFROMFILE);
            if (hIcon == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
            attach(hIcon);

        }
        public IconEx(SystemIcons sysicon, Size size)
        {
            IntPtr hUser = GetModuleHandle("user32");
            IntPtr hIcon = LoadImage(hUser, (IntPtr)sysicon, IMAGE_ICON, size.Width, size.Height, 0);
            if (hIcon == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
            attach(hIcon);
        }


        public Icon Icon
        {
            get { return this.icon; }
        }

        public void Dispose()
        {
            if (icon != null) icon.Dispose();
        }

        private Icon icon;

        private void attach(IntPtr hIcon)
        {
            // Invoke the private constructor so we can get the Icon object to own the handle
            var ci = typeof(Icon).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                null, new Type[] { typeof(IntPtr), typeof(bool) }, null);
            this.icon = (Icon)ci.Invoke(new object[] { hIcon, true });
        }

        private const int IMAGE_ICON = 1;
        private const int LR_LOADFROMFILE = 0x10;
        private const int LR_SHARED = 0x8000;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, int uType,
                                     int cxDesired, int cyDesired, int fuLoad);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadImage(IntPtr hinst, IntPtr resId, int uType,
                                     int cxDesired, int cyDesired, int fuLoad);
    }
}

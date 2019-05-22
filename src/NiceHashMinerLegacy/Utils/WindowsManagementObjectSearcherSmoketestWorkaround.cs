using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Utils
{
    public static class WindowsManagementObjectSearcherSmoketestWorkaround
    {
        public static void Init()
        {
            WindowsManagementObjectSearcher.QueryWin32_OperatingSystemData();
            WindowsManagementObjectSearcher.QueryWin32_VideoController();
        }
    }
}

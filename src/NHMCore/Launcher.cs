using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore
{
    public static class Launcher
    {
        public static bool IsLauncher { get; private set; } = false;

        public static void SetIsLauncher(bool isLauncher)
        {
            IsLauncher = isLauncher;
        }
    }
}

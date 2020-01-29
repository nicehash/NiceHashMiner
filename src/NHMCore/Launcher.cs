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
        public static bool IsUpdated { get; private set; } = false;
        public static bool IsUpdatedFailed { get; private set; } = false;

        public static void SetIsLauncher(bool isLauncher)
        {
            IsLauncher = isLauncher;
        }

        public static void SetIsUpdated(bool isUpdated)
        {
            IsUpdated = isUpdated;
        }

        public static void SetIsUpdatedFailed(bool isUpdatedFailed)
        {
            IsUpdatedFailed = isUpdatedFailed;
        }
    }
}

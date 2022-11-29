using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs.Managers
{
    public static class BundleManager
    {
        private static string BundleName = string.Empty;

        private static string BundleID = string.Empty;
        public static void SetBundleInfo(string name, string id)
        {
            BundleName = name; BundleID = id;
        }
        public static (string BundleName, string BundleID) GetBundleInfo()
        {
            return (BundleName, BundleID);
        }
        public static void ResetBundleInfo()
        {
            BundleName = string.Empty;
            BundleID = string.Empty;
        }
    }
}

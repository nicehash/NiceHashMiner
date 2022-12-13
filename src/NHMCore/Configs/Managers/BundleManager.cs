using Newtonsoft.Json;
using NHM.Common;
using NHMCore.Nhmws.V4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs.Managers
{
    public static class BundleManager
    {
        private static readonly string _TAG = "BundleManager";
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
        public static void Init()
        {
            var path = Paths.AppRootPath("bundle.json");
            if (!File.Exists(path)) File.Create(path);
            var content = File.ReadAllText(path);
            try
            {
                var bundleToApply = JsonConvert.DeserializeObject<Bundle>(content);
                if(bundleToApply != null)
                {
                    SetBundleInfo(bundleToApply.Name, bundleToApply.Id);
                    ApplyBundleOnInit(bundleToApply);
                }
            }
            catch(Exception e)
            {
                Logger.Error(_TAG, e.Message);
                File.WriteAllText(path, string.Empty);
            }
        }
        private static void ApplyBundleOnInit(Bundle bundle)
        {
            OCManager.Instance.ApplyOcBundle(bundle.OcBundles);
            FanManager.Instance.ApplyFanBundle(bundle.FanBundles);
            ELPManager.Instance.ApplyELPBundle(bundle.ElpBundles);
        }
        public static async Task SaveBundle(Bundle bundle)
        {
            var path = Paths.AppRootPath("bundle.json");
            var text = JsonConvert.SerializeObject(bundle);
            await File.AppendAllTextAsync(path, text);
        }
    }
}

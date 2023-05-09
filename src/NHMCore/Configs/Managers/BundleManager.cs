using Newtonsoft.Json;
using NHM.Common;
using NHMCore.ApplicationState;
using NHMCore.Mining;
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
        private static string _path = Paths.AppRootPath("bundle.json");
        public static void SetBundleInfo(string name, string id)
        {
            BundleName = name; 
            BundleID = id;
        }
        public static (string BundleName, string BundleID) GetBundleInfo()
        {
            return (BundleName, BundleID);
        }
        public static void ResetBundleInfo()
        {
            BundleName = string.Empty;
            BundleID = string.Empty;
            try
            {
                File.WriteAllText(_path, string.Empty);
            }
            catch (Exception e)
            {
                Logger.Error(_TAG, e.Message);
            }
        }
        public static void Init()
        {
            if (!File.Exists(_path))
            {
                File.Create(_path);
                return;
            }
            try
            {
                var content = File.ReadAllText(_path);
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
                File.WriteAllText(_path, string.Empty);
            }
        }
        private static void ApplyBundleOnInit(Bundle bundle)
        {
            OCManager.Instance.ApplyOcBundle(bundle.OcBundles);
            FanManager.Instance.ApplyFanBundle(bundle.FanBundles);
            ELPManager.Instance.ApplyELPBundle(bundle.ElpBundles);
            MiningState.Instance.CalculateDevicesStateChange();
        }
        public static async Task SaveBundle(Bundle bundle)
        {
            var text = JsonConvert.SerializeObject(bundle);//todo not saving
            await File.AppendAllTextAsync(_path, text);
        }
        public static List<string> FindTargetGPUNames(string bundleGPU)
        {
            var retGPU = bundleGPU;
            var potentialTargets = AvailableDevices.Devices.Where(d => d.Name.ToLower().Contains(retGPU.ToLower()));
            if (potentialTargets == null) return new() { bundleGPU };
            //order matters
            if (bundleGPU.ToLower().Contains("laptop gpu"))
            {
                return potentialTargets.Where(d => d.Name.ToLower().Contains("laptop gpu")).Select(d => d.Name.ToLower())?.ToList();
            }
            if (bundleGPU.ToLower().Contains("ti"))
            {
                return potentialTargets.Where(d => d.Name.ToLower().Contains("ti")).Select(d => d.Name.ToLower())?.ToList();
            }
            if (bundleGPU.ToLower().Contains("super"))
            {
                return potentialTargets.Where(d => d.Name.ToLower().Contains("super")).Select(d => d.Name.ToLower())?.ToList();
            }
            if (bundleGPU.ToLower().Contains("xtx"))
            {
                return potentialTargets.Where(d => d.Name.ToLower().Contains("xtx")).Select(d => d.Name.ToLower())?.ToList();
            }
            if (bundleGPU.ToLower().Contains("xt"))
            {
                return potentialTargets.Where(d => d.Name.ToLower().Contains("xt")).Select(d => d.Name.ToLower())?.ToList();
            }
            if(bundleGPU.ToLower().Contains("collectors edition"))
            {
                return potentialTargets.Where(d => d.Name.ToLower().Contains("collectors edition")).Select(d => d.Name.ToLower())?.ToList();
            }
            return potentialTargets.Where(d => !d.Name.ToLower().Contains("laptop gpu") &&
                                               !d.Name.ToLower().Contains("ti") &&
                                               !d.Name.ToLower().Contains("super") &&
                                               !d.Name.ToLower().Contains("xtx") &&
                                               !d.Name.ToLower().Contains("xt") &&
                                               !d.Name.ToLower().Contains("collectors edition"))
                                            .Select(d => d.Name.ToLower())?
                                            .ToList();
        }
    }
}

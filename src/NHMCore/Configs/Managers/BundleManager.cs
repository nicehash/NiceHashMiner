﻿using Newtonsoft.Json;
using NHM.Common;
using NHMCore.ApplicationState;
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
            var text = JsonConvert.SerializeObject(bundle);
            await File.AppendAllTextAsync(_path, text);
        }
    }
}
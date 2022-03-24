using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHM.Common;
using NHM.Common.Device;
using NHMCore.Mining;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NHMCore.Utils
{
    public static class GPUProfileManager
    {
        public static readonly int NDEF = Int32.MinValue;
        private static readonly string Tag = "GPUProfileManager";
        private static bool TriedInit = false;
        private static bool Success = false;
        public static Profiles ProfileData = null;
        public static readonly int[] ExistingProfiles = { 1, 2, 3, 4, 11, 12, 101 };

        public static bool CanUseProfiles
        {
            get { return TriedInit && Success; }
        }
        public static void Init()
        {
            if (!TriedInit)
            {
                TriedInit = true;
            }
            else return;
            try
            {
                string path = Paths.RootPath("app/");
                using (var sr = new StreamReader(path + "GPUprofiles.json"))
                {
                    var raw = sr.ReadToEnd();
                    ProfileData = JsonConvert.DeserializeObject<Profiles>(raw);
                    if (ProfileData != null) Success = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, ex.Message);
            }
        }

        public static bool GetProfileForSelectedGPUIfExists(string gpuName, int profile, out OptimizationProfile retProfile)
        {
            retProfile = null;
            if (!Success) return false;
            if (ProfileData.devices == null) return false;
            try
            {
                var devs = ProfileData.devices
                    .Where(dev => dev.name != null)
                    .Where(dev => (gpuName.Contains(dev.name)
                    || dev.name.Contains(gpuName)))
                    .FirstOrDefault();
                if (devs == null || devs.op == null) return false;
                var profiles = devs.op
                    .Where(prof => prof.id == profile)
                    .FirstOrDefault();
                if (profiles == null) return false;
                retProfile = profiles;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, ex.Message);
            }
            return false;
        }

        public static string BuildMTString(OptimizationProfile prof)
        {
            var mtString = "";
            if (prof.mt != null)
                prof.mt.ForEach(mtOpt =>
                {
                    if (mtOpt.Count == 2) mtString += mtOpt[0] + "=" + mtOpt[1] + ";";
                });
            mtString = mtString.Trim(';');
            return mtString;
        }

#region serialization
        public class Profiles
        {
            public int data_version { get; set; }
            public int iteration { get; set; }
            public List<OpName> op_names { get; set; }
            public List<Device> devices { get; set; }
        }
        public class OpName
        {
            public string id { get; set; }
            public string name { get; set; }
        }
        public class Device
        {
            public string name { get; set; }
            public List<OptimizationProfile> op { get; set; }
            public List<string> pci_devs { get; set; }
        }
        public class OptimizationProfile
        {
            public int id { get; set; } = NDEF;
            public int pt { get; set; } = NDEF;
            public int dmc { get; set; } = NDEF;
            public int dcc { get; set; } = NDEF;
            public int mmc { get; set; } = NDEF;
            public int mcc { get; set; } = NDEF;
            public int mcv { get; set; } = NDEF;
            public int pl { get; set; } = NDEF;
            public int fm { get; set; } = NDEF;
            public int ftg { get; set; } = NDEF;
            public string rm { get; set; } = "";
            public List<List<string>> mt { get; set; }
            public List<float> lhr { get; set; }
        }
    }
#endregion serialization
}

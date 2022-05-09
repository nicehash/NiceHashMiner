using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHM.Common;
using System.IO;
using System.Linq;
using NHM.MinerPluginToolkitV1.Interfaces;
using NHM.MinerPlugin;
using NHMCore.Mining;
using NHM.Common.Device;
using NHMCore.Configs;
using NHM.Common.Enums;

namespace NHMCore.Utils
{
    public class GPUProfileManager : NotifyChangedBase, IBackgroundService
    {
        private GPUProfileManager() { }
        private static readonly int NDEF = Int32.MinValue;
        private static readonly int RET_OK = 0;
        private readonly string Tag = "GPUProfileManager";
        private bool TriedInit = false;
        private bool SuccessInit = false;
        private Profiles ProfileData = null;
        public readonly int[] ExistingProfiles = { 1, 2, 3, 4, 11, 12, 101 };
        public bool IsSystemElevated => Helpers.IsElevated;
        private bool _systemContainsSupportedDevices = false;
        public bool SystemContainsSupportedDevices => _systemContainsSupportedDevices;
        public bool SystemContainsSupportedDevicesNotSystemElevated => SystemContainsSupportedDevices && !IsSystemElevated;
        public bool ServiceEnabled { get; set; } = false;
        private static object _startStopLock = new object();
        public static GPUProfileManager Instance { get; } = new GPUProfileManager();
        public List<string> SupportedDeviceNames { get; set; } = new List<string> { "1080", "1080 ti", "titan xp"};
        public void Init()
        {
            if (TriedInit) return;
            TriedInit = true;
            try
            {
                using var sr = new StreamReader(Paths.AppRootPath("GPUprofiles.json"));
                var raw = sr.ReadToEnd();
                ProfileData = JsonConvert.DeserializeObject<Profiles>(raw);
                if (ProfileData != null) SuccessInit = true;
                if (SuccessInit)
                {
                    _systemContainsSupportedDevices = AvailableDevices.Devices.Any(dev => IsSupportedDeviceName(dev.Name));
                    OnPropertyChanged(nameof(SystemContainsSupportedDevices));
                    OnPropertyChanged(nameof(SystemContainsSupportedDevicesNotSystemElevated));
                }
                Logger.Info(Tag, $"Init: {SuccessInit}");
                Logger.Info(Tag, $"System contains supported devices: {SystemContainsSupportedDevices}");
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, ex.Message);
            }
        }

        public bool CanUseProfiles
        {
            get
            {
                return MiscSettings.Instance.UseOptimizationProfiles
                    && SuccessInit
                    && IsSystemElevated
                    && SystemContainsSupportedDevices;
            }
        }

        private List<CUDADevice> GetViableCudaDevices(IEnumerable<MiningPair> miningPairs)
        {
            return miningPairs
                    .GroupBy(x => x.Device.UUID)
                    .Select(g => g.First())
                    .Distinct()
                    .Where(mp => mp.Device is CUDADevice)
                    .Where(mp => mp.Algorithm.FirstAlgorithmType == AlgorithmType.DaggerHashimoto)
                    .Select(mp => mp.Device)
                    .Cast<CUDADevice>()
                    .ToList();
        }

        public string BuildMTString(OptimizationProfile prof)
        {
            if (prof.mt == null) return string.Empty;
            return string.Join(";", prof.mt
                .Where(m => m.Count == 2)
                .Select(item => $"{item[0]}={item[1]}"));
        }
        public void Start(IEnumerable<MiningPair> miningPairs)
        {
            if (!CanUseProfiles) return;
            lock (_startStopLock)
            {
                List<(ComputeDevice device, Device profile)> devicesWithProfilesToOptimize = new List<(ComputeDevice, Device)>();
                var unique = GetViableCudaDevices(miningPairs);
                foreach (var gpu in unique)
                {
                    var profileForDevice = GetDeviceProfile(gpu.Name);
                    var computeDev = AvailableDevices.Devices.Where(x => x.Uuid == gpu.UUID).FirstOrDefault();
                    if (profileForDevice != null && computeDev != null) devicesWithProfilesToOptimize.Add((computeDev, profileForDevice));
                }
                Logger.Info(Tag, $"Can optimize {devicesWithProfilesToOptimize.Count}/{miningPairs.Count()} devices");
                if (devicesWithProfilesToOptimize.Count == 0) return;
                int setCount = 0;
                foreach (var gpuProfilePair in devicesWithProfilesToOptimize)
                {
                    if (FindProfileNumForDeviceAndSetIfExists(gpuProfilePair.device, gpuProfilePair.profile)) setCount++;
                }
                Logger.Info(Tag, $"Optimized {setCount}/{miningPairs.Count()} devices");
            }
        }
        public void Stop(IEnumerable<MiningPair> miningPairs = null)
        {
            if(!CanUseProfiles) return;
            lock (_startStopLock)
            {
                var unique = GetViableCudaDevices(miningPairs);
                foreach (var gpu in unique)
                {
                    var computeDev = AvailableDevices.Devices.Where(x => x.Uuid == gpu.UUID).FirstOrDefault();
                    if (computeDev != null) computeDev.TryResetMemoryTimings();
                }
                Logger.Info(Tag, "Gpu settings reset back to normal");
            }
        }
        private Device GetDeviceProfile(string deviceName)
        {
            var foundProfiles = ProfileData.devices
                .Where(x => x.name != null)
                .Where(x => deviceName.Contains(x.name));
            if (foundProfiles.Count() == 0) return null;
            (int matchCount, Device profile) bestMatch = (0, null);
            foreach(var profile in foundProfiles)
            {
                var split = profile.name.Split(' ');
                var currentMatch = split.Count(item => deviceName.Contains(item));
                if(currentMatch > bestMatch.matchCount)
                {
                    bestMatch.matchCount = currentMatch;
                    bestMatch.profile = profile;
                }
            }
            if (bestMatch.profile != null &&
                SupportedDeviceNames.Any(item => bestMatch.profile.name.ToLower().Contains(item)))
            {
                Logger.Info(Tag, $"{deviceName} can be optimized");
                return bestMatch.profile;
            }
            return null;
        }
        public bool FindProfileNumForDeviceAndSetIfExists(ComputeDevice device, Device profile)
        {
            foreach (var profileID in ExistingProfiles.Reverse())
            {
                var foundProfile = profile.op.Where(x => x.id == profileID).FirstOrDefault();
                if (foundProfile == null) continue;
                var memoryTimings = BuildMTString(foundProfile);
                if (memoryTimings == string.Empty) continue;
                var ret = device.TrySetMemoryTimings(memoryTimings);
                //TODO SET OTHER STUFF FOR DEVICE HERE
                if (ret >= RET_OK) return true;
                return false;
            }
            return false;
        }
        protected bool IsSupportedDeviceName(string deviceName)
        {
            if(GetDeviceProfile(deviceName) == null) return false;
            return true;
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

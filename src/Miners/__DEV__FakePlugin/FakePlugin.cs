using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FakePlugin
{
    /// <summary>
    /// Plugin class inherits IMinerPlugin interface for registering plugin
    /// </summary>
    public class FakePlugin : PluginBase
    {
        // TODO implement this one
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings();

        //public override string PluginUUID => "12a1dc50-1d9d-11ea-8dad-816592d8b973"; //plugin 1
        //public override string PluginUUID => "1d21d950-1d9d-11ea-8dad-816592d8b973"; //plugin 2
        public override string PluginUUID => "24c913d0-1d9d-11ea-8dad-816592d8b973"; //plugin3
        public override string Name => GetPluginName();
        public override Version Version => GetPluginVersion();

        internal static testSettingsJson DEFAULT_SETTINGS = new testSettingsJson
        {
            name = "FakePlugin1",
            exitTimeWaitSeconds = 5,
            version = new Version(8, 0),
            supportedAlgorithmsPerType = new List<SupportedAlgorithmsPerType>
            {
                new SupportedAlgorithmsPerType
                {
                    type = DeviceType.NVIDIA,
                    algorithms = new List<AlgorithmType>
                    {
                        AlgorithmType.GrinCuckatoo32
                    }
                }
            }
        };

        private testSettingsJson GetTestSettings()
        {
            try
            {
                var path = Path.Combine(Paths.MinerPluginsPath(), PluginUUID, "testSettings.json");
                string text = File.ReadAllText(path);
                var settingsObject = JsonConvert.DeserializeObject<testSettingsJson>(text);
                if (settingsObject != null) return settingsObject;
            }
            catch { }
            return DEFAULT_SETTINGS;
        }


        private string GetPluginName()
        {
            var settingsObject = GetTestSettings();
            return settingsObject.name;
        }

        private Version GetPluginVersion()
        {
            var settingsObject = GetTestSettings();
            return settingsObject.version;
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var settingsObject = GetTestSettings();
            var amdAlgos = settingsObject.supportedAlgorithmsPerType.Where(type => type.type == DeviceType.AMD).FirstOrDefault();
            var cudaAlgos = settingsObject.supportedAlgorithmsPerType.Where(type => type.type == DeviceType.NVIDIA).FirstOrDefault();
            var cpuAlgos = settingsObject.supportedAlgorithmsPerType.Where(type => type.type == DeviceType.CPU).FirstOrDefault();

            foreach (var device in devices)
            {

                if (device is AMDDevice amd)
                {
                    if (amdAlgos != null)
                    {
                        var amdList = new List<Algorithm>();
                        foreach (var algo in amdAlgos.algorithms)
                        {
                            amdList.Add(new Algorithm(PluginUUID, algo));
                        }
                        supported.Add(device, amdList);
                    }
                }
                if (device is CUDADevice cuda)
                {
                    if (cudaAlgos != null)
                    {
                        var cudaList = new List<Algorithm>();
                        foreach (var algo in cudaAlgos.algorithms)
                        {
                            cudaList.Add(new Algorithm(PluginUUID, algo));
                        }
                        supported.Add(device, cudaList);
                    }
                }
                if (device is CPUDevice cpu)
                {
                    if (cpuAlgos != null)
                    {
                        var cpuList = new List<Algorithm>();
                        foreach (var algo in cpuAlgos.algorithms)
                        {
                            cpuList.Add(new Algorithm(PluginUUID, algo));
                        }
                        supported.Add(device, cpuList);
                    }
                }
            }

            return supported;
        }

        public FakePlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "1.83",
                ExePath = new List<string> { "DemoMiner.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/NHM_MinerPluginsDownloads/releases/download/binVer/DemoMiner.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Fake miner - High-performance miner for NVIDIA and AMD GPUs."
            };
        }

        public override string Author => "info@nicehash.com";

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            return new List<string>() { };
        }
        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            var settingsObject = GetTestSettings();
            var rebenchAlgos = settingsObject.rebenchmarkAlgorithms;

            var isReBenchVersion = benchmarkedPluginVersion.Major == Version.Major && benchmarkedPluginVersion.Minor < Version.Minor;
            var first = ids.FirstOrDefault();
            var isInList = rebenchAlgos.Contains(first);
            return isReBenchVersion && isInList;
        }
        protected override MinerBase CreateMinerBase()
        {
            return new FakeMiner(PluginUUID);
        }
    }
}

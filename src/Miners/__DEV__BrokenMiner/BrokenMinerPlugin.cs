using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;

namespace BrokenMiner
{
    public class BrokenMinerPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2, IMinerBinsSource
    {

        Version IMinerPlugin.Version => GetValueOrErrorSettings.GetValueOrError("Version", new Version(16, 0));

        string IMinerPlugin.Name => GetValueOrErrorSettings.GetValueOrError("Name", "Broken Plugin");

        string IMinerPlugin.Author => GetValueOrErrorSettings.GetValueOrError("Author", "John Doe");

        string IMinerPlugin.PluginUUID => GetValueOrErrorSettings.GetValueOrError("PluginUUID", "BrokenMinerPluginUUID");

        bool IMinerPlugin.CanGroup(MiningPair a, MiningPair b) => GetValueOrErrorSettings.GetValueOrError("CanGroup", false);

        IEnumerable<string> IBinaryPackageMissingFilesChecker.CheckBinaryPackageMissingFiles() =>
            GetValueOrErrorSettings.GetValueOrError("CheckBinaryPackageMissingFiles", new List<string> { "text_file_acting_as_exe.txt" });

        IMiner IMinerPlugin.CreateMiner() => GetValueOrErrorSettings.GetValueOrError("CreateMiner", new BrokenMiner());

        TimeSpan IGetApiMaxTimeoutV2.GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs) => GetValueOrErrorSettings.GetValueOrError("GetApiMaxTimeout", new TimeSpan(1, 10, 5));
        bool IGetApiMaxTimeoutV2.IsGetApiMaxTimeoutEnabled => GetValueOrErrorSettings.GetValueOrError("IsGetApiMaxTimeoutEnabled", true);

        Dictionary<BaseDevice, IReadOnlyList<Algorithm>> IMinerPlugin.GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // this will break the default loader
            ////// Fake device 
            //var gpu = new BaseDevice(DeviceType.NVIDIA, "FAKE-d97bdb7c-4155-9124-31b7-4743e16d3ac0", "GTX 1070 Ti", 0);
            //supported.Add(gpu, new List<Algorithm>() { new Algorithm("BrokenMinerPluginUUID", AlgorithmType.ZHash), new Algorithm("BrokenMinerPluginUUID", AlgorithmType.DaggerHashimoto) });
            // we support all devices
            foreach (var dev in devices)
            {
                supported.Add(dev, new List<Algorithm>() { new Algorithm("BrokenMinerPluginUUID", AlgorithmType.ZHash) });
            }

            return GetValueOrErrorSettings.GetValueOrError("GetSupportedAlgorithms", supported);
        }

        void IInitInternals.InitInternals() => GetValueOrErrorSettings.SetError("InitInternals");

        bool IReBenchmarkChecker.ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids) =>
            GetValueOrErrorSettings.GetValueOrError("ShouldReBenchmarkAlgorithmOnDevice", false);

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrlsForPlugin()
        {
            return GetValueOrErrorSettings.GetValueOrError("GetMinerBinsUrlsForPlugin", new List<string> { "https://github.com/nicehash/MinerDownloads/releases/download/v1.0/BrokenMinerPlugin.zip" });
        }
    }
}

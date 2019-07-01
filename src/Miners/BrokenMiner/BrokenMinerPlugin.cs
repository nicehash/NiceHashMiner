using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BrokenMiner
{
    public class BrokenMinerPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeout
    {

        Version IMinerPlugin.Version => GetValueOrErrorSettings.GetValueOrError("Version", new Version(1,0));

        string IMinerPlugin.Name => GetValueOrErrorSettings.GetValueOrError("Name", "Broken Plugin");

        string IMinerPlugin.Author => GetValueOrErrorSettings.GetValueOrError("Author", "John Doe");

        string IMinerPlugin.PluginUUID => GetValueOrErrorSettings.GetValueOrError("PluginUUID", "BrokenMinerPluginUUID");

        bool IMinerPlugin.CanGroup(MiningPair a, MiningPair b) => GetValueOrErrorSettings.GetValueOrError("CanGroup", false);

        IEnumerable<string> IBinaryPackageMissingFilesChecker.CheckBinaryPackageMissingFiles() =>
            GetValueOrErrorSettings.GetValueOrError("CheckBinaryPackageMissingFiles", new List<string> { "bminer.exe" });

        IMiner IMinerPlugin.CreateMiner() => GetValueOrErrorSettings.GetValueOrError("CreateMiner", new BrokenMiner());

        TimeSpan IGetApiMaxTimeout.GetApiMaxTimeout() => GetValueOrErrorSettings.GetValueOrError("GetApiMaxTimeout", new TimeSpan(1, 10, 5));

        Dictionary<BaseDevice, IReadOnlyList<Algorithm>> IMinerPlugin.GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // TODO this will break the default loader
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
    }
}

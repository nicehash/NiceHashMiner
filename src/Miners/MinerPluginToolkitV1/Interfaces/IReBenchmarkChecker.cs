using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;

namespace MinerPluginToolkitV1.Interfaces
{
    public interface IReBenchmarkChecker
    {
        bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids);
    }
}

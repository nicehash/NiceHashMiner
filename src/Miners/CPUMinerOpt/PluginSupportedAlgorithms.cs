using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CpuMinerOpt
{
    // TODO move this into PluginBase when we break 3.x plugins with monero fork
    internal static class PluginSupportedAlgorithms
    {
        internal static bool UnsafeLimits(string PluginUUID)
        {
            try
            {
                var unsafeLimits = Path.Combine(Paths.MinerPluginsPath(), PluginUUID, "unsafe_limits");
                return File.Exists(unsafeLimits);
            }
            catch
            { }
            return false;
        }

        internal static Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithmsDict()
        {
            var cpuAlgos = new HashSet<AlgorithmType>(GetSupportedAlgorithmsCPU("").SelectMany(a => a.IDs));
            var ret = new Dictionary<DeviceType, List<AlgorithmType>>
            {
                { DeviceType.CPU, cpuAlgos.ToList() }
            };
            return ret;
        }
        internal static IReadOnlyList<Algorithm> GetSupportedAlgorithmsCPU(string PluginUUID)
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.Lyra2Z) { Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3) { Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.X16R) { Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.X16Rv2)
            };
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Lyra2Z:
                    return "lyra2z";
                case AlgorithmType.Lyra2REv3:
                    return "lyra2rev3";
                case AlgorithmType.X16R:
                    return "x16r";
                case AlgorithmType.X16Rv2:
                    return "x16rv2";
                default:
                    return "";
            }
        }
    }
}

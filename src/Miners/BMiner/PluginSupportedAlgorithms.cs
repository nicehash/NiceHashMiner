using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BMiner
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
            var nvidiaAlgos = new HashSet<AlgorithmType>(GetSupportedAlgorithmsNVIDIA("").SelectMany(a => a.IDs)).ToList();
            var amdAlgos = new HashSet<AlgorithmType>(GetSupportedAlgorithmsAMD("").SelectMany(a => a.IDs)).ToList();
            var ret = new Dictionary<DeviceType, List<AlgorithmType>>
            {
                { DeviceType.NVIDIA, nvidiaAlgos },
                { DeviceType.AMD, amdAlgos },
            };
            return ret;
        }

        internal static IReadOnlyList<Algorithm> GetSupportedAlgorithmsNVIDIA(string PluginUUID)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.ZHash) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.Beam) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckarood29),
            };
            return algorithms;
        }

        internal static IReadOnlyList<Algorithm> GetSupportedAlgorithmsAMD(string PluginUUID)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.Beam) {Enabled = false },
            };
            return algorithms;
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.DaggerHashimoto: return "ethstratum";
                case AlgorithmType.ZHash: return "zhash";
                case AlgorithmType.Beam: return "beam";
                case AlgorithmType.GrinCuckaroo29: return "cuckaroo29";
                case AlgorithmType.GrinCuckatoo31: return "cuckatoo31";
                case AlgorithmType.GrinCuckarood29: return "cuckaroo29d";
                default: return "";
            }
        }

        internal static double DevFee(AlgorithmType algorithmType)
        {
            if (AlgorithmType.DaggerHashimoto == algorithmType) return 0.65;
            return 2.0;
        }
    }
}

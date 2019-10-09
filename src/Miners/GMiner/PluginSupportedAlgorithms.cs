using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GMinerPlugin
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

        internal static List<Algorithm> GetSupportedAlgorithmsNVIDIA(string PluginUUID)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.ZHash),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares,
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckarood29),
                new Algorithm(PluginUUID, AlgorithmType.Beam) { Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.BeamV2),
            };
            return algorithms;
        }

        internal static List<Algorithm> GetSupportedAlgorithmsAMD(string PluginUUID)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares
                new Algorithm(PluginUUID, AlgorithmType.Beam) { Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.BeamV2),
            };
            return algorithms;
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.ZHash:
                    return "144_5";
                case AlgorithmType.Beam:
                    return "beamhashI";
                case AlgorithmType.GrinCuckatoo31:
                    return "grin31";
                case AlgorithmType.CuckooCycle:
                    return "cuckoo29";
                case AlgorithmType.GrinCuckarood29:
                    return "cuckarood29";
                case AlgorithmType.BeamV2:
                    return "beamhashII";
                default:
                    return "";
            }
        }

        // fee fixed
        //internal static double DevFee(AlgorithmType algorithmType)
        //{
        //}
    }
}

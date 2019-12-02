using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeamRedMiner
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
            var amdAlgos = new HashSet<AlgorithmType>(GetSupportedAlgorithmsAMD("").SelectMany(a => a.IDs)).ToList();
            var ret = new Dictionary<DeviceType, List<AlgorithmType>>
            {
                { DeviceType.AMD, amdAlgos },
            };
            return ret;
        }

        internal static List<Algorithm> GetSupportedAlgorithmsAMD(string PluginUUID)
        {
            var algorithms = new List<Algorithm> {
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightR),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2Z),
                new Algorithm(PluginUUID, AlgorithmType.X16R),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                //new Algorithm(PluginUUID, AlgorithmType.MTP) { Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckarood29),
                new Algorithm(PluginUUID, AlgorithmType.X16Rv2)
            };
            return algorithms;
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.CryptoNightR:
                    return "cnr";
                case AlgorithmType.Lyra2REv3:
                    return "lyra2rev3";
                case AlgorithmType.Lyra2Z:
                    return "lyra2z";
                case AlgorithmType.X16R:
                    return "x16r";
                case AlgorithmType.GrinCuckatoo31:
                    return "cuckatoo31_grin";
                //case AlgorithmType.MTP:
                //    return "mtp";
                case AlgorithmType.GrinCuckarood29:
                    return "cuckarood29_grin";
                case AlgorithmType.X16Rv2:
                    return "x16rv2";
                default:
                    return "";
            }
        }

        internal static double DevFee(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Lyra2Z:
                    return 3.0;
                default:
                    return 2.5;
            }
        }
    }
}

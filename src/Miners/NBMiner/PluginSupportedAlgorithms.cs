using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace NBMiner
{
    // TODO move this into PluginBase when we break 3.x plugins with monero fork
    internal static class PluginSupportedAlgorithms
    {
        internal static Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithmsDict()
        {
            var nvidiaAlgos = new HashSet<AlgorithmType>(GetSupportedAlgorithmsNVIDIA("").SelectMany(a => a.IDs)).ToList();
            var ret = new Dictionary<DeviceType, List<AlgorithmType>>
            {
                { DeviceType.NVIDIA, nvidiaAlgos },
            };
            return ret;
        }

        internal static List<Algorithm> GetSupportedAlgorithmsNVIDIA(string PluginUUID)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckarood29),
            };
            return algorithms;
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.GrinCuckatoo31:
                    return "cuckatoo";
                case AlgorithmType.CuckooCycle:
                    return "cuckoo_ae";
                case AlgorithmType.GrinCuckarood29:
                    return "cuckarood";
                default:
                    return "";
            }
        }

        internal static double DevFee(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.GrinCuckatoo31:
                case AlgorithmType.CuckooCycle:
                case AlgorithmType.GrinCuckarood29:
                    return 2.0;
                default:
                    return 0;
            }
        }
    }
}

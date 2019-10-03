using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace CCMinerTpruvot
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

        internal static IReadOnlyList<Algorithm> GetSupportedAlgorithmsNVIDIA(string PluginUUID)
        {
            var algorithms = new List<Algorithm> {
                new Algorithm(PluginUUID, AlgorithmType.X16R), // TODO check performance
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
            };
            return algorithms;
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.Lyra2REv3: return "lyra2v3";
            }
            // TODO throw exception
            return "";
        }

    }
}

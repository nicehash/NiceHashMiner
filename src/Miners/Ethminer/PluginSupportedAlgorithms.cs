using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Ethminer
{
    // TODO move this into PluginBase when we break 3.x plugins with monero fork
    internal static class PluginSupportedAlgorithms
    {
        internal static Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithmsDict()
        {
            var gpuAlgos = new HashSet<AlgorithmType>(GetSupportedAlgorithmsGPU("").SelectMany(a => a.IDs)).ToList();
            var ret = new Dictionary<DeviceType, List<AlgorithmType>>
            {
                { DeviceType.NVIDIA, gpuAlgos },
                { DeviceType.AMD, gpuAlgos },
            };
            return ret;
        }

        internal static List<Algorithm> GetSupportedAlgorithmsGPU(string PluginUUID)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto),
            };
            return algorithms;
        }
    }
}

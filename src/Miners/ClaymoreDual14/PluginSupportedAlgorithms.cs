using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClaymoreDual14
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
            var gpuAlgos = new HashSet<AlgorithmType>(GetSupportedAlgorithmsGPU("").SelectMany(a => a.IDs)).ToList();
            var ret = new Dictionary<DeviceType, List<AlgorithmType>>
            {
                { DeviceType.NVIDIA, gpuAlgos },
                { DeviceType.AMD, gpuAlgos },
            };
            return ret;
        }

        internal static IReadOnlyList<Algorithm> GetSupportedAlgorithmsGPU(string PluginUUID)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto),
            // duals disabled by default
#pragma warning disable 0618
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak) {Enabled = false },
#pragma warning restore 0618
            };
            return algorithms;
        }

        // these are in the base class
        //internal static string AlgorithmName(AlgorithmType algorithmType)
        //{
        //}

        //internal static double DevFee(AlgorithmType algorithmType)
        //{
        //}
    }
}

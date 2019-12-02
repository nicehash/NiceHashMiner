using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZEnemy
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
            var ret = new Dictionary<DeviceType, List<AlgorithmType>>
            {
                { DeviceType.NVIDIA, nvidiaAlgos }
            };
            return ret;
        }

        internal static List<Algorithm> GetSupportedAlgorithmsNVIDIA(string PluginUUID)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.X16R),
                new Algorithm(PluginUUID, AlgorithmType.X16Rv2)
            };
            return algorithms;
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.X16Rv2: return "x16rv2";
            }
            return "";
        }

        internal static double DevFee(AlgorithmType algorithmType)
        {
            return 1.0;
        }
    }
}

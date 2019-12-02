using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CryptoDredge
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
            var nvidiaAlgos = new HashSet<AlgorithmType>(GetSupportedAlgorithmsNVIDIA("").SelectMany(a => a.IDs));
            var ret = new Dictionary<DeviceType, List<AlgorithmType>>
            {
                { DeviceType.NVIDIA, nvidiaAlgos.ToList() }
            };
            return ret;
        }

        internal static IReadOnlyList<Algorithm> GetSupportedAlgorithmsNVIDIA(string PluginUUID)
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
                new Algorithm(PluginUUID, AlgorithmType.X16R),
                new Algorithm(PluginUUID, AlgorithmType.X16Rv2),
                //new Algorithm(PluginUUID, AlgorithmType.MTP) { Enabled = false }
            };
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Lyra2REv3: return "lyra2v3";
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.X16Rv2: return "x16rv2";
                //case AlgorithmType.MTP: return "mtp";
                default: return "";
            }
        }

        internal static double DevFee(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                //case AlgorithmType.MTP: return 2.0;
                default: return 1.0;
            }
        }
    }
}

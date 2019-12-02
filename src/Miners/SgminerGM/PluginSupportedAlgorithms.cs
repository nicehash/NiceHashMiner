using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SgminerGM
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
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto)
                {
                    ExtraLaunchParameters = " --remove-disabled --xintensity 512 -w 192 -g 1"
                },
            };
            return algorithms;
        }

        internal static string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.DaggerHashimoto:
                    return "ethash";
                default:
                    return "";
            }
        }

        // no dev fee
        //internal static double DevFee(AlgorithmType algorithmType)
        //{
        //}
    }
}

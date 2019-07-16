using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace MinerPluginToolkitV1
{
    /// <summary>
    /// Filters class consists of functions that filter out algorithms for devices
    /// </summary>
    public static class Filters
    {
        // WARNING THESE FILTERS ARE NOT THE SAME AS RUN-TIME RAM REQUIREMENTS
        //https://investoon.com/tools/dag_size
        public const ulong MinDaggerHashimotoMemory = 3UL << 30; // 3GB
        public const ulong MinZHashMemory = 1879047230; // 1.75GB
        public const ulong MinBeamMemory = 3113849695; // 2.9GB
        public const ulong MinGrinCuckaroo29Memory = 6012951136; // 5.6GB
        public const ulong MinGrin31Mem = 8UL << 30; // 8GB
        public const ulong MinCuckooCycleMem = 6UL << 30; // 6GB
        public const ulong MinLyra2REv3Mem = 2UL << 30; // 2GB
        public const ulong MinX16RMem = 2UL << 30; // 2GB
        public const ulong MinMTPMem = 5UL << 30; // 5GB
        public const ulong MinGrinCuckarood29Memory = 6012951136; // 5.6GB


        public static readonly Dictionary<AlgorithmType, ulong> minMemoryPerAlgo = new Dictionary<AlgorithmType, ulong>
        {
            { AlgorithmType.DaggerHashimoto, MinDaggerHashimotoMemory },
            { AlgorithmType.ZHash, MinZHashMemory},
            { AlgorithmType.Beam, MinBeamMemory },
            { AlgorithmType.GrinCuckaroo29, MinGrinCuckaroo29Memory },
            { AlgorithmType.GrinCuckatoo31, MinGrin31Mem },
            { AlgorithmType.CuckooCycle, MinCuckooCycleMem },
            { AlgorithmType.Lyra2REv3, MinLyra2REv3Mem },
            { AlgorithmType.X16R, MinX16RMem },
            { AlgorithmType.MTP, MinMTPMem },
            { AlgorithmType.GrinCuckarood29, MinGrinCuckarood29Memory },
        };

        public static List<AlgorithmType> InsufficientDeviceMemoryAlgorithnms(ulong Ram, IEnumerable<AlgorithmType> algos)
        {
            var filterAlgorithms = new List<AlgorithmType>();
            foreach (var algo in algos)
            {
                if (minMemoryPerAlgo.ContainsKey(algo) == false) continue;
                var minRam = minMemoryPerAlgo[algo];
                if (Ram < minRam) filterAlgorithms.Add(algo);
            }
            return filterAlgorithms;
        }

        public static List<Algorithm> FilterAlgorithmsList(List<Algorithm> algos, IEnumerable<AlgorithmType> filterAlgos)
        {
            return algos.Where(a => filterAlgos.Contains(a.FirstAlgorithmType) == false).ToList();
        }

        public static List<Algorithm> FilterInsufficientRamAlgorithmsList(ulong Ram, List<Algorithm> algos)
        {
            var filterAlgos = InsufficientDeviceMemoryAlgorithnms(Ram, algos.Select(a => a.FirstAlgorithmType));
            return FilterAlgorithmsList(algos, filterAlgos);
        }
    }
}

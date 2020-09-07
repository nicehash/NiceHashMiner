using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1
{
    /// <summary>
    /// Filters class consists of functions that filter out algorithms for devices
    /// </summary>
    public static class Filters
    {
        // WARNING THESE FILTERS ARE NOT THE SAME AS RUN-TIME RAM REQUIREMENTS
        //https://investoon.com/tools/dag_size
        public const ulong MinDaggerHashimotoMemory = 5UL << 30; // 5GB
        public const ulong MinZHashMemory = 1879047230; // 1.75GB
        public const ulong MinBeamMemory = 3113849695; // 2.9GB
        public const ulong MinGrin31Mem = 11UL << 30; // 11GB
        public const ulong MinCuckooCycleMem = 6UL << 30; // 6GB
        public const ulong MinLyra2REv3Mem = 2UL << 30; // 2GB
        public const ulong MinGrinCuckarood29Memory = 6012951136; // 5.6GB
        public const ulong MinGrin32Mem = 7UL << 30; // 7.0GB (because system acn reserve GPU memory) really this is 8GB
        public const ulong MinKAWPOWMemory = 4UL << 30; // 4GB
        public const ulong MinCuckaroo29BFCMemory = 5UL << 30; // 5GB


#pragma warning disable 0618
        private static readonly Dictionary<AlgorithmType, ulong> _minMemoryPerAlgo = new Dictionary<AlgorithmType, ulong>
        {
            { AlgorithmType.DaggerHashimoto, MinDaggerHashimotoMemory },
            { AlgorithmType.ZHash, MinZHashMemory},
            { AlgorithmType.BeamV3, MinBeamMemory },
            { AlgorithmType.GrinCuckatoo31, MinGrin31Mem },
            { AlgorithmType.CuckooCycle, MinCuckooCycleMem },
            { AlgorithmType.Lyra2REv3, MinLyra2REv3Mem },
            { AlgorithmType.GrinCuckarood29, MinGrinCuckarood29Memory },
            { AlgorithmType.GrinCuckatoo32, MinGrin32Mem },
            { AlgorithmType.KAWPOW, MinKAWPOWMemory },
            { AlgorithmType.Cuckaroo29BFC, MinCuckaroo29BFCMemory },
        };
#pragma warning restore 0618

        public static List<AlgorithmType> InsufficientDeviceMemoryAlgorithnms(ulong Ram, IEnumerable<AlgorithmType> algos)
        {
            var filterAlgorithms = new List<AlgorithmType>();
            foreach (var algo in algos)
            {
                if (_minMemoryPerAlgo.ContainsKey(algo) == false) continue;
                var minRam = _minMemoryPerAlgo[algo];
                if (Ram < minRam) filterAlgorithms.Add(algo);
            }
            return filterAlgorithms;
        }

        public static List<AlgorithmType> InsufficientDeviceMemoryAlgorithnmsCustom(ulong Ram, IEnumerable<AlgorithmType> algos, Dictionary<AlgorithmType, ulong> minMemoryPerAlgo)
        {
            // fill check
            var check = new Dictionary<AlgorithmType, ulong>();
            foreach (var kvp in minMemoryPerAlgo)
            {
                check[kvp.Key] = kvp.Value;
            }
            // now fill the rest
            foreach (var kvp in _minMemoryPerAlgo)
            {
                if (check.ContainsKey(kvp.Key)) continue; // only fill if the key is missing 
                check[kvp.Key] = kvp.Value;
            }
            var filterAlgorithms = new List<AlgorithmType>();
            foreach (var algo in algos)
            {
                if (check.ContainsKey(algo) == false) continue;
                var minRam = check[algo];
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

        public static List<Algorithm> FilterInsufficientRamAlgorithmsListCustom(ulong Ram, List<Algorithm> algos, Dictionary<AlgorithmType, ulong> minMemoryPerAlgo)
        {
            var filterAlgos = InsufficientDeviceMemoryAlgorithnmsCustom(Ram, algos.Select(a => a.FirstAlgorithmType), minMemoryPerAlgo);
            return FilterAlgorithmsList(algos, filterAlgos);
        }
    }
}

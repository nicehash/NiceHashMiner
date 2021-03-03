using NHM.Common;
using NHM.Common.Configs;
using NHM.Common.Enums;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Switching
{
    /// <summary>
    /// Maintains global registry of NH SMA
    /// </summary>
    public static class NHSmaData
    {
        private const string Tag = "NHSMAData";
        private static string CachedFile => Paths.InternalsPath("cached_sma.json");

        /// <summary>
        /// True iff there has been at least one SMA update
        /// </summary>
        public static bool _hasData = false;
        public static bool HasData
        {
            get
            {
                if (BuildOptions.FORCE_MINING || BuildOptions.FORCE_PROFITABLE) return true;
                return _hasData;
            }
            private set
            {
                _hasData = value;
            }
        }

        // private static Dictionary<AlgorithmType, List<double>> _recentPaying;

        // Global list of SMA data, should be accessed with a lock since callbacks/timers update it
        private static Dictionary<AlgorithmType, double> _currentPayingRates;
        // Global list of stable algorithms, should be accessed with a lock
        private static HashSet<AlgorithmType> _stableAlgorithms;

        static NHSmaData()
        {
            _currentPayingRates = new Dictionary<AlgorithmType, double>();
            _stableAlgorithms = new HashSet<AlgorithmType>();

            var cacheDict = InternalConfigs.ReadFileSettings<Dictionary<AlgorithmType, double>>(CachedFile);

            // _recentPaying = new Dictionary<AlgorithmType, List<double>>();
            foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType)))
            {
                if (algo >= 0)
                {
                    var paying = 0d;

                    if (cacheDict?.TryGetValue(algo, out paying) ?? false)
                        HasData = true;

                    if (BuildOptions.FORCE_MINING || BuildOptions.FORCE_PROFITABLE)
                    {
                        paying = 10000;
                    }

                    _currentPayingRates[algo] = paying;
                }
            }
        }

        #region Update Methods

        // TODO maybe just swap the dictionaries???
        /// <summary>
        /// Change SMA profits to new values
        /// </summary>
        /// <param name="newSma">Algorithm/profit dictionary with new values</param>
        public static void UpdateSmaPaying(Dictionary<AlgorithmType, double> newSma)
        {
            lock (_currentPayingRates)
            {
                foreach (var algo in newSma.Keys)
                {
                    if (_currentPayingRates.ContainsKey(algo))
                    {
                        _currentPayingRates[algo] = newSma[algo];
                        if (BuildOptions.FORCE_MINING || BuildOptions.FORCE_PROFITABLE)
                        {
                            _currentPayingRates[algo] = 1000;
                        }
                    }
                }

                if (MiscSettings.Instance.UseSmaCache)
                {
                    // Cache while in lock so file is not accessed on multiple threads
                    var isFileSaved = InternalConfigs.WriteFileSettings(CachedFile, newSma);
                    if (!isFileSaved) Logger.Error(Tag, "CachedSma not saved");
                }
            }

            HasData = true;
        }

        /// <summary>
        /// Change SMA profit for one algo
        /// </summary>
        internal static void UpdatePayingForAlgo(AlgorithmType algo, double paying)
        {
            lock (_currentPayingRates)
            {
                if (!_currentPayingRates.ContainsKey(algo))
                    throw new ArgumentException("Algo not setup in SMA");
                _currentPayingRates[algo] = paying;
            }

            if (BuildOptions.FORCE_MINING || BuildOptions.FORCE_PROFITABLE)
            {
                _currentPayingRates[algo] = 1000;
            }

            HasData = true;
        }

        /// <summary>
        /// Update list of stable algorithms
        /// </summary>
        /// <param name="algorithms">Algorithms that are stable</param>
        public static void UpdateStableAlgorithms(IEnumerable<AlgorithmType> algorithms)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Updating stable algorithms");
            var hasChange = false;

            lock (_stableAlgorithms)
            {
                var algosEnumd = algorithms as AlgorithmType[] ?? algorithms.ToArray();
                foreach (var algo in algosEnumd)
                {
                    if (_stableAlgorithms.Add(algo))
                    {
                        sb.AppendLine($"\tADDED {algo}");
                        hasChange = true;
                    }
                }

                _stableAlgorithms.RemoveWhere(algo =>
                {
                    if (algosEnumd.Contains(algo)) return false;

                    sb.AppendLine($"\tREMOVED {algo}");
                    hasChange = true;
                    return true;
                });
            }
            if (!hasChange)
            {
                sb.AppendLine("\tNone changed");
            }
            Logger.Info(Tag, sb.ToString());
        }

        #endregion

        #region Get Methods

        /// <summary>
        /// Attempt to get paying rate for an algorithm
        /// </summary>
        /// <param name="algo">Algorithm</param>
        /// <param name="paying">Variable to place paying in</param>
        /// <returns>True iff we know about this algo</returns>
        public static bool TryGetPaying(AlgorithmType algo, out double paying)
        {
            lock (_currentPayingRates)
            {
                return _currentPayingRates.TryGetValue(algo, out paying);
            }
        }

        public static bool IsAlgorithmStable(AlgorithmType algo)
        {
            lock (_stableAlgorithms)
            {
                return _stableAlgorithms.Contains(algo);
            }
        }

        /// <summary>
        /// Filters SMA profits based on whether the algorithm is stable
        /// </summary>
        /// <param name="stable">True to get stable, false to get unstable</param>
        /// <returns>Filtered Algorithm/double map</returns>
        public static Dictionary<AlgorithmType, double> FilteredCurrentProfits(bool stable)
        {
            var dict = new Dictionary<AlgorithmType, double>();

            lock (_currentPayingRates)
            {
                var filtered = _currentPayingRates.Where(kvp => _stableAlgorithms.Contains(kvp.Key) == stable);
                foreach (var kvp in filtered)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return dict;
        }

        /// <summary>
        /// Copy and return SMA profits 
        /// </summary>
        public static Dictionary<AlgorithmType, double> CurrentPayingRatesSnapshot()
        {
            var dict = new Dictionary<AlgorithmType, double>();

            lock (_currentPayingRates)
            {
                foreach (var kvp in _currentPayingRates)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return dict;
        }

        public static async Task<bool> WaitOnDataAsync(int seconds)
        {
            var hasData = HasData;

            for (var i = 0; i < seconds; i++)
            {
                if (hasData) return true;
                await Task.Delay(1000);
                hasData = HasData;
                Logger.Info("NICEHASH", $"After {i}s has data: {hasData}");
            }

            return hasData;
        }
        #endregion
    }
}

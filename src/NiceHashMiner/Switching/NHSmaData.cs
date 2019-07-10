using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinerPluginToolkitV1.Configs;
using NiceHashMiner.Configs;
using NHM.Common;
using NHM.Common.Enums;

namespace NiceHashMiner.Switching
{
    /// <summary>
    /// Maintains global registry of NH SMA
    /// </summary>
    public static class NHSmaData
    {
        private const string Tag = "NHSMAData";
        private static string CachedFile => Paths.InternalsPath("cached_sma.json");

        public static bool Initialized { get; private set; }
        /// <summary>
        /// True iff there has been at least one SMA update
        /// </summary>
        public static bool HasData { get; private set; }

        // private static Dictionary<AlgorithmType, List<double>> _recentPaying;

        // Global list of SMA data, should be accessed with a lock since callbacks/timers update it
        private static Dictionary<AlgorithmType, double> _currentPayingRates;
        // Global list of stable algorithms, should be accessed with a lock
        private static HashSet<AlgorithmType> _stableAlgorithms;

        // Public for tests only
        public static void Initialize()
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

#if FILLSMA
                    paying = 1;
#endif

                    _currentPayingRates[algo] = paying;
                }
            }

            Initialized = true;
        }

        public static void InitializeIfNeeded()
        {
            if (!Initialized) Initialize();
        }

        #region Update Methods

        // TODO maybe just swap the dictionaries???
        /// <summary>
        /// Change SMA profits to new values
        /// </summary>
        /// <param name="newSma">Algorithm/profit dictionary with new values</param>
        public static void UpdateSmaPaying(Dictionary<AlgorithmType, double> newSma)
        {
            CheckInit();
            lock (_currentPayingRates)
            {
                foreach (var algo in newSma.Keys)
                {
                    if (_currentPayingRates.ContainsKey(algo))
                    {
                        _currentPayingRates[algo] = newSma[algo];
#if FILLSMA
                        var paying = 1;
                        _currentPayingRates[algo] = paying;
#endif
                    }
                }

                if (ConfigManager.GeneralConfig.UseSmaCache)
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
            CheckInit();
            lock (_currentPayingRates)
            {
                if (!_currentPayingRates.ContainsKey(algo))
                    throw new ArgumentException("Algo not setup in SMA");
                _currentPayingRates[algo] = paying;
            }

#if FILLSMA
            paying = 1;
            _currentPayingRates[algo] = paying;
#endif

            HasData = true;
        }

        /// <summary>
        /// Update list of stable algorithms
        /// </summary>
        /// <param name="algorithms">Algorithms that are stable</param>
        public static void UpdateStableAlgorithms(IEnumerable<AlgorithmType> algorithms)
        {
            CheckInit();
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

        # region Get Methods
        
        /// <summary>
        /// Attempt to get paying rate for an algorithm
        /// </summary>
        /// <param name="algo">Algorithm</param>
        /// <param name="paying">Variable to place paying in</param>
        /// <returns>True iff we know about this algo</returns>
        public static bool TryGetPaying(AlgorithmType algo, out double paying)
        {
            CheckInit();
            lock (_currentPayingRates)
            {
                return _currentPayingRates.TryGetValue(algo, out paying);
            }
        }

        public static bool IsAlgorithmStable(AlgorithmType algo)
        {
            CheckInit();
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
            CheckInit();
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
        public static Dictionary<AlgorithmType, double> CurrentProfitsSnapshot()
        {
            CheckInit();
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

        #endregion

        /// <summary>
        /// Helper to ensure initialization
        /// </summary>
        private static void CheckInit()
        {
            if (!Initialized)
                throw new InvalidOperationException("NHSmaData cannot be used before initialization");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Switching
{
    /// <summary>
    /// Maintains global registry of NH SMA
    /// </summary>
    public static class NHSmaData
    {
        private const string Tag = "NHSMAData";
        private const string CachedFile = "internals\\cached_sma.json";

        public static bool Initialized { get; private set; }
        /// <summary>
        /// True iff there has been at least one SMA update
        /// </summary>
        public static bool HasData { get; private set; }

        // private static Dictionary<AlgorithmType, List<double>> _recentPaying;

        // Global list of SMA data, should be accessed with a lock since callbacks/timers update it
        private static Dictionary<AlgorithmType, NiceHashSma> _currentSma;
        // Global list of stable algorithms, should be accessed with a lock
        private static HashSet<AlgorithmType> _stableAlgorithms;

        // Public for tests only
        public static void Initialize()
        {
            _currentSma = new Dictionary<AlgorithmType, NiceHashSma>();
            _stableAlgorithms = new HashSet<AlgorithmType>();

            Dictionary<AlgorithmType, double> cacheDict = null;
            try
            {
                var cache = File.ReadAllText(CachedFile);
                cacheDict = JsonConvert.DeserializeObject<Dictionary<AlgorithmType, double>>(cache);
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint(Tag, e.ToString());
            }

            // _recentPaying = new Dictionary<AlgorithmType, List<double>>();
            foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType)))
            {
                if (algo >= 0)
                {
                    var paying = 0d;
                    if (cacheDict?.TryGetValue(algo, out paying) ?? false)
                        HasData = true;

                    _currentSma[algo] = new NiceHashSma
                    {
                        Port = (int) algo + 3333,
                        Name = algo.ToString().ToLower(),
                        Algo = (int) algo,
                        Paying = paying
                    };
                    //_recentPaying[algo] = new List<double>
                    //{
                    //    0
                    //};
                }
            }

            Initialized = true;
        }

        public static void InitializeIfNeeded()
        {
            if (!Initialized) Initialize();
        }

        #region Update Methods

        /// <summary>
        /// Change SMA profits to new values
        /// </summary>
        /// <param name="newSma">Algorithm/profit dictionary with new values</param>
        public static void UpdateSmaPaying(Dictionary<AlgorithmType, double> newSma)
        {
            CheckInit();
            lock (_currentSma)
            {
                foreach (var algo in newSma.Keys)
                {
                    if (_currentSma.ContainsKey(algo))
                    {
                        _currentSma[algo].Paying = newSma[algo];
                    }
                }

                if (ConfigManager.GeneralConfig.UseSmaCache)
                {
                    // Cache while in lock so file is not accessed on multiple threads
                    try
                    {
                        var cache = JsonConvert.SerializeObject(newSma);
                        File.WriteAllText(CachedFile, cache);
                    }
                    catch (Exception e)
                    {
                        Helpers.ConsolePrint(Tag, e.ToString());
                    }
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
            lock (_currentSma)
            {
                if (!_currentSma.ContainsKey(algo))
                    throw new ArgumentException("Algo not setup in SMA");
                _currentSma[algo].Paying = paying;
            }

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
            Helpers.ConsolePrint(Tag, sb.ToString());
        }

        #endregion

        # region Get Methods

        /// <summary>
        /// Attempt to get SMA for an algorithm
        /// </summary>
        /// <param name="algo">Algorithm</param>
        /// <param name="sma">Variable to place SMA in</param>
        /// <returns>True iff we know about this algo</returns>
        public static bool TryGetSma(AlgorithmType algo, out NiceHashSma sma)
        {
            CheckInit();
            lock (_currentSma)
            {
                if (_currentSma.ContainsKey(algo))
                {
                    sma = _currentSma[algo];
                    return true;
                }
            }

            sma = null;
            return false;
        }
        
        /// <summary>
        /// Attempt to get paying rate for an algorithm
        /// </summary>
        /// <param name="algo">Algorithm</param>
        /// <param name="sma">Variable to place paying in</param>
        /// <returns>True iff we know about this algo</returns>
        public static bool TryGetPaying(AlgorithmType algo, out double paying)
        {
            CheckInit();
            if (TryGetSma(algo, out var sma))
            {
                paying = sma.Paying;
                return true;
            }

            paying = default(double);
            return false;
        }

        #endregion

        #region Get Methods

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

            lock (_currentSma)
            {
                foreach (var kvp in _currentSma)
                {
                    if (_stableAlgorithms.Contains(kvp.Key) == stable)
                    {
                        dict[kvp.Key] = kvp.Value.Paying;
                    }
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

        #region Obsolete

        //[Obsolete]
        //public void AppendPayingForAlgo(AlgorithmType algo, double paying)
        //{
        //    if (algo >= 0 && _recentPaying.ContainsKey(algo))
        //    {
        //        if (_recentPaying[algo].Count >= ConfigManager.GeneralConfig.NormalizedProfitHistory || CurrentPayingForAlgo(algo) == 0)
        //        {
        //            _recentPaying[algo].RemoveAt(0);
        //        }
        //        _recentPaying[algo].Add(paying);
        //    }
        //}

        //[Obsolete]
        //public Dictionary<AlgorithmType, NiceHashSma> NormalizedSma()
        //{
        //    foreach (var algo in _recentPaying.Keys)
        //    {
        //        if (_currentSma.ContainsKey(algo))
        //        {
        //            var current = CurrentPayingForAlgo(algo);

        //            if (ConfigManager.GeneralConfig.NormalizedProfitHistory > 0
        //                && _recentPaying[algo].Count >= ConfigManager.GeneralConfig.NormalizedProfitHistory)
        //            {
        //                // Find IQR
        //                var quartiles = _recentPaying[algo].Quartiles();
        //                var IQR = quartiles.Item3 - quartiles.Item1;
        //                var TQ = quartiles.Item3;

        //                if (current > (IQR * ConfigManager.GeneralConfig.IQROverFactor) + TQ)
        //                {
        //                    // result is deviant over
        //                    var norm = (IQR * ConfigManager.GeneralConfig.IQRNormalizeFactor) + TQ;
        //                    Helpers.ConsolePrint("PROFITNORM",
        //                        $"Algorithm {_currentSma[algo].name} profit deviant, {(current - TQ) / IQR} IQRs over ({current} actual, {TQ} 3Q). Normalizing to {norm}");
        //                    _currentSma[algo].paying = norm;
        //                }
        //                else
        //                {
        //                    _currentSma[algo].paying = current;
        //                }
        //            }
        //            else
        //            {
        //                _currentSma[algo].paying = current;
        //            }
        //        }
        //    }

        //    return _currentSma;
        //}

        //[Obsolete]
        //private double CurrentPayingForAlgo(AlgorithmType algo)
        //{
        //    return _recentPaying.ContainsKey(algo) ? _recentPaying[algo].LastOrDefault() : 0;
        //}

        #endregion
    }
}

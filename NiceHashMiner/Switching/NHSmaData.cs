using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Switching
{
    public static class NHSmaData
    {
        public static bool Initialized { get; private set; }
        public static bool HasData { get; private set; }

        // private static Dictionary<AlgorithmType, List<double>> _recentPaying;

        // Global list of SMA data, should be accessed with a lock since callbacks/timers update it
        private static readonly Dictionary<AlgorithmType, NiceHashSma> _currentSma = new Dictionary<AlgorithmType, NiceHashSma>();
        // Global list of stable algorithms, should be accessed with a lock
        private static readonly HashSet<AlgorithmType> _stableAlgorithms = new HashSet<AlgorithmType>();

        public static void Initialize()
        {
            if (Initialized) return;

           // _recentPaying = new Dictionary<AlgorithmType, List<double>>();
            foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType)))
            {
                if (algo >= 0)
                {
                    _currentSma[algo] = new NiceHashSma
                    {
                        port = (int) algo + 3333,
                        name = algo.ToString().ToLower(),
                        algo = (int) algo,
                        paying = 0
                    };
                    //_recentPaying[algo] = new List<double>
                    //{
                    //    0
                    //};
                }
            }

            Initialized = true;
        }

        public static void UpdateSmaPaying(Dictionary<AlgorithmType, double> newSma)
        {
            lock (_currentSma)
            {
                foreach (var algo in newSma.Keys)
                {
                    if (_currentSma.ContainsKey(algo))
                    {
                        _currentSma[algo].paying = newSma[algo];
                    }
                }
            }

            HasData = true;
        }

        public static void UpdateStableAlgorithms(IEnumerable<AlgorithmType> algorithms)
        {
            lock (_stableAlgorithms)
            {
                foreach (var algo in algorithms)
                {
                    _stableAlgorithms.Add(algo);
                }
            }
        }

        public static bool TryGetSma(AlgorithmType algo, out NiceHashSma sma)
        {
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

        public static bool TryGetPaying(AlgorithmType algo, out double paying)
        {
            if (TryGetSma(algo, out var sma))
            {
                paying = sma.paying;
                return true;
            }

            paying = default(double);
            return false;
        }

        public static bool TryGetPayingWithTick(string devId, AlgorithmType algo, out double paying)
        {
            // TODO
            return TryGetPaying(algo, out paying);
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

using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner
{
    public class NiceHashSma
    {
        public int port;
        public string name;
        public int algo;
        public double paying;
    }

    public class NiceHashData
    {
        private readonly Dictionary<AlgorithmType, List<double>> _recentPaying;
        private readonly Dictionary<AlgorithmType, NiceHashSma> _currentSma;

        public NiceHashData()
        {
            _recentPaying = new Dictionary<AlgorithmType, List<double>>();
            var sma = new Dictionary<AlgorithmType, NiceHashSma>();
            foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType)))
            {
                if (algo >= 0)
                {
                    sma[algo] = new NiceHashSma
                    {
                        port = (int) algo + 3333,
                        name = algo.ToString().ToLower(),
                        algo = (int) algo,
                        paying = 0
                    };
                    _recentPaying[algo] = new List<double>
                    {
                        0
                    };
                }
            }
            _currentSma = sma;
        }

        public void AppendPayingForAlgo(AlgorithmType algo, double paying)
        {
            if (algo >= 0 && _recentPaying.ContainsKey(algo))
            {
                if (_recentPaying[algo].Count >= ConfigManager.GeneralConfig.NormalizedProfitHistory || CurrentPayingForAlgo(algo) == 0)
                {
                    _recentPaying[algo].RemoveAt(0);
                }
                _recentPaying[algo].Add(paying);
            }
        }

        public Dictionary<AlgorithmType, NiceHashSma> NormalizedSma()
        {
            foreach (var algo in _recentPaying.Keys)
            {
                if (_currentSma.ContainsKey(algo))
                {
                    var current = CurrentPayingForAlgo(algo);

                    if (ConfigManager.GeneralConfig.NormalizedProfitHistory > 0
                        && _recentPaying[algo].Count >= ConfigManager.GeneralConfig.NormalizedProfitHistory)
                    {
                        // Find IQR
                        var quartiles = _recentPaying[algo].Quartiles();
                        var IQR = quartiles.Item3 - quartiles.Item1;
                        var TQ = quartiles.Item3;

                        if (current > (IQR * ConfigManager.GeneralConfig.IQROverFactor) + TQ)
                        {
                            // result is deviant over
                            var norm = (IQR * ConfigManager.GeneralConfig.IQRNormalizeFactor) + TQ;
                            Helpers.ConsolePrint("PROFITNORM",
                                $"Algorithm {_currentSma[algo].name} profit deviant, {(current - TQ) / IQR} IQRs over ({current} actual, {TQ} 3Q). Normalizing to {norm}");
                            _currentSma[algo].paying = norm;
                        }
                        else
                        {
                            _currentSma[algo].paying = current;
                        }
                    }
                    else
                    {
                        _currentSma[algo].paying = current;
                    }
                }
            }

            return _currentSma;
        }

        private double CurrentPayingForAlgo(AlgorithmType algo)
        {
            return _recentPaying.ContainsKey(algo) ? _recentPaying[algo].LastOrDefault() : 0;
        }
    }
}

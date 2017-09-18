using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;
using NiceHashMiner.Configs;

namespace NiceHashMiner
{
    public class NiceHashSMA
    {
        public int port;
        public string name;
        public int algo;
        public double paying;
    }

    public class NiceHashData
    {
        private Dictionary<AlgorithmType, List<double>> recentPaying;
        private Dictionary<AlgorithmType, NiceHashSMA> currentSMA;
        public NiceHashData() {
            recentPaying = new Dictionary<AlgorithmType, List<double>>();
            var sma = new Dictionary<AlgorithmType, NiceHashSMA>();
            foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType))) {
                if (algo >= 0) {
                    sma[algo] = new NiceHashSMA {
                        port = (int)algo + 3333,
                        name = algo.ToString().ToLower(),
                        algo = (int)algo,
                        paying = 0
                    };
                    recentPaying[algo] = new List<double> { 0 };
                }
            }
            currentSMA = sma;
        }

        public void AppendPayingForAlgo(AlgorithmType algo, double paying) {
            if (algo >= 0 && recentPaying.ContainsKey(algo)) {
                if (recentPaying[algo].Count >= ConfigManager.GeneralConfig.NormalizedProfitHistory || currentPayingForAlgo(algo) == 0) {
                    recentPaying[algo].RemoveAt(0);
                }
                recentPaying[algo].Add(paying);
            }
        }

        public Dictionary<AlgorithmType, NiceHashSMA> NormalizedSMA() {
            foreach (AlgorithmType algo in recentPaying.Keys) {
                if (currentSMA.ContainsKey(algo)) {
                    var current = currentPayingForAlgo(algo);

                    if (ConfigManager.GeneralConfig.NormalizedProfitHistory > 0
                        && recentPaying[algo].Count >= ConfigManager.GeneralConfig.NormalizedProfitHistory) {
                        // Find IQR
                        var quartiles = recentPaying[algo].Quartiles();
                        var IQR = quartiles.Item3 - quartiles.Item1;
                        var TQ = quartiles.Item3;

                        if (current > (IQR * ConfigManager.GeneralConfig.IQROverFactor) + TQ) {  // result is deviant over
                            var norm = (IQR * ConfigManager.GeneralConfig.IQRNormalizeFactor) + TQ;
                            Helpers.ConsolePrint("PROFITNORM", String.Format("Algorithm {0} profit deviant, {1} IQRs over ({2} actual, {3} 3Q). Normalizing to {4}",
                                currentSMA[algo].name,
                                (current - TQ) / IQR,
                                current,
                                TQ,
                                norm));
                            currentSMA[algo].paying = norm;
                        } else {
                            currentSMA[algo].paying = current;
                        }
                    } else {
                        currentSMA[algo].paying = current;
                    }
                }
            }

            return currentSMA;
        }

        private double currentPayingForAlgo(AlgorithmType algo) {
            if (recentPaying.ContainsKey(algo))
                return recentPaying[algo].LastOrDefault();
            return 0;
        }
    }
}

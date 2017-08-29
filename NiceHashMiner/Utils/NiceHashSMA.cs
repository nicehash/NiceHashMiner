using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;

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
        private const double STD_PROF_MULT = 2.0;  // profit is considered deviant if it is this many std devs above average
        private const int PROF_HIST = 5;  // num of recent profits to consider for average

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
                if (recentPaying[algo].Count >= PROF_HIST || currentPayingForAlgo(algo) == 0) {
                    recentPaying[algo].RemoveAt(0);
                }
                recentPaying[algo].Add(paying);
            }
        }

        public Dictionary<AlgorithmType, NiceHashSMA> NormalizedSMA() {
            foreach (AlgorithmType algo in recentPaying.Keys) {
                if (currentSMA.ContainsKey(algo)) {
                    double avg = recentPaying[algo].Average();
                    double std = Math.Sqrt(recentPaying[algo].Average(v => Math.Pow(v - avg, 2)));
                    var current = currentPayingForAlgo(algo);

                    if (current > (std * STD_PROF_MULT) + avg) {  // result is deviant over
                        Helpers.ConsolePrint("PROFITNORM", String.Format("Algorithm {0} profit deviant, {1} std devs over ({2} over {3}",
                            currentSMA[algo].name,
                            (current - avg) / std,
                            current,
                            avg));
                        currentSMA[algo].paying = (std * STD_PROF_MULT) + avg;
                    }
                    currentSMA[algo].paying = current;
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

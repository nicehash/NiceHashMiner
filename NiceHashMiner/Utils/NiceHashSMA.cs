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

    public static class BaseNiceHashSMA
    {
        // Populate a base SMA dictionary with ports and names
        public static Dictionary<AlgorithmType, NiceHashSMA> BaseNiceHashSMADict {
            get {
                var sma = new Dictionary<AlgorithmType, NiceHashSMA>();
                foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType))) {
                    if (algo >= 0) {
                        sma[algo] = new NiceHashSMA {
                            port = (int)algo + 3333,
                            name = algo.ToString().ToLower(),
                            algo = (int)algo,
                            paying = 0
                        };
                    }
                }
                return sma;
            }
        }
    }
}

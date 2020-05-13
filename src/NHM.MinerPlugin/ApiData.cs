using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPlugin
{
    /// <summary>
    /// This class is used for saving of the data we get from miner API
    /// For speed data it has AlgorithmSpeedsTotal and AlgorithmSpeedsPerDevice properties
    /// For power usage data it has PowerUsageTotal and PowerUsagePerDevice properties
    /// </summary>
    [Serializable]
    public class ApiData
    {
        public IReadOnlyList<(AlgorithmType type, double speed)> AlgorithmSpeedsTotal()
        {
            try
            {
                List<(AlgorithmType type, double speed)> totalPairsSum = AlgorithmSpeedsPerDevice.Values.FirstOrDefault().Select(pair => (pair.type, 0.0)).ToList();
                foreach (var pairs in AlgorithmSpeedsPerDevice.Values)
                {
                    for (int i = 0; i < pairs.Count; i++)
                    {
                        totalPairsSum[i] = (totalPairsSum[i].type, totalPairsSum[i].speed + pairs[i].speed);
                    }
                }
                return totalPairsSum;
            }
            catch
            {
            }
            return new List<(AlgorithmType type, double speed)>();
        }

        public int PowerUsageTotal;
        // per device
        // key is device UUID
        public IReadOnlyDictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>> AlgorithmSpeedsPerDevice;
        public IReadOnlyDictionary<string, int> PowerUsagePerDevice;
        public string ApiResponse;
    }
}

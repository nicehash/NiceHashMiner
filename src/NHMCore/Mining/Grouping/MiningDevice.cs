using MinerPlugin;
using NHM.Common.Enums;
using System.Collections.Generic;

namespace NHMCore.Mining.Grouping
{
    public class MiningDevice
    {
        public MiningDevice(ComputeDevice device)
        {
            Device = device;

#if FORCE_MINING
            foreach (var algo in Device.AlgorithmSettings)
            {
                if (algo.Enabled)
                {
                    algo.BenchmarkSpeed = 1000;
                    Algorithms.Add(algo);
                }
            }
#else
            foreach (var algo in Device.AlgorithmSettings)
            {
                var isAlgoMiningCapable = GroupSetupUtils.IsAlgoMiningCapable(algo);
                if (isAlgoMiningCapable)
                {
                    Algorithms.Add(algo);
                }
            }
#endif
        }


        public ComputeDevice Device { get; }
        public List<AlgorithmContainer> Algorithms = new List<AlgorithmContainer>();

        public string GetMostProfitableString()
        {
            return MostProfitableAlgorithmStringID; ;
        }

        public string MostProfitableAlgorithmStringID { get; private set; } = "NONE";
        // prev state
        public string PrevProfitableAlgorithmStringID { get; private set; } = "NONE";

        private int GetMostProfitableIndex()
        {
            return Algorithms.FindIndex((a) => a.AlgorithmStringID == MostProfitableAlgorithmStringID);
        }

        private int GetPrevProfitableIndex()
        {
            return Algorithms.FindIndex((a) => a.AlgorithmStringID == PrevProfitableAlgorithmStringID);
        }

        public double GetCurrentMostProfitValue
        {
            get
            {
                var mostProfitableIndex = GetMostProfitableIndex();
                if (mostProfitableIndex > -1)
                {
                    return Algorithms[mostProfitableIndex].CurrentProfit;
                }

                return 0;
            }
        }

        public double GetPrevMostProfitValue
        {
            get
            {
                var mostProfitableIndex = GetPrevProfitableIndex();
                if (mostProfitableIndex > -1)
                {
                    return Algorithms[mostProfitableIndex].CurrentProfit;
                }

                return 0;
            }
        }

        public MiningPair GetMostProfitablePair()
        {
            var pAlgo = Algorithms[GetMostProfitableIndex()];
            return new MiningPair
            {
                Device = Device.BaseDevice,
                Algorithm = pAlgo.Algorithm
            };
        }

        public bool HasProfitableAlgo()
        {
            return GetMostProfitableIndex() > -1;
        }

        public void RestoreOldProfitsState()
        {
            // restore last state
            MostProfitableAlgorithmStringID = PrevProfitableAlgorithmStringID;
        }

        public void SetNotMining()
        {
            // device isn't mining (e.g. below profit threshold) so set state to none
            MostProfitableAlgorithmStringID = "NONE";
            PrevProfitableAlgorithmStringID = "NONE";
        }

        public void CalculateProfits(Dictionary<AlgorithmType, double> profits)
        {
            // save last state
            PrevProfitableAlgorithmStringID = MostProfitableAlgorithmStringID;
            // assume none is profitable
            MostProfitableAlgorithmStringID = "NONE";
            // calculate new profits
            foreach (var algo in Algorithms)
            {
                algo.UpdateCurProfit(profits);
            }

            // find max paying value and save key
            double maxProfit = double.NegativeInfinity;
            foreach (var algo in Algorithms)
            {
                if (maxProfit < algo.CurrentProfit)
                {
                    maxProfit = algo.CurrentProfit;
                    MostProfitableAlgorithmStringID = algo.AlgorithmStringID;
                }
            }
        }
    }
}

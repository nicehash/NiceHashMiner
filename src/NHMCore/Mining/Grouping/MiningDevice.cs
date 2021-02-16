using NHM.Common.Enums;
using NHMCore.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Mining.Grouping
{
    public class MiningDevice
    {
        public MiningDevice(ComputeDevice device)
        {
            Device = device;

            foreach (var algo in Device.AlgorithmSettings)
            {
                var isAlgoMiningCapable = GroupSetupUtils.IsAlgoMiningCapable(algo);
                if (isAlgoMiningCapable)
                {
                    Algorithms.Add(algo);
                }
            }
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
                    return Algorithms[mostProfitableIndex].CurrentNormalizedProfit;
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
                    return Algorithms[mostProfitableIndex].CurrentNormalizedProfit;
                }

                return 0;
            }
        }

        public AlgorithmContainer GetMostProfitableAlgorithmContainer()
        {
            return Algorithms[GetMostProfitableIndex()];
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
                algo.UpdateCurrentNormalizedProfit(profits);
            }

            // find max paying value and save key
            double maxProfit = double.NegativeInfinity;
            var validAlgorithms = Algorithms.Where(algo => algo.IgnoreUntil <= DateTime.UtcNow);
            if (validAlgorithms.Count() == 0)
            {
                AvailableNotifications.CreateNoAvailableAlgorithmsInfo();
            }
            foreach (var algo in validAlgorithms)
            {
                if (maxProfit < algo.CurrentNormalizedProfit)
                {
                    maxProfit = algo.CurrentNormalizedProfit;
                    MostProfitableAlgorithmStringID = algo.AlgorithmStringID;
                }
            }
        }
    }
}

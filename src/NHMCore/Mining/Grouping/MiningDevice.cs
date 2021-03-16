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
            Algorithms = device.AlgorithmSettings.Where(GroupSetupUtils.IsAlgoMiningCapable).ToArray();
        }

        public static bool ShouldUpdate(MiningDevice miningDevice)
        {
            if (miningDevice == null) return true;
            var newAlgorithms = miningDevice.Device.AlgorithmSettings.Where(GroupSetupUtils.IsAlgoMiningCapable).ToArray();
            var curAlgorithms = miningDevice.Algorithms;
            
            var firstNotSecond = newAlgorithms.Except(curAlgorithms).ToArray();
            if (firstNotSecond.Any()) return true;

            var secondNotFirst = curAlgorithms.Except(newAlgorithms).ToArray();
            return secondNotFirst.Any();
        }

        public ComputeDevice Device { get; }
        public IReadOnlyList<AlgorithmContainer> Algorithms { get; }

        public string MostProfitableAlgorithmStringID { get; private set; } = "NONE";
        // prev state
        public string PrevProfitableAlgorithmStringID { get; private set; } = "NONE";

        public double GetCurrentMostProfitValue
        {
            get
            {
                var mostProfitable = GetMostProfitableAlgorithmContainer();
                return mostProfitable?.CurrentNormalizedProfit ?? 0.0;
            }
        }

        public double GetPrevMostProfitValue
        {
            get
            {
                var prevMostProfitable = Algorithms.FirstOrDefault((a) => a.AlgorithmStringID == PrevProfitableAlgorithmStringID);
                return prevMostProfitable?.CurrentNormalizedProfit ?? 0.0;
            }
        }

        public AlgorithmContainer GetMostProfitableAlgorithmContainer()
        {
            return Algorithms.FirstOrDefault(a => a.AlgorithmStringID == MostProfitableAlgorithmStringID);
        }

        public bool HasProfitableAlgo()
        {
            return GetMostProfitableAlgorithmContainer() != null;
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
            // calculate new profits
             foreach (var algo in Algorithms) algo.UpdateCurrentNormalizedProfit(profits);

            // find max paying value and save key
            var mostProfitable = Algorithms.Where(algo => algo.IgnoreUntil <= DateTime.UtcNow)
                .OrderByDescending(algo => algo.CurrentNormalizedProfit)
                .FirstOrDefault();

            if (mostProfitable == null)
            {
                AvailableNotifications.CreateNoAvailableAlgorithmsInfo();
                MostProfitableAlgorithmStringID = "NONE";
            }
            else
            {
                MostProfitableAlgorithmStringID = mostProfitable.AlgorithmStringID;
            }
        }
    }
}

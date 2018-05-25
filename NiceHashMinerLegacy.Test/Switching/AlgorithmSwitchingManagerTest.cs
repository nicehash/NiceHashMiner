using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMinerLegacy.Tests.Switching
{
    [TestClass]
    public class AlgorithmSwitchingManagerTest
    {
        private static readonly Dictionary<AlgorithmType, double> StartPaying = new Dictionary<AlgorithmType, double>();

        private static readonly AlgorithmType[] TestStables =
        {
            AlgorithmType.DaggerHashimoto,
            AlgorithmType.Lbry,
            AlgorithmType.Sia,
            AlgorithmType.Equihash
        };

        private static readonly Random R = new Random();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType)))
            {
                if (algo > 0)
                {
                    StartPaying[algo] = R.NextDouble();
                }
            }

            NHSmaData.InitializeIfNeeded();
            NHSmaData.UpdateSmaPaying(StartPaying);

            NHSmaData.UpdateStableAlgorithms(TestStables);
        }

        [TestMethod]
        public void SwitchingManager_ShouldReportNewProfitAfterTicks()
        {
            var higherPayings = new Dictionary<AlgorithmType, double>();

            foreach (var key in StartPaying.Keys)
            {
                higherPayings[key] = StartPaying[key] + 0.000001;
            }

            var manager = new AlgorithmSwitchingManager();
            manager.SmaCheckTimerOnElapsed(null, null);
            NHSmaData.UpdateSmaPaying(higherPayings);

            for (var i = 1; i <= AlgorithmSwitchingManager.MaxHistory + 5; i++)
            {
                manager.SmaCheckTimerOnElapsed(null, null);
                foreach (var algo in higherPayings.Keys)
                {
                    var paying = manager.LastPayingForAlgo(algo);

                    Assert.AreEqual(NHSmaData.IsAlgorithmStable(algo), TestStables.Contains(algo));

                    var range = NHSmaData.IsAlgorithmStable(algo)
                        ? AlgorithmSwitchingManager.StableRange
                        : AlgorithmSwitchingManager.UnstableRange;
                    
                    if (i < range.Lower)
                    {
                        // We are below the interval for this algo to have updated
                        Assert.AreEqual(StartPaying[algo], paying);
                    }
                    else if (i >= range.Upper)
                    {
                        // We are above the max ticks for this algo to be updated
                        Assert.AreEqual(higherPayings[algo], paying);
                    }
                }
            }
        }

        [TestMethod]
        public void SwitchingManager_ShouldAlwaysReportLowerProfit()
        {
            var manager = new AlgorithmSwitchingManager();
            manager.SmaCheckTimerOnElapsed(null, null);

            var currentPaying = new Dictionary<AlgorithmType, double>();

            for (var i = 0; i < AlgorithmSwitchingManager.MaxHistory + 10; i++)
            {
                foreach (var algo in StartPaying.Keys)
                {
                    currentPaying[algo] = manager.LastPayingForAlgo(algo);
                    // Randomly add or subtract
                    NHSmaData.UpdatePayingForAlgo(algo, StartPaying[algo] + (R.NextDouble() - 0.5));
                }

                manager.SmaCheckTimerOnElapsed(null, null);

                // Iterate again to check
                foreach (var algo in StartPaying.Keys)
                {
                    Assert.IsTrue(NHSmaData.TryGetPaying(algo, out var paying));
                    if (paying <= currentPaying[algo])
                    {
                        // New value was less/same, so normalized value should be that
                        Assert.AreEqual(paying, manager.LastPayingForAlgo(algo));
                    }
                    else
                    {
                        // New value was more, so normalized should be equal or greater (depending on ticks)
                        Assert.IsTrue(manager.LastPayingForAlgo(algo) >= currentPaying[algo]);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMinerLegacy.Tests.Switching
{
    [TestClass]
    public class NHSmaDataTest
    {
        [TestMethod]
        public void Paying_ShouldReturnCorrectAndFlagsAreSet()
        {
            var testPaying = new Dictionary<AlgorithmType, double>
            {
                {AlgorithmType.CryptoNight, 0.11},
                {AlgorithmType.DaggerHashimoto, 0.9},
                {AlgorithmType.Blake2s, 0}
            };

            var testZero = new List<AlgorithmType>
            {
                AlgorithmType.Keccak,
                AlgorithmType.Equihash,
                AlgorithmType.Pascal
            };

            // Check initialized flag and initialize
            NHSmaData.Initialize();
            Assert.IsTrue(NHSmaData.Initialized);

            // Check hasdata flag and update with test data
            NHSmaData.UpdateSmaPaying(testPaying);
            Assert.IsTrue(NHSmaData.HasData);

            foreach (var algo in testPaying.Keys)
            {
                // Every key from test dict should return true and paying
                Assert.IsTrue(NHSmaData.TryGetPaying(algo, out var paying));
                Assert.AreEqual(testPaying[algo], paying);
            }

            foreach (var algo in testZero)
            {
                // These algos were not set so their value should be 0 (but still return true)
                Assert.IsTrue(NHSmaData.TryGetPaying(algo, out var paying));
                Assert.AreEqual(0, paying);
            }

            // Should be false since DaggerDecred does not have a valid SMA
            Assert.IsFalse(NHSmaData.TryGetPaying(AlgorithmType.DaggerDecred, out _));
        }

        [TestMethod]
        public void Stable_ShouldReturnCorrect()
        {
            // We will update the stable algos multiple times
            var testStable = new List<List<AlgorithmType>>
            {
                new List<AlgorithmType>
                {
                    AlgorithmType.Keccak,
                    AlgorithmType.Equihash,
                    AlgorithmType.Pascal
                },
                new List<AlgorithmType>
                {
                    AlgorithmType.Keccak,
                    AlgorithmType.Equihash,
                    AlgorithmType.Pascal,
                    AlgorithmType.Blake2s
                },
                new List<AlgorithmType>
                {
                    AlgorithmType.DaggerHashimoto,
                    AlgorithmType.Equihash,
                    AlgorithmType.Pascal
                }
            };
            
            NHSmaData.Initialize();

            foreach (var epoch in testStable)
            {
                NHSmaData.UpdateStableAlgorithms(epoch);

                // Test over all algo types
                foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType)))
                {
                    var stable = epoch.Contains(algo);
                    Assert.AreEqual(stable, NHSmaData.IsAlgorithmStable(algo));
                }

            }
        }
    }
}

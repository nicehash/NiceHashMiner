using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMinerLegacy.Tests.Devices.Algorithms
{
    [TestClass]
    public class DefaultAlgorithmsTest
    {
        [TestMethod]
        public void All_ShouldContainAll()
        {
            AssertContainsAll(DefaultAlgorithms.Cpu);
            AssertContainsAll(DefaultAlgorithms.Amd);
            AssertContainsAll(DefaultAlgorithms.Nvidia);
        }

        [TestMethod]
        public void Gpus_ShouldContainGpu()
        {
            AssertContainsGpu(DefaultAlgorithms.Amd);
            AssertContainsGpu(DefaultAlgorithms.Nvidia);
        }

        private static void AssertContainsAll(IReadOnlyDictionary<MinerBaseType, List<Algorithm>> algos)
        {
            Assert.IsTrue(algos.ContainsKey(MinerBaseType.XmrStak));
            var xrmStakAlgos = algos[MinerBaseType.XmrStak];
            Assert.AreEqual(2, xrmStakAlgos.Count);

            AssertAlgorithmsEqual(xrmStakAlgos[0], MinerBaseType.XmrStak, AlgorithmType.CryptoNightV7);
        }

        private static void AssertContainsGpu(IReadOnlyDictionary<MinerBaseType, List<Algorithm>> algos)
        {
            Assert.IsTrue(algos.ContainsKey(MinerBaseType.Claymore));
            var claymoreAlgos = algos[MinerBaseType.Claymore]
                .Where(a => a.NiceHashID != AlgorithmType.CryptoNightV7 && a.NiceHashID != AlgorithmType.Equihash)
                .ToList();
            Assert.AreEqual(7, claymoreAlgos.Count);

            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.NONE,
                AlgorithmType.Decred,
                AlgorithmType.Lbry,
                AlgorithmType.Pascal,
                AlgorithmType.Sia,
                AlgorithmType.Blake2s,
                AlgorithmType.Keccak
            };

            foreach (var algo in claymoreAlgos)
            {
                AssertAlgorithmsEqual(algo, MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto);
                Assert.IsTrue(secondaryList.Any(a => algo.SecondaryNiceHashID == a));
            }
        }

        private static void AssertAlgorithmsEqual(Algorithm algo, MinerBaseType minerType, AlgorithmType algoType)
        {
            Assert.AreEqual(minerType, algo.MinerBaseType);
            Assert.AreEqual(algoType, algo.NiceHashID);
        }
    }
}

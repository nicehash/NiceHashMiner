using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMinerLegacy.Tests.Utils
{
    [TestClass]
    public class AlgorithmNiceHashNamesTest
    {
        [TestMethod]
        public void ShouldReturnNotFoundForInvalid()
        {
            var x = AlgorithmNiceHashNames.GetName((AlgorithmType) 100);

            Assert.AreEqual("NameNotFound type not supported", x);
        }

        [TestMethod]
        public void ShouldReturnNameForValid()
        {
            var expected = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.Blake2s, "Blake2s" },
                { AlgorithmType.CryptoNightV8, "CryptoNightV8" }
            };

            foreach (var key in expected.Keys)
            {
                Assert.AreEqual(expected[key], AlgorithmNiceHashNames.GetName(key));
            }
        }
    }
}

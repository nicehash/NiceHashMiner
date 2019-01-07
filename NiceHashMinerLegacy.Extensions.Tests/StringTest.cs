using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NiceHashMinerLegacy.Extensions.Tests
{
    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void GetHashrateAfterShouldParse()
        {
            // xmr-stak
            var hash = "[2019-01-06 21:18:33] : Benchmark Total: 756.2 H/S".GetHashrateAfter("Benchmark Total:");
            Assert.AreEqual(756.2, hash);

            // Claymore 
            hash = "ETH - Total Speed: 32.339 Mh/s, Total Shares: 0, Rejected: 0, Time: 00:00".GetHashrateAfter("Total Speed:");
            Assert.AreEqual(32339000, hash);
        }

        [TestMethod]
        public void GetHashrateAfterShouldReturnNull()
        {
            var hash = "[2019-01-06 21:18:33] : Benchmark Total: --- H/S".GetHashrateAfter("Benchmark Total:");
            Assert.IsNull(hash);
        }
    }
}

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
            var hash = "[2019-01-06 21:18:33] : Benchmark Total: 756.2 H/S".GetHashrateAfter("Benchmark Total:");
            Assert.AreEqual(756.2, hash);
        }
    }
}

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
            "[2019-01-06 21:18:33] : Benchmark Total: 756.2 H/S".TryGetHashrateAfter("Benchmark Total:", out var hash);
            Assert.AreEqual(756.2, hash);

            // Claymore 
            "ETH - Total Speed: 32.339 Mh/s, Total Shares: 0, Rejected: 0, Time: 00:00"
                .TryGetHashrateAfter("Total Speed:", out hash);
            Assert.AreEqual(32339000, hash);

            // T-rex
            "20190109 23:53:41 Total: 71.89 MH/s".TryGetHashrateAfter("Total:", out hash);
            Assert.AreEqual(71890000, hash);

            // GMiner
            "15:14:18 Total Speed: 27 Sol/s Shares Accepted: 0 Rejected: 0 Power: 199W 0.14 Sol/W"
                .TryGetHashrateAfter("Total Speed:", out hash);
            Assert.AreEqual(27, hash);
        }

        [TestMethod]
        public void GetHashrateAfterShouldReturnNull()
        {
            Assert.IsFalse("[2019-01-06 21:18:33] : Benchmark Total: --- H/S"
                .TryGetHashrateAfter("Benchmark Total:", out var hash));
            Assert.AreEqual(0, hash);
        }
    }
}

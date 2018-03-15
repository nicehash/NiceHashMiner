using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Enums;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Tests.Stats;

namespace NiceHashMinerLegacy.Tests.Algorithms
{
    [TestClass]
    public class AlgorithmTest
    {
        // All arbitrary
        private const double PowerUsage = 245.80;
        private const AlgorithmType Algo = AlgorithmType.Equihash;
        private const MinerBaseType Miner = MinerBaseType.EWBF;
        private const double Speed = 3654181.233518;

        private static Algorithm _algorithm;
        private static double _kwhInBtc;

        private static readonly Dictionary<AlgorithmType, double> Profits = new Dictionary<AlgorithmType, double>
        {
            {Algo, 0.32548}
        };

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            // Use this to init exchange rates
            ExchangeRateApiTest.Initialize(context);
            _kwhInBtc = ExchangeRateApi.GetKwhPriceInBtc();
            _algorithm = new Algorithm(Miner, Algo, "")
            {
                AvaragedSpeed = Speed
            };
        }

        [TestMethod]
        public void Algorithm_ShouldSubtractPower()
        {
            _algorithm.UpdateCurProfit(Profits);
            var expectedProfit = Profits[Algo] * Speed * 0.000000001 -
                                 _kwhInBtc / 1000 * PowerUsage * 24;
            Assert.AreEqual(expectedProfit, _algorithm.CurrentProfit);
        }
    }
}

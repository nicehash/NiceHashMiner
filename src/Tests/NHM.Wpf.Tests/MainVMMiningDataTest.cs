using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHM.Common.Enums;
using NHM.Wpf.ViewModels.Models;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Stats;
using System.Collections.Generic;

namespace NHM.Wpf.Tests
{
    [TestClass]
    public class MainVMMiningDataTest
    {
        [TestMethod]
        public void NumbersShouldFactorCorrectly()
        {
            const double rate = 4.0365457518060869E-05;
            const double speed = 57644160;

            var stats = new MiningStats.DeviceMiningStats
            {
                Rates = new List<(AlgorithmType type, double rate)>
                {
                    (AlgorithmType.Lyra2REv3, rate)
                },
                Speeds = new List<(AlgorithmType type, double speed)>
                {
                    (AlgorithmType.Lyra2REv3, speed)
                },
                PowerUsageAPI = 300,
                PowerUsageDeviceReading = 280.435
            };

            var data = new MiningData(new ComputeDevice(null, 0, ""));
            data.Stats = stats;

            Assert.AreEqual(speed, data.Hashrate);
            Assert.AreEqual(rate, data.Payrate);
            Assert.AreEqual(300, data.PowerUsage);

            ExchangeRateApi.UsdBtcRate = 10000;
            ConfigManager.GeneralConfig.KwhPrice = 0.1;

            Assert.AreEqual(rate * 10000, data.FiatPayrate);

            // 300W uses 0.3 * 24 = 7.2 kWh per day
            // At 10 cents per kWh that is 72 cents
            Assert.AreEqual(0.72, data.PowerCost, 0.00005);
            Assert.AreEqual(rate * 10000 - 0.72, data.Profit, 0.00005);
        }
    }
}

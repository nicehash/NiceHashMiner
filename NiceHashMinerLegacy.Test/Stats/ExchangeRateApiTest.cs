using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Configs;
using NiceHashMiner.Configs.Data;
using NiceHashMiner.Stats;
using System;
using System.Collections.Generic;

namespace NiceHashMinerLegacy.Tests.Stats
{
    [TestClass]
    public class ExchangeRateApiTest
    {
        private const string Currency = "CAD";
        private const double UsdBtcRate = 12012.87;
        private const double KwhPrice = 2.12;
        private const double DoubleAccuracy = 0.000000001;

        private static readonly Dictionary<string, double> FiatExchanges = new Dictionary<string, double>
        {
            {Currency, 1.31}
        };

        private static readonly Random R = new Random();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            ConfigManager.GeneralConfig = new GeneralConfig
            {
                DisplayCurrency = Currency,
                KwhPrice = KwhPrice
            };

            // Add exchange info
            ExchangeRateApi.UsdBtcRate = UsdBtcRate;
            ExchangeRateApi.UpdateExchangesFiat(FiatExchanges);
            ExchangeRateApi.ActiveDisplayCurrency = Currency;
        }

        [TestCleanup]
        public void Cleanup()
        {
            ExchangeRateApi.ActiveDisplayCurrency = Currency;
        }

        [TestMethod]
        public void Exchange_ShouldMatchInitialized()
        {
            Assert.AreEqual(UsdBtcRate, ExchangeRateApi.GetUsdExchangeRate());

            const double testAmountInUsd = 3256.85;
            var testInCur = testAmountInUsd * FiatExchanges[Currency];
            Assert.AreEqual(testInCur, ExchangeRateApi.ConvertToActiveCurrency(testAmountInUsd));
        }

        [TestMethod]
        public void Exchange_ShouldFallOnUsdWhenUnknown()
        {
            // Set to unknown currency
            ExchangeRateApi.ActiveDisplayCurrency = "ETH";
            var testAmount = R.NextDouble() * 20000;

            Assert.AreEqual(testAmount, ExchangeRateApi.ConvertToActiveCurrency(testAmount), DoubleAccuracy);
            Assert.AreEqual("USD", ExchangeRateApi.ActiveDisplayCurrency);
            Assert.AreEqual(testAmount, ExchangeRateApi.ConvertToActiveCurrency(testAmount), DoubleAccuracy);
        }

        [TestMethod]
        public void Exchange_KwhPrice_ShouldMatch()
        {
            var kwhPriceInUsd = KwhPrice / FiatExchanges[Currency];
            var kwhPriceInBtc = kwhPriceInUsd / UsdBtcRate;
            Assert.AreEqual(kwhPriceInBtc, ExchangeRateApi.GetKwhPriceInBtc(), DoubleAccuracy);
        }

        [TestMethod]
        public void Exchange_KwhPrice_ShouldBe0For0Exchange()
        {
            ExchangeRateApi.UpdateExchangesFiat(new Dictionary<string, double>
            {
                {"HMB", 0}
            });
            ExchangeRateApi.ActiveDisplayCurrency = "HMB";
            Assert.AreEqual(0, ExchangeRateApi.GetKwhPriceInBtc());
        }
    }
}

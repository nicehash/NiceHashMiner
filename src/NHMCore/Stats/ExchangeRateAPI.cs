using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NHMCore.Configs;
using NHM.Common;

namespace NHMCore.Stats
{
    public static class ExchangeRateApi
    {
        private static readonly ConcurrentDictionary<string, double> ExchangesFiat = new ConcurrentDictionary<string, double>();
        private static double _usdBtcRate = -1;

        public static event EventHandler<string> CurrencyChanged;
        public static event EventHandler ExchangeChanged;

        public static double UsdBtcRate
        {
            // Access in thread-safe way
            private get => Interlocked.Exchange(ref _usdBtcRate, _usdBtcRate);
            set
            {
                if (value > 0)
                {
                    Interlocked.Exchange(ref _usdBtcRate, value);
                    Logger.Info("ExchangeRateApi", $"USD rate updated: {value} BTC");
                }
            }
        }

        public static double SelectedCurrBtcRate => ConvertToActiveCurrency(UsdBtcRate);

        private static string _activeDisplayCurrency = "USD";
        public static string ActiveDisplayCurrency
        {
            get => _activeDisplayCurrency;
            set
            {
                _activeDisplayCurrency = value;
                CurrencyChanged?.Invoke(null, value);
            }
        }

        private static bool ConverterActive => ConfigManager.GeneralConfig.DisplayCurrency != "USD";

        static ExchangeRateApi()
        {
            ConfigManager.GeneralConfig.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(ConfigManager.GeneralConfig.DisplayCurrency))
                    ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;
            };
        }

        public static void UpdateExchangesFiat(Dictionary<string, double> newExchanges)
        {
            if (newExchanges == null) return;
            foreach (var key in newExchanges.Keys)
            {
                ExchangesFiat.AddOrUpdate(key, newExchanges[key], (k, v) => newExchanges[k]);
            }

            ExchangeChanged?.Invoke(null, EventArgs.Empty);
        }

        public static double ConvertToActiveCurrency(double amount)
        {
            if (!ConverterActive)
            {
                return amount;
            }

            // if we are still null after an update something went wrong. just use USD hopefully itll update next tick
            if (ExchangesFiat.Count == 0 || ActiveDisplayCurrency == "USD")
            {
                return amount;
            }

            //Helpers.ConsolePrint("CurrencyConverter", "Current Currency: " + ConfigManager.Instance.GeneralConfig.DisplayCurrency);
            if (ExchangesFiat.TryGetValue(ActiveDisplayCurrency, out var usdExchangeRate))
                return amount * usdExchangeRate;

            Logger.Info("ExchangeRateApi", $"Unknown Currency Tag: {ActiveDisplayCurrency}, falling back to USD rates");
            ActiveDisplayCurrency = "USD";
            return amount;
        }

        public static double ConvertFromBtc(double amount)
        {
            return ConvertToActiveCurrency(amount * GetUsdExchangeRate());
        }

        public static string GetCurrencyString(double amount)
        {
            return ConvertToActiveCurrency(amount * GetUsdExchangeRate())
                       .ToString("F2", CultureInfo.InvariantCulture)
                   + $" {ActiveDisplayCurrency}/"
                   + Translations.Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
        }

        public static double GetUsdExchangeRate()
        {
            return UsdBtcRate > 0 ? UsdBtcRate : 0.0;
        }

        /// <summary>
        /// Get price of kW-h in BTC if it is set and exchanges are working
        /// Otherwise, returns 0
        /// </summary>
        public static double GetKwhPriceInBtc()
        {
            var price = ConfigManager.GeneralConfig.KwhPrice;
            if (price <= 0) return 0;
            // Converting with 1/price will give us 1/usdPrice
            var invertedUsdRate = ConvertToActiveCurrency(1 / price);
            if (invertedUsdRate <= 0)
            {
                // Should never happen, indicates error in ExchangesFiat
                // Fall back with 0
                Logger.Info("EXCHANGE", "Exchange for currency is 0, power switching disabled.");
                return 0;
            }
            // Make price in USD
            price = 1 / invertedUsdRate;
            // Race condition not a problem since UsdBtcRate will never update to 0
            if (UsdBtcRate <= 0)
            {
                Logger.Info("EXCHANGE", "Bitcoin price is unknown, power switching disabled");
                return 0;
            }
            return price / UsdBtcRate;
        }

        //public static double GetKwhPriceInFiat()
        //{
        //    var price = ConfigManager.GeneralConfig.KwhPrice;
        //    return price > 0 ? price : 0;
        //}
    }
}

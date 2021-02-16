using NHM.Common;
using NHMCore.Configs;
using NHMCore.Configs.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace NHMCore.ApplicationState
{
    public class BalanceAndExchangeRates : NotifyChangedBase
    {
        private readonly ConcurrentDictionary<string, double> ExchangesFiat = new ConcurrentDictionary<string, double>();

        public static BalanceAndExchangeRates Instance { get; } = new BalanceAndExchangeRates();

        private BalanceAndExchangeRates()
        {
            // this will not work since we change the instance
            //Configs.ConfigManager.GeneralConfig.PropertyChanged += GeneralConfig_PropertyChanged;
        }
        // TODO make some sort of an interface where we keep track of all instances that need this handler
        public void GeneralConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(GeneralConfig.AutoScaleBTCValues):
                    // trigger display strings change
                    SetBTCScaledProperties();
                    break;
                default:
                    break;
            }
        }

        internal void UpdateExchangesFiat(double usdBtcRate, Dictionary<string, double> newExchanges)
        {
            UsdBtcRate = usdBtcRate;
            if (newExchanges != null)
            {
                _fiatCurrencyKeys = newExchanges.Keys.ToList();
                _fiatCurrencyKeys.Add("USD");
                _fiatCurrencyKeys.Sort();
                OnPropertyChanged(nameof(HasFiatCurrencyOptions));
                OnPropertyChanged(nameof(FiatCurrencyOptions));
                //OnPropertyChanged(nameof(SelectedFiatCurrency)); // ????
                foreach (var key in newExchanges.Keys)
                {
                    ExchangesFiat.AddOrUpdate(key, newExchanges[key], (k, v) => newExchanges[k]);
                }
            }
            CalculateFiatBalance();
        }
        private double _usdBtcRate = -1;
        private double UsdBtcRate
        {
            // Access in thread-safe way
            get => Interlocked.Exchange(ref _usdBtcRate, _usdBtcRate);
            set
            {
                if (value > 0)
                {
                    Interlocked.Exchange(ref _usdBtcRate, value);
                    Logger.Info("ExchangeRateApi", $"USD rate updated: {value} BTC");
                }
            }
        }

        // if no login we have no balance
        private double? _btcBalance = null;
        public double? BtcBalance
        {
            get => _btcBalance;
            internal set
            {
                _btcBalance = value;
                // TODO calc Fiat balance
                OnPropertyChanged(nameof(BtcBalance));
                CalculateFiatBalance();
                SetBTCScaledProperties();
            }
        }

        // we need to be logined and have fiat exchange rates as well
        private double? _fiatBalance = null;
        public double? FiatBalance
        {
            get => _fiatBalance;
            private set
            {
                _fiatBalance = value;
                OnPropertyChanged(nameof(FiatBalance));
            }
        }

        private void CalculateFiatBalance()
        {
            if (BtcBalance.HasValue)
            {
                var usdAmount = (BtcBalance * GetUsdExchangeRate()) ?? 0;
                FiatBalance = ConvertToActiveCurrency(usdAmount);
                // set display
                DisplayFiatBalance = $"≈ {(FiatBalance ?? 0):F2} {SelectedFiatCurrency}";
            }
            else
            {
                DisplayFiatBalance = "";
            }
            OnPropertyChanged(nameof(DisplayFiatBalance));
            OnPropertyChanged(nameof(ExchangeTooltip));
        }

        public bool HasFiatCurrencyOptions => _fiatCurrencyKeys.Count > 0;
        public IReadOnlyList<string> FiatCurrencyOptions => _fiatCurrencyKeys;
        private List<string> _fiatCurrencyKeys = new List<string> { };

        public bool HasSelectedFiatCurrency => _fiatCurrencyKeys.Contains(SelectedFiatCurrency);
        private string _fiatCurrency { get; set; } = "USD";
        public string SelectedFiatCurrency
        {
            get => _fiatCurrency;
            set
            {
                // TODO make sure you can set this fiat currency
                _fiatCurrency = value;
                CalculateFiatBalance();
            }
        }
        //public bool ContainsFiatCurrency { get; set; }

        // BTC Display balance stuff coresponds to the scaling
        private void SetBTCScaledProperties()
        {
            if (GUISettings.Instance.AutoScaleBTCValues && _btcBalance < 0.1)
            {
                var scaled = (_btcBalance ?? 0) * 1000;
                DisplayBTCBalance = $"{scaled:F5}";
                DisplayBTCSymbol = "mBTC";
            }
            else
            {
                DisplayBTCBalance = $"{(_btcBalance ?? 0):F8}";
                DisplayBTCSymbol = "BTC";
            }
            OnPropertyChanged(nameof(DisplayBTCBalance));
            OnPropertyChanged(nameof(DisplayBTCSymbol));
        }

        public string DisplayBTCBalance { get; private set; }
        public string DisplayBTCSymbol { get; private set; }
        public string DisplayFiatBalance { get; private set; }

        // TODO maybe rename
        public string ExchangeTooltip => $"1 BTC = {ConvertToActiveCurrency(UsdBtcRate):F2} {SelectedFiatCurrency}";


        #region COPY/PASTE from NHMCore.Stats.ExchangeRateApi

        private bool ConverterActive => SelectedFiatCurrency != "USD";
        // TODO change return to (double convertedAmount, bool ok)
        public double ConvertToActiveCurrency(double usdAmount)
        {
            if (!ConverterActive)
            {
                return usdAmount;
            }

            // if we are still null after an update something went wrong. just use USD hopefully itll update next tick
            if (ExchangesFiat.Count == 0 || SelectedFiatCurrency == "USD")
            {
                return usdAmount;
            }

            //Helpers.ConsolePrint("CurrencyConverter", "Current Currency: " + ConfigManager.Instance.GeneralConfig.DisplayCurrency);
            if (ExchangesFiat.TryGetValue(SelectedFiatCurrency, out var usdExchangeRate))
                return usdAmount * usdExchangeRate;

            // TODO Don't FALLBACK to USD exchange. if we can't get the converted values then this should break
            Logger.Info("ExchangeRateApi", $"Unknown Currency Tag: {SelectedFiatCurrency}, falling back to USD rates");
            SelectedFiatCurrency = "USD";
            return usdAmount;
        }

        public double ConvertFromBtc(double amount)
        {
            return ConvertToActiveCurrency(amount * GetUsdExchangeRate());
        }

        public double GetUsdExchangeRate()
        {
            return UsdBtcRate > 0 ? UsdBtcRate : 0.0;
        }

        // TODO these exchange not working functions might cause problems
        /// <summary>
        /// Get price of kW-h in BTC if it is set and exchanges are working
        /// Otherwise, returns 0
        /// </summary>
        public double GetKwhPriceInBtc()
        {
            var price = SwitchSettings.Instance.KwhPrice;
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
            var usdBtcRate = GetUsdExchangeRate();
            if (usdBtcRate <= 0)
            {
                Logger.Info("EXCHANGE", "Bitcoin price is unknown, power switching disabled");
                return 0;
            }
            return price / usdBtcRate;
        }

        #endregion COPY/PASTE from NHMCore.Stats.ExchangeRateApi
    }
}

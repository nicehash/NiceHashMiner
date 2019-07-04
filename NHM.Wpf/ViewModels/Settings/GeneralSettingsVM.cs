using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels.Settings
{
    public class GeneralSettingsVM : SettingsBaseVM
    {
        // Placeholders
        public IReadOnlyList<string> CurrencyOptions { get; }
        public IReadOnlyList<string> LanguageOptions { get; }

        // More placeholders
        public string SelectedCurrency { get; set; } = "CAD";
        public int SelectedLangIndex { get; set; } = 0;
        private bool _logToFile = false;

        public bool LogToFile
        {
            get => _logToFile;
            set
            {
                _logToFile = value;
                OnPropertyChanged();
            }
        }

        public GeneralSettingsVM(object settingsObj)
            : base(settingsObj, "General")
        {
            CurrencyOptions = new List<string>
            {
                "AUD",
                "BGN",
                "BRL",
                "CAD",
                "CHF",
                "CNY",
                "CZK",
                "DKK",
                "EUR",
                "GBP",
                "HKD",
                "HRK",
                "HUF",
                "IDR",
                "ILS",
                "INR",
                "JPY",
                "KRW",
                "MXN",
                "MYR",
                "NOK",
                "NZD",
                "PHP",
                "PLN",
                "RON",
                "RUB",
                "SEK",
                "SGD",
                "THB",
                "TRY",
                "USD",
                "ZAR"
            };

            LanguageOptions = new List<string>
            {
                "English",
                "Russian"
            };
        }
    }
}

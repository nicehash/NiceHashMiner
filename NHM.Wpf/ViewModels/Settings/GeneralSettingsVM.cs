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
        public IEnumerable<string> LanguageOptions => Translations.GetAvailableLanguagesNames();

        // More placeholders
        public string SelectedCurrency { get; set; } = "CAD";

        public int SelectedLangIndex
        {
            get => Translations.GetCurrentIndex();
            set
            {
                var code = Translations.GetLanguageCodeFromIndex(value);
                Translations.SetLanguage(code);
            }
        }
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
            SelectedLangIndex = Translations.GetCurrentIndex();

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
        }
    }
}

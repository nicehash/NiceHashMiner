using NHM.Common;
using System.Collections.Generic;

namespace NHMCore.Configs
{
    public class TranslationsSettings : NotifyChangedBase
    {
        public static TranslationsSettings Instance { get; } = new TranslationsSettings();

        private TranslationsSettings()
        {

        }

        public IEnumerable<string> LanguageOptions => Translations.GetAvailableLanguagesNames();

        private string _language = ""; // no language by default
        public string Language
        {
            get => _language;
            set
            {
                _language = value;
                OnPropertyChanged(nameof(Language));
            }
        }

        public int LanguageIndex
        {
            get => Translations.GetLanguageIndexFromCode(Language);
            set
            {
                var newLang = Translations.GetLanguageCodeFromIndex(value);
                if (Language != newLang)
                {
                    Language = newLang;

                    Translations.SelectedLanguage = Language;
                }
                OnPropertyChanged(nameof(LanguageIndex));
            }
        }
    }
}

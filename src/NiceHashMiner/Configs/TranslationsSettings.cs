using NiceHashMiner.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Configs
{
    public class TranslationsSettings
    {
        public static TranslationsSettings Instance { get; } = new TranslationsSettings();

        public string Language { get; set; } = ""; // no language by default
        public int LanguageIndex
        {
            get => Translations.GetLanguageIndexFromCode(Language);
            set
            {
                var newLang = Translations.GetLanguageCodeFromIndex(value);
                if (Language != newLang)
                {
                    Language = newLang;
                    Translations.SetLanguage(Language);
                    FormHelpers.TranslateAllOpenForms();
                }
            }
        }


    }
}

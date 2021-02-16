using Newtonsoft.Json;
using NHM.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NHMCore
{
    public static class Translations
    {
        [Serializable]
        private class TranslationFile
        {
            public Dictionary<string, string> Languages { get; set; }
            public Dictionary<string, Dictionary<string, string>> Translations { get; set; }
        }

        [Serializable]
        public class Language
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public static event EventHandler LanguageChanged;

        private static Dictionary<string, Dictionary<string, string>> _entries;

        // transform so it is possible to switch from any language
        private static readonly Dictionary<string, Dictionary<string, string>> TransformedEntries;
        private static List<Language> _availableLanguages;


        private static string _selectedLanguage = "en";
        public static string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (value == _selectedLanguage) return;

                if (_availableLanguages.All(l => l.Code != value)) return;

                _selectedLanguage = value;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        static Translations()
        {
            TransformedEntries = new Dictionary<string, Dictionary<string, string>>();
            _availableLanguages = new List<Language>();
            _entries = new Dictionary<string, Dictionary<string, string>>();

            // always have english
            var enMetaData = new Language
            {
                Code = "en",
                Name = "English",
            };

            // try init
            TryInitTranslations();

            var flag = false;
            foreach (var lang in _availableLanguages)
            {
                if (lang.Code == "en")
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                _availableLanguages.Add(enMetaData);
            }
        }

        private static void TryInitTranslations()
        {
            // file in binary root path
            string transFilePath = Paths.AppRootPath("translations.json");
            try
            {
                var translations = JsonConvert.DeserializeObject<TranslationFile>(File.ReadAllText(transFilePath, Encoding.UTF8));
                if (translations == null) return;

                if (translations.Languages != null)
                {
                    _availableLanguages = translations.Languages.Select(pair => new Language { Code = pair.Key, Name = pair.Value }).ToList();
                    foreach (var kvp in _availableLanguages)
                    {
                        Logger.Info("Translations", $"Found language: code: {kvp.Code}, name: {kvp.Name}");
                    }
                }
                if (translations.Translations != null)
                {
                    _entries = translations.Translations;
                    // init transformed entries so we can switch back and forth
                    foreach (var lang in _availableLanguages)
                    {
                        if (lang.Code == "en") continue;
                        foreach (var enTrans in _entries)
                        {
                            var enKey = enTrans.Key;
                            var enToOther = enTrans.Value;
                            if (enToOther.ContainsKey(lang.Code) == false) continue;
                            var trKey = enToOther[lang.Code];
                            var trToOther = new Dictionary<string, string>();
                            trToOther["en"] = enKey;
                            foreach (var kvp in enToOther)
                            {
                                var langCode = kvp.Key;
                                if (lang.Code == langCode) continue;

                                var translatedText = kvp.Value;
                                trToOther[langCode] = translatedText;
                            }
                            TransformedEntries[trKey] = trToOther;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("NICEHASH", $"Lang error: {e.Message}");
            }
        }

        public static List<string> GetAvailableLanguagesNames()
        {
            var langNames = new List<string>();
            foreach (var kvp in _availableLanguages)
            {
                langNames.Add(kvp.Name);
            }
            return langNames;
        }

        public static string GetLanguageCodeFromName(string name)
        {
            foreach (var kvp in _availableLanguages)
            {
                if (kvp.Name == name) return kvp.Code;
            }
            return "";
        }

        public static string GetLanguageCodeFromIndex(int index)
        {
            if (index < _availableLanguages.Count)
            {
                return _availableLanguages[index].Code;
            }
            return "";
        }

        public static int GetLanguageIndexFromCode(string code)
        {
            for (var i = 0; i < _availableLanguages.Count; i++)
            {
                var kvp = _availableLanguages[i];
                if (kvp.Code == code) return i;
            }
            return 0;
        }

        public static int GetCurrentIndex()
        {
            return GetLanguageIndexFromCode(_selectedLanguage);
        }

        public static string Tr(object obj)
        {
            return obj == null ? "" : Tr(obj.ToString());
        }

        // Tr Short for translate
        public static string Tr(string text)
        {
            if (string.IsNullOrEmpty(_selectedLanguage)) return text;

            // if other language search for it
            if (_entries.ContainsKey(text) && _entries[text].ContainsKey(_selectedLanguage))
            {
                return _entries[text][_selectedLanguage];
            }
            var containsTransformed = TransformedEntries.ContainsKey(text);
            // check transformed entry
            if (containsTransformed && TransformedEntries[text].ContainsKey(_selectedLanguage))
            {
                return TransformedEntries[text][_selectedLanguage];
            }
            // check transformed entry en
            if (containsTransformed && TransformedEntries[text].ContainsKey("en"))
            {
                return TransformedEntries[text]["en"];
            }
            // didn't find text with language key so just return the text 
            return text;
        }

        public static string Tr(string text, params object[] args)
        {
            return string.Format(Tr(text), args);
        }
    }
}

using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner
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

        static Translations()
        {
            // always have english
            var enMetaData = new Language
            {
                Code = "en",
                Name = "English",
            };
            _availableLanguages = new List<Language>();

            // try init
            TryInitTranslations();
            bool flag = false;
            foreach(var lang in _availableLanguages)
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

        private static string _selectedLanguage = "en";
        private static Dictionary<string, Dictionary<string, string>> _entries = new Dictionary<string, Dictionary<string, string>>();
        public static List<Language> _availableLanguages = new List<Language>();

        private static void TryInitTranslations()
        {
            // file in binary root path
            var transFilePath = "translations.json";
            try
            {
                TranslationFile translations = JsonConvert.DeserializeObject<TranslationFile>(File.ReadAllText(transFilePath, Encoding.UTF8));
                if (translations == null) return;

                if (translations.Languages != null)
                {
                    _availableLanguages = translations.Languages.Select(pair => new Language { Code = pair.Key, Name = pair.Value }).ToList();
                    foreach (var kvp in _availableLanguages)
                    {
                        Helpers.ConsolePrint("NICEHASH", $"Found language: code: {kvp.Code}, name: {kvp.Name}");
                    }
                } 
                if (translations.Translations != null)
                {
                    _entries = translations.Translations;
                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("NICEHASH", "Lang error: " + e.Message);
            }
        }

        public static void SetLanguage(string langCode)
        {
            foreach(var lang in _availableLanguages)
            {
                if (lang.Code == langCode)
                {
                    _selectedLanguage = lang.Code;
                    break;
                }
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

        // Tr Short for translate
        public static string Tr(string text)
        {
            // if other language search for it
            if (!string.IsNullOrEmpty(_selectedLanguage) && _entries.ContainsKey(text) && _entries[text].ContainsKey(_selectedLanguage))
            {
                return _entries[text][_selectedLanguage];
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

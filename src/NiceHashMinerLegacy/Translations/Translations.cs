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
        public class TranslationFile
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

        [Serializable]
        private class Translation
        {
            public string text { get; set; }
            public Dictionary<string, string> langTranslation { get; set; }
        }

        static Translations()
        {
            // always have english
            var enMetaData = new Language
            {
                Code = "en",
                Name = "English",
            };
            AvailableLanguages = new List<Language>();

            // try init
            TryInitFromFiles();
            bool flag = false;
            foreach(var lang in AvailableLanguages)
            {
                if (lang.Code == "en")
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                AvailableLanguages.Add(enMetaData);
            }
        }

        private static string selectedLanguage;
        private static Dictionary<string, Dictionary<string, string>> _entries = new Dictionary<string, Dictionary<string, string>>();
        public static List<Language> AvailableLanguages = new List<Language>();
        private static TranslationFile _translations;

        public static void TryInitFromFiles()
        {
                var transFilePath = "C:\\Users\\Domen\\Desktop\\nhmlDevelop\\NiceHashMinerLegacy\\src\\NiceHashMinerLegacy\\langs\\translations.json";
                    try
                    {
                        _translations = JsonConvert.DeserializeObject<TranslationFile>(File.ReadAllText(transFilePath, Encoding.UTF8));
                    // TODO null checks
                    AvailableLanguages = _translations.Languages.Select(pair => new Language { Code = pair.Key, Name = pair.Value} ).ToList();
                    _entries = _translations.Translations;
                } catch (Exception e)
                    {
                        Helpers.ConsolePrint("NICEHASH", "Lang error: " + e.Message);
                    }
        }

        public static void SetLanguage(string lName)
        {
            foreach(var lang in AvailableLanguages)
            {
                if (lang.Name == lName)
                {
                    selectedLanguage = lang.Code;
                }
            }
        }

        public static Dictionary<string, string> GetAvailableLanguages()
        {
            var retdict = new Dictionary<string, string>();

            foreach (var kvp in AvailableLanguages)
            {
                Helpers.ConsolePrint("NICEHASH", "Found language: " + kvp.Name);
                retdict.Add(kvp.Code, kvp.Name);
            }

            return retdict;
        }

        // Tr Short for translate
        public static string Tr(string text)
        {
            // if other language search for it
            if (!string.IsNullOrEmpty(selectedLanguage) && _entries.ContainsKey(text) && _entries[text].ContainsKey(selectedLanguage))
            {
                return _entries[text][selectedLanguage];
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

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

        private static Dictionary<string, Dictionary<string, string>> _entries = new Dictionary<string, Dictionary<string, string>>();

        // transform so it is possible to switch from any language
        private static readonly Dictionary<string, Dictionary<string, string>> _transformedEntries = new Dictionary<string, Dictionary<string, string>>();
        private static List<Language> _availableLanguages = new List<Language>();


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
            // try init
            TryInitTranslations();

            // always have english
            var hasEng = _availableLanguages.Any(lang => lang.Code == "en");
            if (!hasEng) _availableLanguages.Add(new Language { Code = "en", Name = "English", });
        }

        private static void TryInitTranslations()
        {
            // file in binary root path
            string transFilePath = Paths.AppRootPath("translations.json");
            try
            {
                var translations = JsonConvert.DeserializeObject<TranslationFile>(File.ReadAllText(transFilePath, Encoding.UTF8));
                if (translations == null) return;

                void initLanguages()
                {
                    if (translations.Languages == null) return;
                    _availableLanguages = translations.Languages.Select(pair => new Language { Code = pair.Key, Name = pair.Value }).ToList();
                    foreach (var kvp in _availableLanguages) Logger.Info("Translations", $"Found language: code: {kvp.Code}, name: {kvp.Name}");
                }
                initLanguages();

                void initTranslations()
                {
                    if (translations.Translations == null) return;
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
                            _transformedEntries[trKey] = trToOther;
                        }
                    }
                }
                initTranslations();
            }
            catch (Exception e)
            {
                Logger.Error("NICEHASH", $"Lang error: {e.Message}");
            }
        }

        public static List<string> GetAvailableLanguagesNames() => _availableLanguages?.Select(kvp => kvp.Name).ToList();

        public static string GetLanguageCodeFromName(string name)
        {
            var target = _availableLanguages.FirstOrDefault(kvp => kvp.Name == name);
            return target?.Code ?? "";
        }

        public static string GetLanguageCodeFromIndex(int index)
        {
            if (index < _availableLanguages.Count) return _availableLanguages[index].Code;
            return "";
        }

        public static int GetLanguageIndexFromCode(string code) => Math.Max(0, _availableLanguages.FindIndex(kvp => kvp.Code == code));

        public static int GetCurrentIndex() => GetLanguageIndexFromCode(_selectedLanguage);

        public static string Tr(object obj)
        {
            return obj == null ? "" : Tr(obj.ToString());
        }

        private static (bool ok, string trText) GetTranslated(string text, Dictionary<string, Dictionary<string, string>> entries, string lang)
        {
            var ok = !string.IsNullOrEmpty(lang) && entries.ContainsKey(text) && entries[text].ContainsKey(lang);
            return (ok, ok ? entries[text][lang] : "");
        }

        // Tr Short for translate
        public static string Tr(string text)
        {
            var (ok, trText) = GetTranslated(text, _entries, _selectedLanguage);
            if (ok) return trText;
            // check transformed entry
            (ok, trText) = GetTranslated(text, _transformedEntries, _selectedLanguage);
            if (ok) return trText;
            // check transformed entry en
            (ok, trText) = GetTranslated(text, _transformedEntries, "en");
            if (ok) return trText;
            // didn't find text with language key so just return the text 
            return text;
        }

        public static string Tr(string text, params object[] args) => string.Format(Tr(text), args);
    }
}

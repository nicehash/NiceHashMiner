using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner
{
    internal class International
    {
        private class Language
        {
#pragma warning disable 649
            public string Name;
            public LanguageType ID;
            public Dictionary<string, string> Entries;
#pragma warning restore 649
        }

        private static Language _selectedLanguage;
        private static Language _english;

        private static List<Language> GetLanguages()
        {
            var langs = new List<Language>();

            try
            {
                var di = new DirectoryInfo("langs");
                var files = di.GetFiles("*.lang");

                foreach (var fi in files)
                {
                    try
                    {
                        var l = JsonConvert.DeserializeObject<Language>(
                            File.ReadAllText(fi.FullName)); // TODO , Globals.JsonSettings not sure since data must be localized
                        langs.Add(l);
                    }
                    catch (Exception ex)
                    {
                        Helpers.ConsolePrint("NICEHASH", "Lang error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", "Lang error: " + ex.Message);
            }

            return langs;
        }

        public static void Initialize(LanguageType lid)
        {
            var langs = GetLanguages();

            foreach (var lang in langs)
            {
                if (lang.ID == lid)
                {
                    Helpers.ConsolePrint("NICEHASH", "Selected language: " + lang.Name);
                    _selectedLanguage = lang;
                }
                if (lang.ID == LanguageType.En)
                {
                    _english = lang;
                }
            }

            Helpers.ConsolePrint("NICEHASH", "Critical error: missing language");
        }

        /// <summary>
        /// Call this method to obtain available languages. Used by Settings GUI.
        /// </summary>
        /// <returns>Each dictionary entry contains id of the language (int) and name of the language (string).</returns>
        public static Dictionary<LanguageType, string> GetAvailableLanguages()
        {
            var langs = GetLanguages();
            var retdict = new Dictionary<LanguageType, string>();

            foreach (var lang in langs)
            {
                Helpers.ConsolePrint("NICEHASH", "Found language: " + lang.Name);
                retdict.Add(lang.ID, lang.Name);
            }

            return retdict;
        }

        public static string GetText(string token)
        {
            if (_selectedLanguage == null) return "";
            if (!_selectedLanguage.Entries.ContainsKey(token))
            {
                if (_english.Entries.ContainsKey(token))
                {
                    // Default to English if no token for selected lang
                    return _english.Entries[token];
                }

                return "";
            }

            return _selectedLanguage.Entries[token];
        }
    }
}

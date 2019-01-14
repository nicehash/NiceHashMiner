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
        private class TrMetaInfo
        {
            public string Name { get; set; }
            public LanguageType ID { get; set; } // for backward compatibility
            public string Code { get; set; }
        }

        static Translations()
        {
            // always have english
            var enMetaData = new TrMetaInfo
            {
                Name = "English",
                ID = LanguageType.En,
                Code = "en",
            };
            _metaData = new Dictionary<LanguageType, TrMetaInfo>();
            _metaData[LanguageType.En] = enMetaData;

            // try init
            TryInitFromFiles();
        }
        
        private static string selectedLanguage = "en";
        private static Dictionary<string, Dictionary<string, string>> _entries = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<LanguageType, TrMetaInfo> _metaData = new Dictionary<LanguageType, TrMetaInfo>();

        private static void TryInitFromFiles()
        {
            try
            {
                var di = new DirectoryInfo("langs");
                var files = di.GetFiles("*.json");

                foreach (var fi in files)
                {
                    if (fi.FullName.Contains("langs"))
                    {
                        try
                        {
                            _entries = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(
                                File.ReadAllText(fi.FullName)); // TODO , Globals.JsonSettings not sure since data must be localized
                        }
                        catch (Exception ex)
                        {
                            Helpers.ConsolePrint("NICEHASH", "Lang error: " + ex.Message);
                        }
                    }
                    if (fi.FullName.Contains("meta"))
                    {
                        try
                        {
                            var tmp = JsonConvert.DeserializeObject<List<TrMetaInfo>>(
                                File.ReadAllText(fi.FullName)); // TODO , Globals.JsonSettings not sure since data must be localized
                            foreach (var el in tmp)
                            {
                                _metaData[el.ID] = el;
                            }
                        }
                        catch (Exception ex)
                        {
                            Helpers.ConsolePrint("NICEHASH", "Meta data Lang error: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", "Lang error: " + ex.Message);
            }
        }

        public static void SetLanguage(LanguageType lid)
        {
            if (_metaData.ContainsKey(lid))
            {
                selectedLanguage = _metaData[lid].Code;
            }
        }

        public static Dictionary<LanguageType, string> GetAvailableLanguages()
        {
            var retdict = new Dictionary<LanguageType, string>();

            foreach (var kvp in _metaData)
            {
                var metaData = kvp.Value;
                Helpers.ConsolePrint("NICEHASH", "Found language: " + metaData.Name);
                retdict.Add(metaData.ID, metaData.Name);
            }

            return retdict;
        }

        // Tr Short for translate
        public static string Tr(string text)
        {
            // if other language search for it
            if (_entries.ContainsKey(text) && _entries[text].ContainsKey(selectedLanguage))
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

using Newtonsoft.Json;

namespace CrowdinTranslationsConverter
{
    public class TranslationFile
    {
        public Dictionary<string, string> Languages { get; set; }
        public Dictionary<string, Dictionary<string, string>> Translations { get; set; }
    }

    class Program
    {
        private void TranslationsDeserializer(string path)
        {
            var crowdin = new Dictionary<string, Dictionary<string, string>>();

            var counter = 1;
            var data = JsonConvert.DeserializeObject<TranslationFile>(File.ReadAllText(path + @"\translations.json"));

            foreach (var lang in data.Languages.Keys)
            {
                crowdin.Add(lang, new Dictionary<string, string>());
            }

            foreach (var (key, value) in data.Translations)
            {
                if (!value.ContainsKey("en")) value.Add("en", key);
            }

            foreach (var (_, translations) in data.Translations)
            {
                foreach (var langKey in crowdin.Keys)
                {
                    var translation = translations.ContainsKey(langKey) ? translations[langKey] : "";
                    crowdin[langKey].Add("translation_" + counter, translation);
                }
                counter++;
            }

            foreach (var langKey in crowdin.Keys)
            {
                File.WriteAllText(path + @"\nhm_" + langKey + ".json", JsonConvert.SerializeObject(crowdin[langKey], Formatting.Indented));
            }
        }

        private void TranslationsSerializer(string path)
        {
            var counter = 1;
            var crowdin = new Dictionary<string, Dictionary<string, string>>();
            var langs = new Dictionary<string, string> { { "en", "English" }, { "ru", "Русский" }, { "bg", "Български" }, { "es", "Español" }, { "it", "Italiano" }, { "pl", "Polski" }, { "pt", "Português" }, { "ro", "Română" }, { "zh_cn", "简体中文" } };

            TranslationFile translationFile = new()
            {
                Languages = langs,
                Translations = new Dictionary<string, Dictionary<string, string>>()
            };

            foreach (var lang in langs)
            {
                crowdin.Add(lang.Key, new Dictionary<string, string>());
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path + @"\nhm_" + lang.Key + ".json"));
                foreach (var dataItem in data)
                {
                    crowdin[lang.Key].Add(dataItem.Key, dataItem.Value);
                }
            }

            foreach (var item in crowdin.Values.First())
            {
                var translationKey = "translation_" + counter;
                var englishKey = crowdin["en"][translationKey];
                translationFile.Translations.Add(englishKey, new Dictionary<string, string>());

                foreach (var key in langs.Keys)
                {
                    if (key == "en") continue;
                    if (crowdin[key][translationKey] == "") continue;
                    translationFile.Translations[englishKey].Add(key, crowdin[key][translationKey]);
                }
                counter++;
            }

            File.WriteAllText(path + @"\translations.json", JsonConvert.SerializeObject(translationFile, Formatting.Indented));
        }
        static async Task Main(string[] args)
        {

        }
    }
}

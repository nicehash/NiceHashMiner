# How to help translate client

If you would like a client to be translated to your language or would like to add/modify existing translations there is an easy way to do it.

In the <a href="./../src/NiceHashMiner">NiceHashMiner</a> directory exists `translations.json` file, containing all available translations.

So if you would like to just modify existing translation find the one in the file and modify it.<br>
To add a new translation for existing language, find an existing english translation and add a new one in the following format: `language_code: "Translated text"`.

Here is an example:
```
"Start": {
      "bg": "Тест",
      "es": "Empezar",
      "pt": "Iniciar",
      "zh_cn": "开始"
    },
```
so if I would like to add a russian translation I would just add it like this:
```
"Start": {
      "bg": "Тест",
      "es": "Empezar",
      "pt": "Iniciar",
      "ru": "Старт"
      "zh_cn": "开始",
    },
```

If you would like to see your language as supported translation in the client you would have to:

1) Add your language as option in the `Languages` section at top of the file
```
"Languages": {
    "en": "English",
    "ru": "Русский (Unofficial)",
    "es": "Español (Unofficial)",
    "pt": "Português (Unofficial)",
    "bg": "Български (Unofficial)",
    "it": "Italiano (Unofficial)",
    "pl": "Polski (Unofficial)",
    "zh_cn": "简体中文 (Unofficial)",
    "ro": "Română (Unofficial)"
  },
```
2) Translate at least the most the most seen translations (Main, Settings, Benchmark windows).

<br>

**In all cases you have to create a pull request so we can check the changes and accept them or inform you of what should be changed.**

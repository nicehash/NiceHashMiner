using NHMCore.Configs;

namespace NHM.Wpf.ViewModels
{
    class ChooseLanguageVM : BaseVM
    {
        public TranslationsSettings TranslationsSettings => TranslationsSettings.Instance;
    }
}

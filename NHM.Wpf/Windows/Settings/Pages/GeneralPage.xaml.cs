using System;
using System.Windows.Controls;
using NHM.Wpf.Windows.Common;

namespace NHM.Wpf.Windows.Settings.Pages
{
    /// <summary>
    /// Interaction logic for GeneralPage.xaml
    /// </summary>
    public partial class GeneralPage : UserControl, ISettingsPage
    {
        public GeneralPage()
        {
            InitializeComponent();

            Translations.LanguageChanged += Translations_LanguageChanged;
            Translations_LanguageChanged(null, null);
        }

        private void Translations_LanguageChanged(object sender, EventArgs e)
        {
            WindowUtils.Translate(this);
        }
    }
}

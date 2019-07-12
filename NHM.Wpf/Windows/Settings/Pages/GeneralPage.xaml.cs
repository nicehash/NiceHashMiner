using System;
using System.Windows;
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
            Loaded += GeneralPage_Loaded;
            Unloaded += GeneralPage_Unloaded;
        }

        private void GeneralPage_Loaded(object sender, RoutedEventArgs e)
        {
            Translations.LanguageChanged += Translations_LanguageChanged;
            Translations_LanguageChanged(null, null);
        }

        private void GeneralPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Translations.LanguageChanged -= Translations_LanguageChanged;
        }

        private void Translations_LanguageChanged(object sender, EventArgs e)
        {
            WindowUtils.Translate(this);
        }
    }
}

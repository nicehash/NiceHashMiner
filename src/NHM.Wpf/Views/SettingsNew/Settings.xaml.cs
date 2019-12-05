using NHMCore.Configs;
using System.Windows;
using System.Windows.Controls;

namespace NHM.Wpf.Views.SettingsNew
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        private bool _isGeneral = true;

        public Settings()
        {
            InitializeComponent();
            OnScreenChange(_isGeneral);
        }

        protected void OnScreenChange(bool isGeneral)
        {
            if (isGeneral)
            {
                GeneralButton.IsChecked = true;
                AdvancedButton.IsChecked = false;
                GeneralTab.IsSelected = true;
            }
            else
            {
                GeneralButton.IsChecked = false;
                AdvancedButton.IsChecked = true;
                AdvancedTab.IsSelected = true;
            }
        }

        private void Btn_GeneralSettings_Click(object sender, RoutedEventArgs e)
        {
            _isGeneral = true;
            OnScreenChange(_isGeneral);
        }

        private void Btn_AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            _isGeneral = false;
            OnScreenChange(_isGeneral);
        }

        private void Btn_default_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SetDefaults();
        }
    }
}

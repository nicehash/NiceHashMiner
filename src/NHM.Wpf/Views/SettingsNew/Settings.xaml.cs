using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using System.Threading.Tasks;
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

            ConfigManager.ShowRestartRequired += ShowRestartRequired;
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

        private void ShowRestartRequired(object sender, bool e)
        {
            btn_restart.Visibility = e ? Visibility.Visible : Visibility.Collapsed;
        }

        // RESTART doesn't work with debug console and mining running
        private async void Btn_restart_Click(object sender, RoutedEventArgs e)
        {
            await OnRestart();
        }

        private async Task OnRestart()
        {
            for (int tryIt = 0; tryIt < 5; tryIt++)
            {
                await ApplicationStateManager.BeforeExit();
                ApplicationStateManager.RestartProgram();
                await Task.Delay(1000);
            }
        }
    }
}

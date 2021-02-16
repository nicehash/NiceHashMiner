using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NiceHashMiner.Views.Common;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static NHMCore.Translations;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        private int _tabIndex = 0;

        public Settings()
        {
            InitializeComponent();
            OnScreenChange(_tabIndex);

            ConfigManager.ShowRestartRequired += ShowRestartRequired;
        }

        protected void OnScreenChange(int tabIndex)
        {
            switch (tabIndex)
            {
                case 0:
                    GeneralButton.IsChecked = true;
                    AdvancedButton.IsChecked = false;
                    AboutButton.IsChecked = false;
                    QrButton.IsChecked = false;
                    GeneralTab.IsSelected = true;
                    break;
                case 1:
                    GeneralButton.IsChecked = false;
                    AdvancedButton.IsChecked = true;
                    AboutButton.IsChecked = false;
                    QrButton.IsChecked = false;
                    AdvancedTab.IsSelected = true;
                    break;
                case 2:
                    GeneralButton.IsChecked = false;
                    AdvancedButton.IsChecked = false;
                    AboutButton.IsChecked = true;
                    QrButton.IsChecked = false;
                    AboutTab.IsSelected = true;
                    break;
                case 3:
                    GeneralButton.IsChecked = false;
                    AdvancedButton.IsChecked = false;
                    AboutButton.IsChecked = false;
                    QrButton.IsChecked = true;
                    QrTab.IsSelected = true;
                    break;
            }
        }

        private void Btn_Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btnSender = sender as ToggleButton;
                switch (btnSender.Name)
                {
                    case "GeneralButton":
                        OnScreenChange(0);
                        break;
                    case "AdvancedButton":
                        OnScreenChange(1);
                        break;
                    case "AboutButton":
                        OnScreenChange(2);
                        break;
                    case "QrButton":
                        OnScreenChange(3);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Settings", $"Error occured: {ex.Message}");
            }
        }

        private void Btn_default_Click(object sender, RoutedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            {
                Title = Tr("Set default settings?"),
                Description = Tr("Are you sure you would like to set everything back to defaults? This will restart NiceHash Miner automatically."),
                OkText = Tr("Yes"),
                CancelText = Tr("No"),
                AnimationVisible = Visibility.Collapsed
            };
            nhmConfirmDialog.OKClick += (s, e1) =>
            {
                Translations.SelectedLanguage = "en";
                ConfigManager.SetDefaults();
                Task.Run(() => ApplicationStateManager.RestartProgram());
            };
            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }

        private void ShowRestartRequired(object sender, bool e)
        {
            btn_restart.Visibility = e ? Visibility.Visible : Visibility.Collapsed;
        }

        // RESTART doesn't work with debug console and mining running
        private async void Btn_restart_Click(object sender, RoutedEventArgs e)
        {
            await ApplicationStateManager.RestartProgram();
        }

        private void QrButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var btnSender = sender as ToggleButton;
            btnSender.Background = Application.Current.FindResource("QrDarkLogo") as Brush;
        }

        private void QrButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var btnSender = sender as ToggleButton;
            btnSender.Background = Application.Current.FindResource("QrLightLogo") as Brush;
        }
    }
}

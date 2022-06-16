using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NiceHashMiner.Views.Common;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static NHMCore.Translations;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {

        private readonly (TabItem tab, ToggleButton button)[] _tabButtonPairs;

        public Settings()
        {
            InitializeComponent();

            ConfigManager.ShowRestartRequired += ShowRestartRequired;
            _tabButtonPairs = new (TabItem tab, ToggleButton button)[]
            {
                (GeneralTab, GeneralButton),
                (AdvancedTab, AdvancedButton),
                (AboutTab, AboutButton),
                (QrTab, QrButton),
            };
            OnScreenChange(0);
        }

        private void OnScreenChange(int tabIndex)
        {
            for (int i = 0; i < _tabButtonPairs.Length; i++)
            {
                var (tab, button) = _tabButtonPairs[i];
                var selectedAndChecked = i == tabIndex;
                button.IsChecked = selectedAndChecked;
                if (selectedAndChecked) tab.IsSelected = true;
            }
            if (tabIndex >= _tabButtonPairs.Length) throw new Exception($"OnScreenChange unknown tab index {tabIndex}");
        }

        private static readonly string[] _buttonNameToTabIndex = { "GeneralButton", "AdvancedButton", "AboutButton", "QrButton" };
        private void Btn_Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ToggleButton tb) OnScreenChange(Array.IndexOf(_buttonNameToTabIndex, tb.Name));
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

        private void QrButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ToggleButton tb) tb.Background = Application.Current.FindResource("QrDarkLogo") as Brush;
        }

        private void QrButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ToggleButton tb) tb.Background = Application.Current.FindResource("QrLightLogo") as Brush;
        }
    }
}

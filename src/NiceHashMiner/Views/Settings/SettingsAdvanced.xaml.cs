using NHMCore.Configs;
using NHMCore.Mining;
using System.Windows.Controls;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsAdvanced.xaml
    /// </summary>
    public partial class SettingsAdvanced : UserControl
    {
        public SettingsAdvanced()
        {
            InitializeComponent();
            SwitchSettings.Instance.PropertyChanged += Instance_PropertyChanged;
            MiningSettings.Instance.PropertyChanged += Instance_PropertyChanged;
            LoggingDebugConsoleSettings.Instance.PropertyChanged += Instance_PropertyChanged;
            IdleMiningSettings.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AvailableDevices.HasNvidia)
            {
                wp_cuda.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                wp_cuda.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }
    }
}

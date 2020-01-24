using NHMCore.Configs;
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
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }
    }
}

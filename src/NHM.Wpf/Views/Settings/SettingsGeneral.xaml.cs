using NHMCore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NHM.Wpf.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsGeneral.xaml
    /// </summary>
    public partial class SettingsGeneral : UserControl
    {
        public SettingsGeneral()
        {
            InitializeComponent();
            LanguageSettings.Visibility = AppRuntimeSettings.ShowLanguage ? Visibility.Visible : Visibility.Collapsed;
            ThemeSettings.Visibility = AppRuntimeSettings.ThemeSettingsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddressHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private async void TextBoxBitcoinAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            if (ApplicationStateManager.SetResult.INVALID == result)
            {
                textBoxBTCAddress.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("Red");
                //errorProvider1.SetError(textBoxBTCAddress, Tr("Invalid Bitcoin address! {0} will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!", NHMProductInfo.Name));
            }
            else
            {
                textBoxBTCAddress.BorderBrush = SystemColors.ControlDarkBrush;
                //errorProvider1.SetError(textBoxBTCAddress, "");
            }
        }

        // TODO validator can be outside from setting
        private void TextBoxWorkerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var trimmedWorkerNameText = textBoxWorkerName.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkerNameText);
            if (ApplicationStateManager.SetResult.INVALID == result)
            {
                textBoxWorkerName.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("Red");
                //errorProvider1.SetError(textBoxWorkerName, Tr("Invalid workername!\n\nPlease enter a valid workername (Aa-Zz, 0-9, up to 15 character long)."));
            }
            else
            {
                textBoxWorkerName.BorderBrush = SystemColors.ControlDarkBrush;
                //errorProvider1.SetError(textBoxWorkerName, "");
            }
        }
    }
}

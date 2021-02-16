using NHMCore;
using NHMCore.Utils;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace NiceHashMiner.Views.Common
{
    /// <summary>
    /// Interaction logic for EnterWalletDialog.xaml
    /// </summary>
    public partial class EnterWalletDialog : UserControl
    {
        public EnterWalletDialog()
        {
            InitializeComponent();
            WindowUtils.Translate(this);
        }

        private void BtcTextValidation()
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var btcOK = CredentialValidators.ValidateBitcoinAddress(trimmedBtcText);
            SaveButton.IsEnabled = btcOK;
            if (btcOK)
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            else
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxBad") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }

        private void TextBoxBitcoinAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtcTextValidation();
        }

        private void TextBoxBitcoinAddress_KeyUp(object sender, KeyEventArgs e)
        {
            BtcTextValidation();
        }

        private void TextBoxBitcoinAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            BtcTextValidation();
        }

        private void AddressHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            CustomDialogManager.HideCurrentModal();
        }

        private async void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            if (ApplicationStateManager.SetResult.INVALID == result)
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxBad") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
            else
            {
                CustomDialogManager.HideCurrentModal();
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
        }
    }
}

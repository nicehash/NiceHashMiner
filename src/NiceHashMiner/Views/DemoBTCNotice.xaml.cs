using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for DemoBTCNotice.xaml
    /// </summary>
    public partial class DemoBTCNotice : BaseDialogWindow
    {
        public DemoBTCNotice()
        {
            InitializeComponent();
            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            BtcTextValidation();
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

        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            if (CredentialValidators.ValidateBitcoinAddress(trimmedBtcText))
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
                CredentialsSettings.Instance.SetBitcoinAddress(trimmedBtcText);
                Close();
            }
            else
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxBad") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }

        private void DemoMiningButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

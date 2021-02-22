using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for DemoBTCNotice.xaml
    /// </summary>
    public partial class DemoBTCNotice : BaseDialogWindow
    {
        private Window _showMeHowWindow = null;
        public DemoBTCNotice()
        {
            InitializeComponent();
            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            BtcTextValidation();
            Closing += DemoBTCNotice_Closing;
        }

        private void DemoBTCNotice_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _showMeHowWindow.Close();
            }
            catch
            { }
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

        private void BaseDialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var gifPath = Paths.AppRootPath("assets", "enter_BTC_manually.gif");
            if (File.Exists(gifPath))
            {
                _showMeHowWindow = new ManuallyEnterBTCTutorial();
                _showMeHowWindow.Left = this.Left + this.Width;
                _showMeHowWindow.Top = this.Top;
                _showMeHowWindow.Show();
            }
        }
    }
}

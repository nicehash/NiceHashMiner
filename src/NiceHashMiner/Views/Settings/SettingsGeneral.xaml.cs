using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NiceHashMiner.Views.Settings
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
            if (CredentialsSettings.Instance.IsBitcoinAddressValid)
            {
                textBoxBTCAddress.Text = CredentialsSettings.Instance.BitcoinAddress;
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            if (CredentialsSettings.Instance.IsWorkerNameValid)
            {
                textBoxWorkerName.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
        }

        private void AddressHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void ValidateBTCAddr()
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var btcOK = CredentialValidators.ValidateBitcoinAddress(trimmedBtcText);
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

        private async void TextBoxBitcoinAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            ValidateBTCAddr();
        }

        private void TextBoxBitcoinAddress_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ValidateBTCAddr();
        }

        private void TextBoxBitcoinAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CredentialsSettings.Instance.IsBitcoinAddressValid)
            {
                textBoxBTCAddress.Text = CredentialsSettings.Instance.BitcoinAddress;
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            else
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
        }

        private void ValidateWorkername()
        {
            var trimmedWorkername = textBoxWorkerName.Text.Trim();
            var btcOK = CredentialValidators.ValidateWorkerName(trimmedWorkername);
            if (btcOK)
            {
                textBoxWorkerName.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            else
            {
                textBoxWorkerName.Style = Application.Current.FindResource("InputBoxBad") as Style;
                textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }

        // TODO validator can be outside from setting
        private void TextBoxWorkerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var trimmedWorkerNameText = textBoxWorkerName.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkerNameText);
            ValidateWorkername();
        }

        private void TextBoxWorkerName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ValidateWorkername();
        }

        private void TextBoxWorkerName_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (CredentialsSettings.Instance.IsWorkerNameValid)
            //{
            //    //// TODO we break binding if we assing this
            //    //textBoxWorkerName.Text = CredentialsSettings.Instance.WorkerName;
            //    textBoxWorkerName.Style = Application.Current.FindResource("InputBoxGood") as Style;
            //    textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            //}
            //else
            //{
            //    textBoxWorkerName.Style = Application.Current.FindResource("InputBoxGood") as Style;
            //    textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            //}
            ValidateWorkername();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void CreateLogReportButton_Click(object sender, RoutedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            { 
                Title = Translations.Tr("Pack log files?"),
                Description = Translations.Tr("This will restart your program and create a zip file on Desktop."),
                OkText = Translations.Tr("Ok"),
                CancelText = Translations.Tr("Cancel")
            };
            nhmConfirmDialog.OKClick += (s, e1) => 
            {
                File.Create(Paths.RootPath("do.createLog"));
                Task.Run(() => ApplicationStateManager.RestartProgram());
            };
            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);

        }
    }
}

using NHMCore;
using NHMCore.Configs;
using NHMCore.Notifications;
using NHMCore.Utils;
using NiceHashMiner.ViewModels;
using NiceHashMiner.Views.Common;
using System.Diagnostics;
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
            CredentialsSettings.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CredentialsSettings.Instance.BitcoinAddress))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        textBoxBTCAddress.Text = CredentialsSettings.Instance.BitcoinAddress;
                    });
                }
            };

            var (btcStyle, btcBrush) = GetStyleBrush(CredentialsSettings.Instance.IsBitcoinAddressValid);
            if (CredentialsSettings.Instance.IsBitcoinAddressValid) textBoxBTCAddress.Text = CredentialsSettings.Instance.BitcoinAddress;
            textBoxBTCAddress.Style = btcStyle;
            textBoxBTCAddress.BorderBrush = btcBrush;
            var (workerStyle, workerBrush) = GetStyleBrush(CredentialsSettings.Instance.IsWorkerNameValid);
            textBoxWorkerName.Style = workerStyle;
            textBoxWorkerName.BorderBrush = workerBrush;
        }

        private (Style style, Brush brush) GetStyleBrush(bool isGood)
        {
            var (styleName, brushName) = isGood ? ("InputBoxGood", "NastyGreenBrush") : ("InputBoxBad", "RedDangerColorBrush");
            return (
                Application.Current.FindResource(styleName) as Style,
                (Brush)Application.Current.FindResource(brushName)
                );
        }

        private string _reportId = null;

        private void AddressHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Helpers.VisitUrlLink(e.Uri.AbsoluteUri);
        }

        private void ValidateBTCAddr()
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var btcOK = CredentialValidators.ValidateBitcoinAddress(trimmedBtcText);
            if (btcOK) ValidateInternalBTCAddress();
            var (style, brush) = GetStyleBrush(btcOK);
            textBoxBTCAddress.Style = style;
            textBoxBTCAddress.BorderBrush = brush;
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
            var (style, brush) = GetStyleBrush(CredentialsSettings.Instance.IsBitcoinAddressValid);
            if (CredentialsSettings.Instance.IsBitcoinAddressValid)
            {
                textBoxBTCAddress.Text = CredentialsSettings.Instance.BitcoinAddress;
            }
            textBoxBTCAddress.Style = style;
            textBoxBTCAddress.BorderBrush = brush;
        }
        private void ValidateInternalBTCAddress()
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            if (!CredentialValidators.ValidateInternalBitcoinAddress(trimmedBtcText) && invalidBTCAddressWarningIcon != null && externalAddressHelp != null)
            {
                invalidBTCAddressWarningIcon.Visibility = Visibility.Visible;
                externalAddressHelp.Visibility = Visibility.Visible;
                return;
            }
            if (invalidBTCAddressWarningIcon != null) invalidBTCAddressWarningIcon.Visibility = Visibility.Collapsed;
            if (externalAddressHelp != null) externalAddressHelp.Visibility = Visibility.Collapsed;
        }

        private void ValidateWorkername()
        {
            var trimmedWorkername = textBoxWorkerName.Text.Trim();
            var workernameOK = CredentialValidators.ValidateWorkerName(trimmedWorkername);
            var (style, brush) = GetStyleBrush(workernameOK);
            textBoxWorkerName.Style = style;
            textBoxWorkerName.BorderBrush = brush;
        }

        // TODO validator can be outside from setting
        private void TextBoxWorkerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var trimmedWorkerNameText = textBoxWorkerName.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkerNameText);
            ValidateWorkername();
            WorkernamePanel._enterWorkernameDialog.OnWorkernameChangeHack?.Invoke(this, trimmedWorkerNameText);// hack on a hack
        }

        private void TextBoxWorkerName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ValidateWorkername();
        }

        private void TextBoxWorkerName_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateWorkername();
        }

        private void TextBoxElectricityCost_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxElectricityCost.Text == "") textBoxElectricityCost.Text = "0";
        }

        private void TextBoxMinimumProfit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (MinProfitTB.Text == "") MinProfitTB.Text = "0";
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            {
                Title = Translations.Tr("Language change"),
                Description = Translations.Tr("Your program will be restarted in order to completely change the language of NiceHash Miner."),
                OkText = Translations.Tr("Ok"),
                CancelVisible = Visibility.Collapsed,
                AnimationVisible = Visibility.Collapsed
            };
            nhmConfirmDialog.OKClick += (s, e1) => CommitGeneralAndRestart();
            nhmConfirmDialog.OnExit += (s, e1) => CommitGeneralAndRestart();

            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }

        private void CommitGeneralAndRestart()
        {
            GUISettings.Instance.DisplayTheme = GUISettings.Instance.NextDisplayTheme;
            ConfigManager.GeneralConfigFileCommit();
            Task.Run(() => ApplicationStateManager.RestartProgram());
        }

        private void RevertTheme()
        {
            this.ThemeSelect.SelectionChanged -= new SelectionChangedEventHandler(this.ThemeComboBox_SelectionChanged);
            GUISettings.Instance.RevertTheme();
            ConfigManager.GeneralConfigFileCommit();
            this.ThemeSelect.SelectionChanged += new SelectionChangedEventHandler(this.ThemeComboBox_SelectionChanged);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void CreateLogReportButton_Click(object sender, RoutedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            {
                Title = Translations.Tr("Pack and upload log files?"),
                Description = Translations.Tr("This will upload log report to our server."),
                OkText = Translations.Tr("Ok"),
                CancelText = Translations.Tr("Cancel"),
                AnimationVisible = Visibility.Collapsed
            };
            nhmConfirmDialog.OKClick += (s, e1) => ProcessLogReport();
            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }

        private async void ProcessLogReport()
        {
            var (success, uuid, _) = await Helpers.CreateAndUploadLogReport();
            if (success)
            {
                _reportId = uuid;
                CopyId.Visibility = Visibility.Visible;
                LogReportIdText.Content = Translations.Tr("Last report ID: {0}", uuid);
            }
            else
            {
                CopyId.Visibility = Visibility.Collapsed;
            }
            CreateBugUUIDDialog(uuid, success);
            AvailableNotifications.CreateLogUploadResultInfo(success, uuid);
        }

        private void CreateBugUUIDDialog(string uuid, bool success)
        {
            var bugUUIDDialog = new CustomDialog();
            if (success)
            {
                bugUUIDDialog.Title = "Bug report ID";
                bugUUIDDialog.Description = Translations.Tr("Use following ID for bug reporting.\n{0}", uuid);
                bugUUIDDialog.OkText = Translations.Tr("Copy to clipboard");
                bugUUIDDialog.CancelVisible = Visibility.Collapsed;
                bugUUIDDialog.CloseOnOk = false;
                bugUUIDDialog.AnimationVisible = Visibility.Collapsed;
                bugUUIDDialog.OKClick += (s, e) => Clipboard.SetText(uuid);
            }
            else
            {
                bugUUIDDialog.Title = "Bug report failed";
                bugUUIDDialog.Description = Translations.Tr("Bug report has failed. Please contact our support agent for help.");
                bugUUIDDialog.OkText = Translations.Tr("OK");
                bugUUIDDialog.CancelVisible = Visibility.Collapsed;
                bugUUIDDialog.AnimationVisible = Visibility.Collapsed;
            }

            CustomDialogManager.ShowModalDialog(bugUUIDDialog);
        }

        private void CopyLogReportIdButton_Click(object sender, RoutedEventArgs e)
        {
            if (_reportId == null) return;
            Clipboard.SetText(_reportId);
        }

        private void SelectTheme()
        {
            GUISettings.Instance.NextDisplayTheme = ThemeSelect.SelectedItem.ToString();//so we can commit on restart
            GUISettings.Instance.DisplayTheme = ThemeSelect.SelectedItem.ToString();//for current show of theme
            ConfigManager.GeneralConfigFileCommit();
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            {
                Title = Translations.Tr("Theme change"),
                Description = Translations.Tr("Your program will be restarted in order to completely change the theme of NiceHash Miner."),
                OkText = Translations.Tr("Ok"),
                CancelVisible = Visibility.Collapsed,
                AnimationVisible = Visibility.Collapsed
            };
            SelectTheme();
            nhmConfirmDialog.OKClick += (s, e1) => CommitGeneralAndRestart();
            nhmConfirmDialog.OnExit += (s, e1) => RevertTheme();//reverts even when click ok because it exits...

            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }
        private void NetProfitToggle_click(object sender, RoutedEventArgs e) //TODO for this to work change datacontext in xaml for this
        {
            if(DataContext is MainVM mvm && mvm.Devices != null)
            {
                foreach(var dev in mvm.Devices)
                {
                    dev?.OrderAlgorithmsByPaying();
                }
            }
        }
    }
}

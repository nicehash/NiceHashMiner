using Newtonsoft.Json;
using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        private string _uuid;
        private bool _gotQRCode = false;

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
            Unloaded += LoginWindow_Unloaded;
            HideIconAndTitle = true;
            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            if (GUISettings.Instance.DisplayTheme == "Dark") CheckBoxMode.IsChecked = true;
        }

        private void LoginWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            BtcHttpServer.Stop();
        }

        public bool? LoginSuccess { get; private set; } = null;

        private void SetTheme(bool isLight)
        {
            var displayTheme = isLight ? "Light" : "Dark";
            GUISettings.Instance.DisplayTheme = displayTheme;
            ThemeSetterManager.SetTheme(isLight);
            if (_gotQRCode)
            {
                var (image, ok) = QrCodeImageGenerator.GetQRCodeImage(_uuid, isLight);
                if (ok) rect_qrCode.Fill = image;
            }
        }

        private void CheckBoxMode_Checked_Dark(object sender, RoutedEventArgs e)
        {
            SetTheme(false);
        }

        private void CheckBoxMode_Unchecked_Light(object sender, RoutedEventArgs e)
        {
            SetTheme(true);
        }

        private void Register_OnClick(object sender, RoutedEventArgs e)
        {
            Helpers.VisitUrlLink(Links.Register);
        }

        private void ManuallyEnterBtc_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Login_OnClick(object sender, RoutedEventArgs e)
        {
            Helpers.VisitUrlLink($"{Links.Login}?nhmv4=1");
        }

        private async Task InitQRCode()
        {
            // this is vaild for 10 minutes
            _uuid = System.Guid.NewGuid().ToString();
            _gotQRCode = await BTC_FromQrCodeAPI.RequestNew_QR_Code(_uuid, ApplicationStateManager.RigID());
            if (_gotQRCode)
            {
                var isLight = GUISettings.Instance.DisplayTheme == "Light";
                // create qr code
                var (image, ok) = QrCodeImageGenerator.GetQRCodeImage(_uuid, isLight);
                if (ok)
                {
                    rect_qrCode.Fill = image;
                    ScanLabel.Content = "Scan with official NiceHash mobile application";
                    ScanConfirmButton.Content = "Confirm scan";
                }
                else
                {
                    ScanLabel.Content = "QR Code image generation failed";
                    ScanConfirmButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ScanConfirmButton.Visibility = Visibility.Collapsed;
                ScanLabel.Content = "Unable to retreive QR Code";
                //ScanConfirmButton.Content = "Retry QR code";
            }
        }

        private async void Confirm_Scan_Click(object sender, RoutedEventArgs e)
        {
            await Confirm_Scan_ClickTask();
        }

        private static async Task<string> GetBTCForUUID(string uuid)
        {
            try
            {
                Logger.Info("LoginWindow.GetBTCForUUID", "Waiting for btc address");
                return await BTC_FromQrCodeAPI.GetBTCForUUID(uuid);
            }
            catch (Exception ex)
            {
                Logger.Error("LoginWindow.GetBTCForUUID", $"GetBTCForUUID failed with {ex.Message}");
                return null;
            }
        }

        private async Task Confirm_Scan_ClickTask()
        {
            if (!_gotQRCode)
            {
                await InitQRCode();
                return;
            }
            var btc = await GetBTCForUUID(_uuid);
            if (CredentialValidators.ValidateBitcoinAddress(btc))
            {
                Logger.Info("LoginWindow.Confirm_Scan_ClickTask", $"Got valid btc address: {btc}");
                CredentialsSettings.Instance.SetBitcoinAddress(btc);
                Close();
            }
        }

        private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitQRCode();
            // background Task
            BtcHttpServer.RunBackgrounTask();
        }
    }
}

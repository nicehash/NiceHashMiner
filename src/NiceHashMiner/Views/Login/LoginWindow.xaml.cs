using Newtonsoft.Json;
using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        private LoginBrowser _loginBrowser;
        private string _uuid;
        private bool _gotQRCode = false;

        public LoginWindow()
        {
            InitializeComponent();
            Unloaded += LoginBrowser_Unloaded;
            Loaded += LoginWindow_Loaded;
            HideIconAndTitle = true;
            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
        }

        private void LoginBrowser_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_loginBrowser != null) _loginBrowser.AllowClose = true;
                _loginBrowser?.ForceCleanup();
                _loginBrowser?.Close();
            }
            catch
            { }
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
            Process.Start(Links.Register);
        }

        private void ManuallyEnterBtc_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Login_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
            if (_loginBrowser == null) _loginBrowser = new LoginBrowser();
            _loginBrowser.Top = this.Top;
            _loginBrowser.Left = this.Left;
            _loginBrowser.ShowDialog();
            LoginSuccess = _loginBrowser.LoginSuccess;
            this.Top = _loginBrowser.Top;
            this.Left = _loginBrowser.Left;
            if (!CredentialsSettings.Instance.IsBitcoinAddressValid)
            {
                ShowDialog();
            }
            else
            {
                Close();
            }
        }

        [Serializable]
        internal class BtcResponse
        {
            public string btc { get; set; }
        }

        private async Task InitQRCode()
        {
            // this is vaild for 10 minutes
            _uuid = Guid.NewGuid().ToString();
            _gotQRCode = await QrCodeGenerator.RequestNew_QR_Code(_uuid, ApplicationStateManager.RigID());
            if (_gotQRCode)
            {
                // create qr code
                var (image, ok) = QrCodeImageGenerator.GetQRCodeImage(_uuid);
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
                using (var client = new HttpClient())
                {
                    Logger.Info("LoginWindow.GetBTCForUUID", "Waiting for btc address");
                    var resp = await client.GetAsync($"https://api2.nicehash.com/api/v2/organization/nhmqr/{uuid}");
                    if (!resp.IsSuccessStatusCode) return null;
                    var contentString = await resp.Content.ReadAsStringAsync();
                    var btcResp = JsonConvert.DeserializeObject<BtcResponse>(contentString);
                    var setBtc = btcResp?.btc;
                    Logger.Info("LoginWindow.GetBTCForUUID", $"GetBTCForUUID Got btc address: {setBtc} for response: '{contentString}'");
                    return setBtc;
                }
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
        }
    }
}

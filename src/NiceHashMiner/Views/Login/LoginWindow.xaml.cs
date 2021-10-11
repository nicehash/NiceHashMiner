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

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        private LoginBrowser _loginBrowser;
        private string _uuid = Guid.NewGuid().ToString();
        public LoginWindow()
        {
            InitializeComponent();
            Unloaded += LoginBrowser_Unloaded;
            HideIconAndTitle = true;
            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            _ = ProcessQRCode();
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

        private void CheckBoxMode_Checked(object sender, RoutedEventArgs e)
        {
            GUISettings.Instance.DisplayTheme = "Dark";
            ThemeSetterManager.SetTheme(false);
            rect_qrCode.Fill = QrCodeHelpers.GetQRCode(_uuid, false);
        }

        private void CheckBoxMode_Unchecked(object sender, RoutedEventArgs e)
        {
            GUISettings.Instance.DisplayTheme = "Light";
            ThemeSetterManager.SetTheme(true);
            rect_qrCode.Fill = QrCodeHelpers.GetQRCode(_uuid);
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
            internal string btc { get; set; }
        }

        [Serializable]
        internal class RigUUIDRequest
        {
            internal string qrId { get; set; } = "";
            internal string rigId { get; set; } = "";
        }

        private static async Task<bool> RequestNew_QR_Code(string uuid, string rigId)
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(new RigUUIDRequest { qrId = uuid, rigId = rigId });
                using (var content = new StringContent(requestBody, Encoding.UTF8, "application/json"))
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync("https://api2.nicehash.com/api/v2/organization/nhmqr", content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Login.RequestNew_QR_Code", $"Got Exception: {e.Message}");
                return false;
            }
        }

        private async Task ProcessQRCode()
        {
            var requestSuccess = await RequestNew_QR_Code(_uuid, ApplicationStateManager.RigID());
            if (requestSuccess)
            {
                // create qr code
                rect_qrCode.Fill = QrCodeHelpers.GetQRCode(_uuid);
            }
            else
            {
                ScanLabel.Visibility = Visibility.Collapsed;
                ScanConfirmButton.Visibility = Visibility.Collapsed;
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
            var btc = await GetBTCForUUID(_uuid);
            if (CredentialValidators.ValidateBitcoinAddress(btc))
            {
                Logger.Info("LoginWindow.Confirm_Scan_ClickTask", $"Got valid btc address: {btc}");
                CredentialsSettings.Instance.SetBitcoinAddress(btc);
                Close();
            }
        }
    }
}

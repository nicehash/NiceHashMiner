using Newtonsoft.Json;
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

        private async Task ProcessQRCode()
        {
            var rigID = ApplicationStateManager.RigID();

            var requestBody = "{\"qrId\":\"" + _uuid + "\", \"rigId\":\"" + rigID + "\"}";
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync("https://api2.nicehash.com/api/v2/organization/nhmqr", content);
            }
            // create qr code
            rect_qrCode.Fill = QrCodeHelpers.GetQRCode(_uuid);
        }

        [Serializable]
        private class BtcResponse
        {
            public string btc { get; set; }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await BtnClickTask();
        }

        private async Task BtnClickTask()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var resp = await client.GetAsync($"https://api2.nicehash.com/api/v2/organization/nhmqr/{_uuid}");
                    if (resp.IsSuccessStatusCode)
                    {
                        var contentString = await resp.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(contentString))
                        {
                            var btcResp = JsonConvert.DeserializeObject<BtcResponse>(contentString);
                            if (btcResp.btc != null)
                            {
                                if (CredentialValidators.ValidateBitcoinAddress(btcResp.btc))
                                {
                                    CredentialsSettings.Instance.SetBitcoinAddress(btcResp.btc);
                                    Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

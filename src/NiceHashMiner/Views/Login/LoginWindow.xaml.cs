using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using NHMCore.Configs;
using NHMCore.Utils;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using ZXing.Rendering;
using ZXing;
using ZXing.QrCode.Internal;
using System.Drawing;
using ZXing.Common;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Net.Http;
using NHMCore;
using System;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using NiceHashMiner.ViewModels;

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        private string _uuid = Guid.NewGuid().ToString();
        public LoginWindow()
        {
            InitializeComponent();
            HideIconAndTitle = true;
            WindowUtils.Translate(this);
            _ = ProcessQRCode();
        }

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
            var browser = new LoginBrowser();
            browser.ShowDialog();
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

            //if all ok start timer to poll
            using (var client = new HttpClient())
            {
                while (true)
                {
                    await Task.Delay(5000);
                    try
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        [Serializable]
        private class BtcResponse
        {
            public string btc { get; set; }
        }
    }
}

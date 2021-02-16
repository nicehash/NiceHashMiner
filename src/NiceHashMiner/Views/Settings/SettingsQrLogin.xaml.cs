using Newtonsoft.Json;
using NHMCore;
using NHMCore.Configs;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsQrLogin.xaml
    /// </summary>
    public partial class SettingsQrLogin : System.Windows.Controls.UserControl
    {
        private string _uuid = Guid.NewGuid().ToString();
        Stopwatch stopWatch;

        public SettingsQrLogin()
        {
            InitializeComponent();
            _ = ProcessQRCode();
            lbl_qr_status.Visibility = Visibility.Collapsed;
            btn_gen_qr.Visibility = Visibility.Collapsed;
        }

        private async Task ProcessQRCode()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();

            var rigID = ApplicationStateManager.RigID();

            var requestBody = "{\"qrId\":\"" + _uuid + "\", \"rigId\":\"" + rigID + "\"}";
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync("https://api2.nicehash.com/api/v2/organization/nhmqr", content);

                // create qr code
                rect_qrCode.Fill = QrCodeHelpers.GetQRCode(_uuid, GUISettings.Instance.DisplayTheme == "Light");

                while (true)
                {
                    await Task.Delay(5000);

                    if (stopWatch.ElapsedMilliseconds >= (1000 * 60 * 10))
                    {
                        lbl_qr_status.Visibility = Visibility.Visible;
                        btn_gen_qr.Visibility = Visibility.Visible;
                        lbl_qr_status.Content = Translations.Tr("QR Code timeout. Please generate new one.");
                        return;
                    }
                }
            }
        }

        [Serializable]
        private class BtcResponse
        {
            public string btc { get; set; }
        }

        private async void btn_gen_qr_Click(object sender, RoutedEventArgs e)
        {
            lbl_qr_status.Visibility = Visibility.Collapsed;
            btn_gen_qr.Visibility = Visibility.Collapsed;
            await ProcessQRCode();
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
                                var ret = await ApplicationStateManager.SetBTCIfValidOrDifferent(btcResp.btc);
                                if (ret == ApplicationStateManager.SetResult.CHANGED)
                                {
                                    lbl_qr_status.Visibility = Visibility.Visible;
                                    btn_gen_qr.Visibility = Visibility.Visible;
                                    lbl_qr_status.Content = Translations.Tr("BTC Address was changed - this code is already used.");
                                }
                                return;
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

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

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string _uuid = Guid.NewGuid().ToString();
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
            CreateQRCode(_uuid, false);
        }

        private void CheckBoxMode_Unchecked(object sender, RoutedEventArgs e)
        {
            GUISettings.Instance.DisplayTheme = "Light";
            ThemeSetterManager.SetTheme(true);
            CreateQRCode(_uuid, true);
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

            var requestBody = "{\"qrId\":\""+ _uuid + "\", \"rigId\":\"" + rigID + "\"}";
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            //var response = await client.PostAsync("https://api2.nicehash.com/api/v2/organization/nhmqr", content);
            var response = await client.PostAsync("https://api-test-dev.nicehash.com/api/v2/organization/nhmqr", content);

            var responseString = response.Content.ReadAsStringAsync();

            // create qr code
            CreateQRCode(_uuid);

            //if all ok start timer to poll
            while (true)
            {
                await Task.Delay(2000);
                try
                {
                    //var resp = client.GetAsync($"https://api2.nicehash.com/api/v2/organization/nhmqr/{uuid}").Result;
                    var resp = client.GetAsync($"https://api-test-dev.nicehash.com/api/v2/organization/nhmqr/{_uuid}").Result;
                    if (resp.IsSuccessStatusCode)
                    {
                        var contentString = resp.Content.ReadAsStringAsync().Result;
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

        private void CreateQRCode(string uuid, bool LightTheme = true)
        {
            var encOptions = new EncodingOptions
            {
                Width = 160,
                Height = 160,
                Margin = 0,
                PureBarcode = false
            };
            encOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);

            var bw = new BarcodeWriter();
            bw.Renderer = new BitmapRenderer();
            bw.Options = encOptions;
            bw.Format = BarcodeFormat.QR_CODE;

            var bm = bw.Write(uuid);
            try
            {
                if (!LightTheme)
                {
                    for (int j = 0; (j <= (bm.Height - 1)); j++)
                    {
                        for (int k = 0; (k <= (bm.Width - 1)); k++)
                        {
                            var inv = bm.GetPixel(k, j);
                            inv = System.Drawing.Color.FromArgb(255, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                            bm.SetPixel(k, j, inv);
                        }
                    }
                }               

                var overlay = new Bitmap("../Resources/logo.png");
                var g = Graphics.FromImage(bm);
                var x = (bm.Width - overlay.Width) / 2;
                var y = (bm.Height - overlay.Height) / 2;
                g.FillRectangle(new SolidBrush(System.Drawing.Color.White), x, y, overlay.Width, overlay.Height);
                g.DrawImage(overlay, new System.Drawing.Point(x, y));

                //bmp to bmpimg
                BitmapImage bitmapImage;
                using (var memory = new MemoryStream())
                {
                    bm.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
                // end of bmp to bmpimg       

                var brush = new ImageBrush(bitmapImage);
                rect_qrCode.Fill = brush;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }           
        }

        [Serializable]
        private class BtcResponse
        {
            public string btc { get; set; }
        }
    }
}

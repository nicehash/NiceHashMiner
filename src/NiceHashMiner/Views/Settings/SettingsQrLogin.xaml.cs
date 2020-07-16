using Newtonsoft.Json;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsQrLogin.xaml
    /// </summary>
    public partial class SettingsQrLogin : System.Windows.Controls.UserControl
    {
        private string _uuid = Guid.NewGuid().ToString();
        public SettingsQrLogin()
        {
            InitializeComponent();
            _ = ProcessQRCode();
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
            CreateQRCode(_uuid);

            //if all ok start timer to poll
            while (true)
            {
                await Task.Delay(2000);
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
                                        var ret = await ApplicationStateManager.SetBTCIfValidOrDifferent(btcResp.btc);
                                        if(ret == ApplicationStateManager.SetResult.CHANGED)
                                        {
                                            System.Windows.Forms.MessageBox.Show("BTC Address was changed.", "SUCCESS", MessageBoxButtons.OK);
                                        }
                                        return;
                                    }
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
            if (GUISettings.Instance.DisplayTheme == "Dark") LightTheme = false;
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
                var overlay = new Bitmap("../Resources/logoLight32.png");
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
                    overlay = new Bitmap("../Resources/logoDark32.png");
                }

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CreateQRCode(_uuid);
        }
    }
}

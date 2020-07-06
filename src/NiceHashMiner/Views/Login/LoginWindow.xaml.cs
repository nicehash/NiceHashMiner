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
using System.Collections.Generic;
using NHMCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        private static readonly HttpClient client = new HttpClient();
        private Timer _evalTimer;
        private object _lock = new object();

        public LoginWindow()
        {
            InitializeComponent();
            HideIconAndTitle = true;
            WindowUtils.Translate(this);
            ProcessQRCode();           
        }

        private void CheckBoxMode_Checked(object sender, RoutedEventArgs e)
        {
            GUISettings.Instance.DisplayTheme = "Dark";
            ThemeSetterManager.SetTheme(false);
        }

        private void CheckBoxMode_Unchecked(object sender, RoutedEventArgs e)
        {
            GUISettings.Instance.DisplayTheme = "Light";
            ThemeSetterManager.SetTheme(true);
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

        private void ProcessQRCode()
        {
            var uuid = Guid.NewGuid().ToString();
            var rigID = ApplicationStateManager.RigID();
            var values = new Dictionary<string, string>
            {
                { "qrId", uuid },
                { "rigId", rigID }
            };

            var content = new FormUrlEncodedContent(values);

            //var response = client.PostAsync("https://api2.nicehash.com/api/v2/organization/nhmqr", content).Result;
            var response = client.PostAsync("https://api-test.nicehash.com/api/v2/organization/nhmqr", content).Result;

            var responseString = response.Content.ReadAsStringAsync();

            // create qr code
            CreateQRCode(uuid);

            //if all ok start timer to poll
            _evalTimer = new Timer((s) => { Dispatcher.Invoke(EvalTimer_Elapsed); }, null, 100, 1000);
        }

        private async void EvalTimer_Elapsed()
        {
            await EvalTimer_ElapsedTask();
        }

        private async Task EvalTimer_ElapsedTask()
        {
            using (var tryLock = new TryLock(_lock))
            {
                if (!tryLock.HasAcquiredLock) return;
                try
                {
                    var response = await client.GetAsync("https://api-test.nicehash.com/api/v2/organization/nhmqr");

                    _evalTimer.Dispose();
                    Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void CreateQRCode(string uuid)
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

            //convert drawing group to bitmap
            var drawingLogo = Application.Current.FindResource("LoginQRCircleLight") as Drawing;
            var drawingLogoImage = new DrawingImage(drawingLogo);
            Bitmap overlay;
            using (var ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(ToBitmapSource(drawingLogoImage)));
                encoder.Save(ms);

                using (var bmp = new Bitmap(ms))
                {
                    overlay = new Bitmap(bmp);
                }
            }
            // end of converting

            var bm = bw.Write(uuid);
            int deltaHeigth = bm.Height - overlay.Height;
            int deltaWidth = bm.Width - overlay.Width;

            var g = Graphics.FromImage(overlay);
            g.DrawImage(overlay, new System.Drawing.Point(deltaWidth / 2, deltaHeigth / 2));

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

        private BitmapSource ToBitmapSource(DrawingImage source)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(source, new Rect(new System.Windows.Point(0, 0), new System.Windows.Size(source.Width, source.Height)));
            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)source.Width, (int)source.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }

        internal class TryLock : IDisposable
        {
            private object locked;
            public bool HasAcquiredLock { get; private set; }
            public TryLock(object obj)
            {
                if (Monitor.TryEnter(obj))
                {
                    HasAcquiredLock = true;
                    locked = obj;
                }
            }
            public void Dispose()
            {
                if (HasAcquiredLock)
                {
                    Monitor.Exit(locked);
                    locked = null;
                    HasAcquiredLock = false;
                }
            }
        }
    }
}

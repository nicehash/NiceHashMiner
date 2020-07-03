using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using NHMCore.Configs;
using NHMCore.Utils;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using ZXing.Rendering;
using ZXing;
using ZXing.QrCode.Internal;
using System.Drawing;
using ZXing.Common;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            HideIconAndTitle = true;
            GetQRCode();
            WindowUtils.Translate(this);
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

        private void GetQRCode()
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
            var drawingLogo = Application.Current.FindResource("NHMLogoDark") as Drawing;
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

            var bm = bw.Write("test");
            int deltaHeigth = bm.Height - overlay.Height;
            int deltaWidth = bm.Width - overlay.Width;

            var g = Graphics.FromImage(overlay);
            g.DrawImage(overlay, new System.Drawing.Point(20, 20));
            //g.DrawImage(overlay, new System.Drawing.Point(deltaWidth / 2, deltaHeigth / 2));

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
    }
}

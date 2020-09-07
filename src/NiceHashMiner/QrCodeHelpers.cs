using NHM.Common;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace NiceHashMiner
{
    class QrCodeHelpers
    {
        public static ImageBrush GetQRCode(string uuid, bool LightTheme = true)
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
                var overlay = new Bitmap(Properties.Resources.logoLight32);
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
                    overlay = new Bitmap(Properties.Resources.logoDark32);
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
                return brush;
            }
            catch (Exception ex)
            {
                Logger.Error("QRCode", ex.Message);
                return new ImageBrush();
            }
        }
    }
}

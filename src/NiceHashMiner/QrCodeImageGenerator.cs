using NHM.Common;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace NiceHashMiner
{
    static class QrCodeImageGenerator
    {
        public static (ImageBrush, bool) GetQRCodeImage(string uuid, bool LightTheme = true)
        {
            var encOptions = new EncodingOptions
            {
                Width = 160,
                Height = 160,
                Margin = 0,
                PureBarcode = false
            };
            encOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);

            var bw = new QRCodeWriter();
            
            var bm = bw.encode(uuid, BarcodeFormat.QR_CODE, 160, 160, encOptions.Hints);
            var bitMap = new Bitmap(160, 160);
            try
            {
                var overlay = new Bitmap(Properties.Resources.logoLight32);
                if (!LightTheme)
                {
                    for (int j = 0; (j <= (bitMap.Height - 1)); j++)
                    {
                        for (int k = 0; (k <= (bitMap.Width - 1)); k++)
                        {
                            var inv = bm[k, j];
                            if (inv)
                            {
                                bitMap.SetPixel(k, j, System.Drawing.Color.FromArgb(255, 255, 255, 255));
                            }
                            else bitMap.SetPixel(k, j, System.Drawing.Color.FromArgb(255, 0, 0, 0));
                        }
                    }
                    overlay = new Bitmap(Properties.Resources.logoDark32);
                }
                else{
                    for (int j = 0; (j <= (bitMap.Height - 1)); j++)
                    {
                        for (int k = 0; (k <= (bitMap.Width - 1)); k++)
                        {
                            var inv = bm[k, j];
                            if (inv)
                            {
                                bitMap.SetPixel(k, j, System.Drawing.Color.FromArgb(255, 0, 0, 0));
                            }
                            else bitMap.SetPixel(k, j, System.Drawing.Color.FromArgb(255, 255, 255, 255));
                        }
                    }
                }

                var g = Graphics.FromImage(bitMap);
                var x = (bitMap.Width - overlay.Width) / 2;
                var y = (bitMap.Height - overlay.Height) / 2;
                g.FillRectangle(new SolidBrush(System.Drawing.Color.White), x, y, overlay.Width, overlay.Height);
                g.DrawImage(overlay, new Point(x, y));

                //bmp to bmpimg
                BitmapImage bitmapImage;
                using (var memory = new MemoryStream())
                {
                    bitMap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
                // end of bmp to bmpimg

                var brush = new ImageBrush(bitmapImage);
                return (brush, true);
            }
            catch (Exception ex)
            {
                Logger.Error("QRCodeImageGenerator", ex.Message);
                return (new ImageBrush(), false);
            }
        }
    }
}

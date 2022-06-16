using NHMCore;
using NHMCore.Configs;
using NHMCore.Nhmws;
using NHMCore.Utils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsQrLogin.xaml
    /// </summary>
    public partial class SettingsQrLogin : System.Windows.Controls.UserControl
    {
        private string _uuid = System.Guid.NewGuid().ToString();
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
            var res = await BTC_FromQrCodeAPI.RequestNew_QR_Code(_uuid, rigID);

            if (!res)
            {
                lbl_qr_status.Visibility = Visibility.Visible;
                lbl_qr_status.Content = "Unable to retreive QR Code";
                return;
            }

            var (image, ok) = QrCodeImageGenerator.GetQRCodeImage(_uuid, GUISettings.Instance.DisplayTheme == "Light");

            if (!ok)
            {
                lbl_qr_status.Visibility = Visibility.Visible;
                lbl_qr_status.Content = "QR Code image generation failed";
                return;
            }

            rect_qrCode.Fill = image;
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
                var btc = await BTC_FromQrCodeAPI.GetBTCForUUID(_uuid);
                if (btc == null) return;
                var ret = await ApplicationStateManager.SetBTCIfValidOrDifferent(btc);
                if (ret == NhmwsSetResult.CHANGED)
                {
                    lbl_qr_status.Visibility = Visibility.Visible;
                    btn_gen_qr.Visibility = Visibility.Visible;
                    lbl_qr_status.Content = Translations.Tr("BTC Address was changed - this code is already used.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

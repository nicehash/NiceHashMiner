using NHMCore;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NiceHashMiner.Views.EULA
{
    /// <summary>
    /// Interaction logic for EULA.xaml
    /// </summary>
    public partial class EULA : System.Windows.Controls.UserControl
    {
        public event EventHandler<RoutedEventArgs> OKClick;

        public EULA()
        {
            InitializeComponent();

            using (var stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(NHMCore.Properties.Resources.Eula)))
            {
                stream.Position = 0;
                EulaRtb.SelectAll();
                EulaRtb.Selection.Load(stream, System.Windows.DataFormats.Rtf);
            }
            EulaRtb.SelectAll();
            EulaRtb.Selection.ApplyPropertyValue(FontFamilyProperty, FindResource("UbuntuFontFamily") as FontFamily);
        }

        private void EulaRtb_OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void Hyperlink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            Process.Start(hyperlink.NavigateUri.ToString());
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToSSetings.Instance.AgreedWithTOS = ApplicationStateManager.CurrentTosVer;
            this.Visibility = Visibility.Hidden;
            OKClick?.Invoke(this, e);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationStateManager.ExecuteApplicationExit();
        }
    }
}

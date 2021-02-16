using NiceHashMiner.Views.Common.NHBase;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for EulaWindow.xaml
    /// </summary>
    public partial class EulaWindowFirstLong : BaseDialogWindow
    {
        public bool AcceptedTos { get; set; } = false;

        public EulaWindowFirstLong()
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

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            AcceptedTos = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Hyperlink_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            Process.Start(hyperlink.NavigateUri.ToString());
        }
    }
}

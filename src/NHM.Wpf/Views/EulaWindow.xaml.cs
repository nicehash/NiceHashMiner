using NHM.Wpf.Views.Common;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for EulaWindow.xaml
    /// </summary>
    public partial class EulaWindow : Window
    {
        public bool AcceptedTos { get; set; } = false;

        public EulaWindow()
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
            WindowUtils.InitWindow(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }

        private void EulaRtb_OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void EulaWindow_OnClosing(object sender, CancelEventArgs e)
        {
            WindowUtils.Window_OnClosing(this);
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

using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;

namespace NHM.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for EulaWindow.xaml
    /// </summary>
    public partial class EulaWindow : Window
    {
        private bool _acceptedTos;

        public EulaWindow()
        {
            InitializeComponent();

            EulaRtb.Rtf = Properties.Resources.Eula;
        }

        private void EulaRtb_OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void EulaWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_acceptedTos) e.Cancel = true;
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            _acceptedTos = true;
            Close();
        }
    }
}

using System.Windows;
using NHM.Wpf.Views.Common;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for _3rdPartyTosWindow.xaml
    /// </summary>
    public partial class _3rdPartyTosWindow : Window
    {
        public bool Accepted { get; private set; } = false;

        public _3rdPartyTosWindow()
        {
            InitializeComponent();

            WindowUtils.InitWindow(this);
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }

        private void EulaWindow_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowUtils.Window_OnClosing(this);
        }
    }
}

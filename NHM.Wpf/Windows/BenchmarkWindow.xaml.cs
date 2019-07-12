using System.Windows;
using NHM.Wpf.Windows.Common;
using NHM.Wpf.Windows.Settings.Pages;

namespace NHM.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for BenchmarkWindow.xaml
    /// </summary>
    public partial class BenchmarkWindow : Window, ISettingsPage
    {
        public BenchmarkWindow()
        {
            InitializeComponent();

            WindowUtils.Translate(this);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

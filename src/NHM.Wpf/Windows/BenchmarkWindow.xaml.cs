using NHM.Wpf.ViewModels;
using NHM.Wpf.Windows.Common;
using NHM.Wpf.Windows.Settings.Pages;
using NiceHashMiner.Mining;
using System.Collections.Generic;
using System.Windows;

namespace NHM.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for BenchmarkWindow.xaml
    /// </summary>
    public partial class BenchmarkWindow : Window, ISettingsPage
    {
        private readonly BenchmarkViewModel _vm;

        public BenchmarkWindow(IEnumerable<ComputeDevice> devices)
        {
            InitializeComponent();

            if (DataContext is BenchmarkViewModel vm)
                _vm = vm;
            else
            {
                _vm = new BenchmarkViewModel();
                DataContext = _vm;
            }

            _vm.Devices = devices;

            WindowUtils.Translate(this);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            _vm.CommitBenchmarks();
            Close();
        }
    }
}

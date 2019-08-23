using System;
using NHM.Wpf.ViewModels;
using NHM.Wpf.Windows.Common;
using NHM.Wpf.Windows.Settings.Pages;
using NiceHashMiner.Mining;
using System.Collections.Generic;
using System.Windows;
using NiceHashMiner.Benchmarking;

namespace NHM.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for BenchmarkWindow.xaml
    /// </summary>
    public partial class BenchmarkWindow : Window, ISettingsPage, IDisposable
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
            _vm.OnBenchEnd += OnBenchEnd;

            WindowUtils.Translate(this);
        }

        private void OnBenchEnd(object sender, BenchEndEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!e.StartMining)
                {
                    MessageBox.Show(
                        !e.DidAlgosFail
                            ? Translations.Tr("All benchmarks have been successful")
                            : Translations.Tr("Not all benchmarks finished successfully."),
                        Translations.Tr("Benchmark finished report"),
                        MessageBoxButton.OK);
                }
                else
                {
                    // MainWindow will look for this result and start mining if it's true
                    DialogResult = true;
                    Close();
                }
            });
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            _vm.CommitBenchmarks();
            Close();
        }

        private void StartStopButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_vm.InBenchmark)
                _vm.StopBenchmark();
            else
                _vm.StartBenchmark();
        }

        public void Dispose()
        {
            _vm.OnBenchEnd -= OnBenchEnd;
            _vm.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NHM.Wpf.ViewModels;
using NHM.Wpf.Views.Common;
using NHM.Wpf.Views.Settings.Pages;
using NHMCore;
using NHMCore.Benchmarking;
using NHMCore.Mining;

namespace NHM.Wpf.Views
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

            _vm = this.AssertViewModel<BenchmarkViewModel>();

            _vm.Devices = devices;
            _vm.OnBenchEnd += OnBenchEnd;

            WindowUtils.InitWindow(this);
        }

        private void OnBenchEnd(object sender, BenchEndEventArgs e)
        {
            Dispatcher?.Invoke(() =>
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.CommitBenchmarks();
        }

        private void ContextMenu_OnOpening(object sender, ContextMenuEventArgs e)
        {
            // Don't open during bench
            if (_vm.InBenchmark) e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }
    }
}

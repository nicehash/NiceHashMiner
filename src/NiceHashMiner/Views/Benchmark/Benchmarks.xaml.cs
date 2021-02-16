using NHM.Common.Enums;
using NiceHashMiner.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.Benchmark
{
    /// <summary>
    /// Interaction logic for Benchmarks.xaml
    /// </summary>
    public partial class Benchmarks : UserControl
    {
        private MainVM _vm;
        public Benchmarks()
        {
            InitializeComponent();
            DataContextChanged += Benchmarks_DataContextChanged;
        }

        private void Benchmarks_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MainVM mainVM)
            {
                _vm = mainVM;
                BenchmarkTypeContextMenu.DataContext = e.NewValue;
                return;
            }
            throw new Exception("Benchmarks_DataContextChanged e.NewValue must be of type MainVM");
        }

        private readonly HashSet<Button> _toggleButtonsGuard = new HashSet<Button>();

        private void BenchmarkTypeContextMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                _toggleButtonsGuard.Add(tButton);
                BenchmarkTypeContextMenu.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) =>
                {
                    _toggleButtonsGuard.Remove(tButton);
                    //tButton.IsChecked = false;
                    BenchmarkTypeContextMenu.Closed -= closedHandler;
                };
                BenchmarkTypeContextMenu.Closed += closedHandler;
            }

        }

        private void SetStandard(object sender, RoutedEventArgs e)
        {
            _vm.BenchmarkSettings.SelectedBenchmarkType = BenchmarkPerformanceType.Standard;
            BenchmarkTypeContextMenu.IsOpen = false;
        }

        private void SetQuick(object sender, RoutedEventArgs e)
        {
            _vm.BenchmarkSettings.SelectedBenchmarkType = BenchmarkPerformanceType.Quick;
            BenchmarkTypeContextMenu.IsOpen = false;
        }

        private void SetPrecise(object sender, RoutedEventArgs e)
        {
            _vm.BenchmarkSettings.SelectedBenchmarkType = BenchmarkPerformanceType.Precise;
            BenchmarkTypeContextMenu.IsOpen = false;
        }

        private void BenchmarkButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.BenchmarkSettings.StartBenchmark();
        }
    }
}

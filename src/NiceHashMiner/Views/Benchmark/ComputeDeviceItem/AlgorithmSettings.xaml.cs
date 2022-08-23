using NHMCore.Mining;
using NiceHashMiner.Views.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    /// <summary>
    /// Interaction logic for AlgorithmSettings.xaml
    /// </summary>
    public partial class AlgorithmSettings : UserControl
    {
        public event EventHandler<RoutedEventArgs> CloseClick;
        //public event EventHandler<RoutedEventArgs> SaveClick;

        AlgorithmContainer _algorithmContainer;
        public AlgorithmSettings()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is AlgorithmContainer algorithmContainer)
                {
                    _algorithmContainer = algorithmContainer;
                    secondarySpeedPanel.Visibility = _algorithmContainer.IsDual ? Visibility.Visible : Visibility.Collapsed;
                    ToggleButtonHidden.Visibility = _algorithmContainer.HasBenchmark ? Visibility.Visible : Visibility.Collapsed;
                    _algorithmContainer.PropertyChanged += _algorithmContainer_PropertyChanged;
                    return;
                }
                throw new Exception("unsupported datacontext type");
            };
            WindowUtils.Translate(this);
        }

        private void _algorithmContainer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlgorithmContainer.HasBenchmark))
            {
                ToggleButtonHidden.Visibility = _algorithmContainer.HasBenchmark ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            CloseClick?.Invoke(sender, e);
        }

        private void ToggleClickReBenchHandler(object sender, RoutedEventArgs e)
        {
            var tb = e.Source as ToggleButton;
            if (ToggleButtonHidden == tb)
            {
                _algorithmContainer.IsReBenchmark = !_algorithmContainer.IsReBenchmark;
            }
        }

        private void EnableOnlyThisAlgorithmClick(object sender, RoutedEventArgs e)
        {
            //_algorithmContainer.Enabled = true;
            foreach (var algo in _algorithmContainer.ComputeDevice.AlgorithmSettings)
            {
                algo.Enabled = algo == _algorithmContainer;
            }
        }
    }
}

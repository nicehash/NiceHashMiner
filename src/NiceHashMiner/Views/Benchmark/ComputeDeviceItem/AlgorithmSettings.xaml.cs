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
            switch (e.PropertyName)
            {
                case nameof(AlgorithmContainer.HasBenchmark):
                    ToggleButtonHidden.Visibility = _algorithmContainer.HasBenchmark ? Visibility.Visible : Visibility.Collapsed;
                    return;
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            bool found = false;
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            border.Background = Application.Current.Resources["BackgroundColor"] as Brush;
            border.BorderBrush = Application.Current.Resources["BorderColor"] as Brush;
            var minerOptionsPackage = _algorithmContainer.PluginContainer.GetMinerOptionsPackage();
            if (minerOptionsPackage == null) return;

            string queryFull = (sender as TextBox).Text;

            var elpParams = queryFull.Split(' ').ToList();
            var query = elpParams.LastOrDefault();

            if (query?.Length == 0)
            {
                // Clear   
                resultStack.Children.Clear();
                border.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                border.Visibility = System.Windows.Visibility.Visible;
            }

            // Clear the list   
            resultStack.Children.Clear();

            // Add the result   
            if (minerOptionsPackage != null && minerOptionsPackage.GeneralOptions != null)
            {
                foreach (var generalOption in minerOptionsPackage.GeneralOptions)
                {
                    // skip if already present 
                    var skipOption = false;
                    foreach (var elpParam in elpParams ?? Enumerable.Empty<string>())
                    {
                        bool longNameContains = generalOption.LongName != null && elpParam.Contains(generalOption.LongName);
                        bool shortNameContains = generalOption.ShortName != null && elpParam.Contains(generalOption.ShortName);
                        if (longNameContains || shortNameContains)
                        {
                            skipOption = true;
                            break;
                        }
                    }
                    if (skipOption) continue;

                    // add to auto-complete
                    if (generalOption.LongName != null && generalOption.LongName.ToLower().StartsWith(query.ToLower()))
                    {
                        addItem(generalOption.LongName);
                        found = true;
                    }
                    if (generalOption.ShortName != null && generalOption.ShortName.ToLower().StartsWith(query.ToLower()))
                    {
                        addItem(generalOption.ShortName);
                        found = true;
                    }
                }

            }

            if (!found)
            {
                border.Visibility = System.Windows.Visibility.Collapsed;
            }
        }


        private void addItem(string text)
        {
            TextBlock block = new TextBlock();

            // Add the text   
            block.Text = text;

            // A little style...   
            block.Margin = new Thickness(2, 3, 2, 3);
            block.Cursor = Cursors.Hand;
            block.Foreground = Application.Current.Resources["TextColorBrush"] as Brush;

            // Mouse events   
            block.MouseLeftButtonUp += (sender, e) =>
            {
                var elpParams = textBox.Text.Split(' ').ToList();
                List<string> allButLast = new List<string>();
                if (elpParams.Count > 1)
                {
                    allButLast = elpParams.GetRange(0, elpParams.Count - 1);
                }
                allButLast.Add((sender as TextBlock).Text);
                textBox.Text = string.Join(" ", allButLast);
            };

            block.MouseEnter += (sender, e) =>
            {
                TextBlock b = sender as TextBlock;
                b.Background = Application.Current.Resources["BorderColor"] as Brush;
            };

            block.MouseLeave += (sender, e) =>
            {
                TextBlock b = sender as TextBlock;
                b.Background = Brushes.Transparent;
            };

            // Add to the panel   
            resultStack.Children.Add(block);
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

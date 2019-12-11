using NHMCore.Mining;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NHM.Wpf.Views.Benchmark.ComputeDeviceItem
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

            DataContextChanged += (s, e) => {
                if (e.NewValue is AlgorithmContainer algorithmContainer)
                {
                    _algorithmContainer = algorithmContainer;
                    secondarySpeedPanel.Visibility = _algorithmContainer.IsDual ? Visibility.Visible : Visibility.Collapsed;
                    return;
                }
                throw new Exception("unsupported datacontext type");
            };
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            bool found = false;
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            var minerOptionsPackage = _algorithmContainer.PluginContainer.GetMinerOptionsPackage();
            if (minerOptionsPackage == null) return;

            string queryFull = (sender as TextBox).Text;

            var elpParams = queryFull.Split(' ').ToList();
            List<string> allButLast = null;
            if (elpParams.Count > 1)
            {
                allButLast = elpParams.GetRange(0, elpParams.Count - 1);
            }
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
            foreach (var generalOption in minerOptionsPackage.GeneralOptions)
            {
                // skip if already present 
                var skipOption = false;
                foreach (var elpParam in allButLast ?? Enumerable.Empty<string>())
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
                b.Background = Brushes.PeachPuff;
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
    }
}

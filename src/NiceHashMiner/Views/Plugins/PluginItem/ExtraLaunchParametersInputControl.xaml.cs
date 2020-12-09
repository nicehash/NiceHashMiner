using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using NHMCore.Mining;
using NiceHashMiner.ViewModels.Plugins;
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

namespace NiceHashMiner.Views.Plugins.PluginItem
{
    /// <summary>
    /// Interaction logic for ElpInputControl.xaml
    /// </summary>
    public partial class ExtraLaunchParametersInputControl : UserControl
    {
        public static readonly DependencyProperty UUIDProperty =
           DependencyProperty.Register("PluginUUID", typeof(string), typeof(ExtraLaunchParametersInputControl));

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("DataType", typeof(string), typeof(ExtraLaunchParametersInputControl));

        public ExtraLaunchParametersInputControl()
        {
            InitializeComponent();
        }

        public string PluginUUID { get => (string)GetValue(UUIDProperty); }
        public string DataType { get => (string)GetValue(TypeProperty); }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            bool found = false;
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            border.Background = Application.Current.Resources["BackgroundColor"] as Brush;
            border.BorderBrush = Application.Current.Resources["BorderColor"] as Brush;

            var miner = new MinerOptionsPackage();
            var _minerOptionsPackage = miner;
            if (_minerOptionsPackage == null) return;

            string queryFull = (sender as TextBox).Text;

            var elpParams = queryFull.Split(' ').ToList();
            var query = elpParams.LastOrDefault();

            if (query?.Length == 0)
            {
                // Clear   
                resultStack.Children.Clear();
                border.Visibility = Visibility.Collapsed;
            }
            else
            {
                border.Visibility = Visibility.Visible;
            }

            // Clear the list   
            resultStack.Children.Clear();

            // Add the result   
            if (_minerOptionsPackage != null && _minerOptionsPackage.GeneralOptions != null)
            {
                foreach (var generalOption in _minerOptionsPackage.GeneralOptions)
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
                border.Visibility = Visibility.Collapsed;
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
                var elpParams = tbx_elp.Text.Split(' ').ToList();
                List<string> allButLast = new List<string>();
                if (elpParams.Count > 1)
                {
                    allButLast = elpParams.GetRange(0, elpParams.Count - 1);
                }
                allButLast.Add((sender as TextBlock).Text);
                tbx_elp.Text = string.Join(" ", allButLast);
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
    }
}

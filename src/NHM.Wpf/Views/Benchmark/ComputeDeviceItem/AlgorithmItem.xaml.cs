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
    /// Interaction logic for AlgorithmItem.xaml
    /// </summary>
    public partial class AlgorithmItem : UserControl
    {
        public AlgorithmItem()
        {
            InitializeComponent();

            DataContextChanged += AlgorithmItem_DataContextChanged;
        }

        private void AlgorithmItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is AlgorithmContainer)
            {
                AlgorithmSettingsContextMenu.DataContext = e.NewValue;
                return;
            }
            throw new Exception("unsupported datacontext type");
        }

        private readonly HashSet<Button> _toggleButtonsGuard = new HashSet<Button>();
        private void AlgorithmSettings_Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                _toggleButtonsGuard.Add(tButton);
                AlgorithmSettingsContextMenu.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) => {
                    _toggleButtonsGuard.Remove(tButton);
                    //tButton.IsChecked = false;
                    AlgorithmSettingsContextMenu.Closed -= closedHandler;
                };
                AlgorithmSettingsContextMenu.Closed += closedHandler;
            }
        }

        private void CloseAlgorithmSettings_Button_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmSettingsContextMenu.IsOpen = false;
        }
    }
}

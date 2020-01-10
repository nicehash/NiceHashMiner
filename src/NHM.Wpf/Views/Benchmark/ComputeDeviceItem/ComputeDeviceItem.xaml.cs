using NHM.Wpf.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for ComputeDeviceItem.xaml
    /// </summary>
    public partial class ComputeDeviceItem : UserControl
    {
        private DeviceData _deviceData;

        public ComputeDeviceItem()
        {
            InitializeComponent();

            DataContextChanged += ComputeDeviceItem_DataContextChanged;
            AlgorithmsGrid.Visibility = Visibility.Collapsed;
        }

        private void ComputeDeviceItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DeviceData dd)
            {
                _deviceData = dd;
                return;
            }
            //throw new Exception("ComputeDeviceItem_DataContextChanged e.NewValue must be of type DeviceData");
        }

        private void Collapse()
        {
            AlgorithmsGrid.Visibility = Visibility.Collapsed;
            AlgorithmsGridToggleButton.IsChecked = false;
            AlgorithmsGridToggleButtonHidden.IsChecked = false;
        }

        private void Expand()
        {
            AlgorithmsGrid.Visibility = Visibility.Visible;
            AlgorithmsGridToggleButton.IsChecked = true;
            AlgorithmsGridToggleButtonHidden.IsChecked = true;
        }

        private void DropDownAlgorithms_Button_Click(object sender, RoutedEventArgs e)
        {
            var tb = e.Source as ToggleButton;
            if (EnableDisableCheckBox == tb) return; // don't trigger algo dropdown if we click disable button
            if (tb.IsChecked.Value)
            {
                Expand();
            }
            else
            {
                Collapse();
            }
        }

        private void Action_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Algorithm sorting
        private void SortAlgorithmButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByAlgorithm();
        }

        private void SortMinerButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByPlugin();
        }

        private void SortSpeedButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsBySpeeds();
        }

        private void SortPayingButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByPaying();
        }

        private void SortStatusButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByStatus();
        }

        private void SortEnabledButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByEnabled();
        }
        #endregion Algorithm sorting
    }
}

using NHM.Common;
using NHM.Wpf.ViewModels.Models;
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

namespace NHM.Wpf.Views.Benchmark
{
    /// <summary>
    /// Interaction logic for ComputeDeviceElement.xaml
    /// </summary>
    public partial class ComputeDeviceElement : UserControl
    {
        private DeviceData _deviceData;

        public ComputeDeviceElement()
        {
            InitializeComponent();
            DataContextChanged += ComputeDeviceElement_DataContextChanged;
        }

        private void ComputeDeviceElement_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DeviceData dd)
            {
                _deviceData = dd;
                return;
            }
            throw new Exception("ComputeDeviceElement_DataContextChanged e.NewValue must be of type DeviceData");
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            Logger.Debug("ComputeDeviceElement", $"{e.Source}");
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked == null) return;
            
            if (PLUGIN_NAME_HEADER == headerClicked.Column)
            {
                _deviceData.OrderAlgorithmsByPlugin();
            }
            if (ALGORITHM_HEADER == headerClicked.Column)
            {
                _deviceData.OrderAlgorithmsByAlgorithm();
            }
            if (SPEEDS_HEADER == headerClicked.Column)
            {
                _deviceData.OrderAlgorithmsBySpeeds();
            }
            if (PAYING_HEADER == headerClicked.Column)
            {
                _deviceData.OrderAlgorithmsByPaying();
            }
            if (STATUS_HEADER == headerClicked.Column)
            {
                _deviceData.OrderAlgorithmsByStatus();
            }
            if (ENABLED_HEADER == headerClicked.Column)
            {
                _deviceData.OrderAlgorithmsByEnabled();
            }
        }
    }
}

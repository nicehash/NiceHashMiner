using NiceHashMiner.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace NiceHashMiner.Views.Devices
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
            //DataContext = this;
        }

        private void ComputeDeviceItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
            if (DataContext is DeviceData dd)
            {
                _deviceData = dd;
                return;
            }
            throw new Exception("ComputeDeviceItem DataContext be of type DeviceData");
        }

        private async void StartStopButton(object sender, RoutedEventArgs e)
        {
            await _deviceData?.StartStopClick();
        }
    }
}

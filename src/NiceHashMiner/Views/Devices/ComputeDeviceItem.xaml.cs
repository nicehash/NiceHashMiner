using NiceHashMiner.ViewModels.Models;
using NiceHashMiner.Views.Common;
using System;
using System.Windows;
using System.Windows.Controls;

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
            WindowUtils.Translate(this);
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

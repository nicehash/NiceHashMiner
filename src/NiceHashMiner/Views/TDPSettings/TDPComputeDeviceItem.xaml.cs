using NHM.DeviceMonitoring.TDP;
using NiceHashMiner.ViewModels.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.TDPSettings
{
    /// <summary>
    /// Interaction logic for TDPComputeDeviceItem.xaml
    /// </summary>
    public partial class TDPComputeDeviceItem : UserControl
    {
        DeviceDataTDP _deviceDataTDP;

        public TDPComputeDeviceItem()
        {
            InitializeComponent();

            DataContextChanged += TDPComputeDeviceItem_DataContextChanged;
        }

        private void TDPComputeDeviceItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DeviceDataTDP dd)
            {
                _deviceDataTDP = dd;
                return;
            }
            throw new Exception("TDPComputeDeviceItem_DataContextChanged e.NewValue must be of type DeviceDataTDP");
        }

        private void SetSimple(object sender, RoutedEventArgs e)
        {
            var strType = textBox_simple.Text.ToUpper();
            if (Enum.TryParse(strType, out TDPSimpleType type))
            {
                _deviceDataTDP.SetSimple(type);
            }
            else
            {
                // TODO
            }
        }

        private void SetPercentage(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(textBox_percentage.Text, out var value))
            {
                _deviceDataTDP.SetPercentage(value);
            }
            else
            {
                // TODO
            }
        }
    }
}

using NHMCore.Mining;
using NiceHashMiner.ViewModels.Models;
using NiceHashMiner.Views.Common;
using System.Windows;
using System.Windows.Controls;


namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    /// <summary>
    /// Interaction logic for DeviceDataCopy.xaml
    /// </summary>
    public partial class DeviceDataCopy : UserControl
    {
        private DeviceData _deviceData;

        public DeviceDataCopy()
        {
            InitializeComponent();
            DataContextChanged += DeviceDataCopy_DataContextChanged;
            WindowUtils.Translate(this);
        }

        private void DeviceDataCopy_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DeviceData dd)
            {
                _deviceData = dd;
                return;
            }
        }


        private void Copy_Device_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button devBtn && devBtn.DataContext is ComputeDevice sourceDevice)
            {
                _deviceData.CopySettingsFromAnotherDevice(sourceDevice);
                if (this.TemplatedParent is ContextMenu parentContext) parentContext.IsOpen = false;
            }
        }
    }
}

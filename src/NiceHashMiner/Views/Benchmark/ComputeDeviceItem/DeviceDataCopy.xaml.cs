using NHMCore.Mining;
using NiceHashMiner.ViewModels;
using NiceHashMiner.ViewModels.Models;
using NiceHashMiner.Views.Common;
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
using static NHMCore.Translations;


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

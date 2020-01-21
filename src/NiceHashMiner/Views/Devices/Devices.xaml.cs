using NiceHashMiner.ViewModels;
using NHMCore;
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

namespace NiceHashMiner.Views.Devices
{
    /// <summary>
    /// Interaction logic for Devices.xaml
    /// </summary>
    public partial class Devices : UserControl
    {
        private MainVM _vm;

        public Devices()
        {
            InitializeComponent();

            DataContextChanged += Dashboard_DataContextChanged;
        }

        private void Dashboard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MainVM mainVM)
            {
                _vm = mainVM;
                return;
            }
            throw new Exception("Dashboard_DataContextChanged e.NewValue must be of type MainVM");
        }

        private async void StopAllDevicesButtonClick(object sender, RoutedEventArgs e)
        {
            await _vm.StopMining();
        }

        private async void StartAllDevicesButtonClick(object sender, RoutedEventArgs e)
        {
            await _vm.StartMining();
        }

        private async void ToggleDisableEnableAllDevices(object sender, RoutedEventArgs e)
        {
            var setEnabled = !_vm.MiningState.AllDeviceEnabled;
            await ApplicationStateManager.SetDeviceEnabledState(this, ("*", setEnabled));
        }
    }
}

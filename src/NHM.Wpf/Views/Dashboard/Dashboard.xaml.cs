using NHM.Wpf.ViewModels;
using NHMCore;
using NHMCore.ApplicationState;
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

namespace NHM.Wpf.Views.Dashboard
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private MainVM _vm;

        public Dashboard()
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

        private async void ToggleButtonStartStop_Click(object sender, RoutedEventArgs e)
        {
            await ToggleButtonStartStop_ClickTask();
        }

        private async Task ToggleButtonStartStop_ClickTask()
        {
            // IF ANY MINING execute STOP
            if (MiningState.Instance.AnyDeviceRunning)
            {
                await _vm.StopMining();
            }
            else
            {
                await _vm.StartMining();
            }
        }
    }
}

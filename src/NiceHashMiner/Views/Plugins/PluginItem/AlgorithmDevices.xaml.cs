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

namespace NiceHashMiner.Views.Plugins.PluginItem
{
    /// <summary>
    /// Interaction logic for AlgorithmDevices.xaml
    /// </summary>
    public partial class AlgorithmDevices : UserControl
    {
        public AlgorithmDevices()
        {
            InitializeComponent();
        }

        private void ShowDevices_Click(object sender, RoutedEventArgs e)
        {
            if(itc_devs.Visibility == Visibility.Collapsed)
            {
                itc_devs.Visibility = Visibility.Visible;
                btn_show_devices.Content = "Hide Devices";
            }
            else
            {
                itc_devs.Visibility = Visibility.Collapsed;
                btn_show_devices.Content = "Show Devices for selected algorithm";
            }
        }
    }
}

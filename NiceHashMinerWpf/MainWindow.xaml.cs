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

namespace NiceHashMinerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var devs = new List<DeviceInfo>
            {
                new DeviceInfo(false, "CPU#1 Intel(R) Core(TM) i7-8700k CPU @ 3.70GHz", null, 10, null, "3 / 3 / 0"),
                new DeviceInfo(true, "GPU#1 EVGA GeForce GTX 1080 Ti", 64, 0, 1550, "36 / 27 / 5"),
                new DeviceInfo(true, "GPU#2 EVGA GeForce GTX 1080 Ti", 54, 0, 1150, "36 / 27 / 3"),

            };

            DevGrid.ItemsSource = devs;
        }
    }

    public class DeviceInfo
    {
        public bool Enabled { get; set; }
        public string Device { get; }
        public string Status => Enabled ? "Stopped" : "Disabled";
        public int? Temp { get; }
        public int Load { get; }
        public int? RPM { get; }
        public string AlgoDetail { get; }

        public DeviceInfo(bool enabled, string dev, int? temp, int load, int? rpm, string detail)
        {
            Enabled = enabled;
            Device = dev;
            Temp = temp;
            Load = load;
            RPM = rpm;
            AlgoDetail = detail;
        }
    }
}

using NiceHashMiner.ViewModels.Models;
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

namespace NiceHashMiner.Views.ParameterOverview
{
    /// <summary>
    /// Interaction logic for DeviceELPItem.xaml
    /// </summary>
    public partial class DeviceELPItem : UserControl
    {
        private string LastText = string.Empty;
        public DeviceELPItem()
        {
            InitializeComponent();
        }
        private void DeviceValueTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb &&
                DataContext is DeviceELPElement ee)
            {
                if (tb.Text == String.Empty) ee.OnELPValueChanged(sender, e, 0);
                ee.OnELPValueChanged(sender, e, 1);// change which will decide on add or just change
            }
        }
    }
}

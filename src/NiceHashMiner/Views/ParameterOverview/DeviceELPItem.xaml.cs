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
using NHM.Common.Enums;

namespace NiceHashMiner.Views.ParameterOverview
{
    /// <summary>
    /// Interaction logic for DeviceELPItem.xaml
    /// </summary>
    public partial class DeviceELPItem : UserControl
    {
        public DeviceELPItem()
        {
            Loaded += DeviceELPItem_Loaded;
            InitializeComponent();
        }

        private void DeviceELPItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DeviceELPElement ee)
            {
                ee.OnELPValueChanged(DeviceELPValueTB, e, ELPEventActionType.ModifyOrAdd);
            }
        }

        private void DeviceValueTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb &&
                DataContext is DeviceELPElement ee)
            {
                ee.OnELPValueChanged(sender, e, tb.Text == String.Empty ? 
                    ELPEventActionType.Delete : ELPEventActionType.ModifyOrAdd);
            }
        }
    }
}

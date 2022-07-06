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
using NHMCore.Configs.ELPDataModels;
using NHMCore.Utils;

namespace NiceHashMiner.Views.ParameterOverview
{
    /// <summary>
    /// Interaction logic for DeviceELPItem.xaml
    /// </summary>
    public partial class DeviceELPItem : UserControl
    {
        public DeviceELPItem()
        {
            InitializeComponent();
        }

        private void DeviceELPValueTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(DataContext is DeviceELPElement ee && sender is TextBox tb)
            {
                ee.ELP = tb.Text;
                ELPManager.Instance.IterateSubModelsAndConstructELPs();
            }
        }
    }
}

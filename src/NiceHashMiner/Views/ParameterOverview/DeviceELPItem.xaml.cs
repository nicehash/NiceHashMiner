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
                CheckFlagDelimBoxValidAndUpdateIfOK(sender);
                ee.ELP = tb.Text;
                ELPManager.Instance.IterateSubModelsAndConstructELPs();
            }
        }

        private void DeviceELPValueTB_LostFocus(object sender, RoutedEventArgs e)
        {
            ELPManager.Instance.UpdateMinerELPConfig();
        }

        private void CheckFlagDelimBoxValidAndUpdateIfOK(object sender)
        {
            if (sender is not TextBox tb) return;
            var text = tb.Text;
            if (DataContext is DeviceELPElement de)
            {
                if (de.HeaderType == HeaderType.Value) return;
                if (string.IsNullOrEmpty(text))
                {
                    DeviceELPValueTB.Style = Application.Current.FindResource("inputBox") as Style;
                    DeviceELPValueTB.BorderBrush = (Brush)Application.Current.FindResource("BorderColor");
                    return;
                }
                if (IsParsedTextLenTwo(text))
                {
                    DeviceELPValueTB.Style = Application.Current.FindResource("InputBoxGood") as Style;
                    DeviceELPValueTB.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
                    return;
                }
                DeviceELPValueTB.Style = Application.Current.FindResource("InputBoxBad") as Style;
                DeviceELPValueTB.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }
        private bool IsParsedTextLenTwo(string txt)
        {
            return txt.Trim().Split(' ').Length == 2;
        }
    }
}

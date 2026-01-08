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
using Windows.UI;
using NHMCore.Configs.Managers;

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
            if(DataContext is DeviceELPElement ee && sender is TextBox tb && !ee.IsHeader)
            {
                CheckValueValidAndUpdateIfOK(sender);
                ee.ELP = tb.Text;
                ELPManager.Instance.IterateSubModelsAndConstructELPs();
            }
        }
        private void HeaderFlagELPValueTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is DeviceELPElement ee && sender is TextBox tb && ee.IsHeader)
            {
                ee.FLAG = tb.Text;
                CheckValueValidAndUpdateIfOK(sender);
                ee.SafeSetELP();
                ELPManager.Instance.IterateSubModelsAndConstructELPs();
            }
        }
        private void HeaderDelimELPValueTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is DeviceELPElement ee && sender is TextBox tb && ee.IsHeader)
            {
                ee.DELIM = tb.Text;
                CheckValueValidAndUpdateIfOK(sender);
                ee.SafeSetELP();
                ELPManager.Instance.IterateSubModelsAndConstructELPs();
            }
        }
        private void TB_LostFocus(object sender, RoutedEventArgs e)
        {
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
            ELPManager.Instance.UpdateMinerELPConfig();
        }

        private void CheckValueValidAndUpdateIfOK(object sender)
        {
            if (sender is not TextBox tb) return;
            var text = tb.Text;
            if (DataContext is DeviceELPElement de)
            {
                if (IsParsedTextLenOne(text))
                {
                    tb.Style = Application.Current.FindResource("InputBoxGood") as Style;
                    tb.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
                    return;
                }
                if (string.IsNullOrEmpty(text))
                {
                    tb.Style = Application.Current.FindResource("inputBox") as Style;
                    tb.BorderBrush = (Brush)Application.Current.FindResource("BorderColor");
                    return;
                }
                tb.Style = Application.Current.FindResource("InputBoxBad") as Style;
                tb.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }
        private bool IsParsedTextLenOne(string txt)
        {
            if (txt.Length == 1 && txt == " ") return true; 
            return txt.Trim().Split(' ').Length == 1;
        }
    }
}

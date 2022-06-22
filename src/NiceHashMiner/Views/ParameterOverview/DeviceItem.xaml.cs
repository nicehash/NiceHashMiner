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
    /// Interaction logic for DeviceItem.xaml
    /// </summary>
    public partial class DeviceItem : UserControl
    {
        private string LastText = string.Empty;
        public DeviceItem()
        {
            Loaded += Form_Loaded;
            InitializeComponent();
        }
        private void Form_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void DeviceValueTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb &&
                DataContext is DeviceELPData dd)
            {
               if (tb.Text == String.Empty)
                {
                    dd.OnELPValueChanged(sender, e, LastText, string.Empty);
                    return;
                }
            }
            //TODO
            //if is last add new column 0 // do this in lostfocus
            //if value empty remove this column 1
            //if last then dont delete but clear
        }

        private void DeviceValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb &&
                DataContext is DeviceELPData dd)
            {
                if (LastText == string.Empty) dd.OnELPValueChanged(sender, e, string.Empty, tb.Text);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                LastText = tb.Text;
            }
        }
        //spremenljivka wpf tista settingscheckboxitem registrer
    }
}

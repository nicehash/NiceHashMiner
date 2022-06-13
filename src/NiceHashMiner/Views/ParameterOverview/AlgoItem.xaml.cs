using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for AlgoItem.xaml
    /// </summary>
    public partial class AlgoItem : UserControl
    {
        public AlgoItem()
        {
            InitializeComponent();
        }
        private void DropDownDevices_Button_Click(object sender, RoutedEventArgs e)
        {
            var tb = e.Source as ToggleButton;
            if (tb.IsChecked.Value)
            {
                Expand();
            }
            else
            {
                Collapse();
            }
        }
        private void Collapse()
        {
            AlgorithmsGrid.Visibility = Visibility.Collapsed;
            DevicesGridToggleButton.IsChecked = false;
            DevicesGridToggleButtonHidden.IsChecked = false;
        }

        private void Expand()
        {
            AlgorithmsGrid.Visibility = Visibility.Visible;
            DevicesGridToggleButton.IsChecked = true;
            DevicesGridToggleButtonHidden.IsChecked = true;
        }
        private void CheckDualParamBoxValid()
        {
            var args = DualParameterInput.Text.Trim().Split(' ');
            if (args.Length <= 0 || (args.Length == 1 && args[0] == string.Empty))
            {
                DualParameterInput.Style = Application.Current.FindResource("inputBox") as Style;
                DualParameterInput.BorderBrush = (Brush)Application.Current.FindResource("BorderColor");
                return;
            }
            if (args.Length % 2 == 0)
            {
                DualParameterInput.Style = Application.Current.FindResource("InputBoxGood") as Style;
                DualParameterInput.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
                return;
            }
            DualParameterInput.Style = Application.Current.FindResource("InputBoxBad") as Style;
            DualParameterInput.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
        }
        private void DualParameterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDualParamBoxValid();
        }
        private void DualParameterInput_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckDualParamBoxValid();
        }
        private void DualParameterInput_KeyUp(object sender, KeyEventArgs e)
        {
            CheckDualParamBoxValid();
        }
    }
}


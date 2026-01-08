using NHM.Common.Enums;
using NHMCore.Configs.ELPDataModels;
using NHMCore.Configs.Managers;
using NiceHashMiner.ViewModels.Models;
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
            if (e.Source is not ToggleButton tb) return;
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
        private void CheckDualParamBoxValidAndUpdateIfOK(object sender)
        {
            if (sender is not TextBox tb) return;
            var text = tb.Text;
            if (DataContext is AlgoELPData ad)
            {
                if (text == string.Empty)
                {
                    DualParameterInput.Style = Application.Current.FindResource("inputBox") as Style;
                    DualParameterInput.BorderBrush = (Brush)Application.Current.FindResource("BorderColor");
                    ad.ClearDoubleParams();
                    return;
                }
                if (ad.UpdateDoubleParams(text))
                {
                    DualParameterInput.Style = Application.Current.FindResource("InputBoxGood") as Style;
                    DualParameterInput.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
                    return;
                }
                DualParameterInput.Style = Application.Current.FindResource("InputBoxBad") as Style;
                DualParameterInput.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }
        private void UpdateSingleParams(object sender)
        {
            if (sender is not TextBox tb) return;
            var text = tb.Text;
            if (DataContext is AlgoELPData ad)
            {
                if (text == string.Empty)
                {
                    SingleParameterInput.Style = Application.Current.FindResource("inputBox") as Style;
                    SingleParameterInput.BorderBrush = (Brush)Application.Current.FindResource("BorderColor");
                    ad.ClearSingleParams();
                    return;
                }
                ad.UpdateSingleParams(text);
                SingleParameterInput.Style = Application.Current.FindResource("InputBoxGood") as Style;
                SingleParameterInput.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
        }
        private void DualParameterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDualParamBoxValidAndUpdateIfOK(sender);
        }
        private void DualParameterInput_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckDualParamBoxValidAndUpdateIfOK(sender);
            ELPManager.Instance.UpdateMinerELPConfig();
        }
        private void SingleParameterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSingleParams(sender);
        }
        private void SingleParameterInput_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSingleParams(sender);
            ELPManager.Instance.UpdateMinerELPConfig();
        }
    }
}


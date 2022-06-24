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
            Loaded += Form_Loaded;
            InitializeComponent();
        }
        private void Form_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AlgoELPData ad)
            {
                foreach(var dev in ad.Devices)
                {
                    dev.ELPValueChanged += ChangeValueColumn;
                }
            }
        }
        void ChangeValueColumn(object sender, EventArgs e, int action, DeviceELPData de, DeviceELPElement elt)
        {
            if (DataContext is not AlgoELPData ad) return;
            if (sender is not TextBox tb) return;
            if(!de.IsDeviceDataHeader)
            {
                ad.NotifyMinerForELPRescan();
                return;
            }
            if (action == 0)//remove //todo change to enums
            {
                var column = de.ELPs.IndexOf(elt);
                if (column == de.ELPs.Count - 1 && tb.Text == String.Empty) return;
                foreach (var dev in ad.Devices)
                {
                    dev.RemoveELP(column);
                }
                ad.NotifyMinerForELPRescan();
            }
            else if (action == 1 && tb.Text != String.Empty)
            {
                var column = de.ELPs.IndexOf(elt);
                if (column == de.ELPs.Count - 1)
                {
                    foreach (var dev in ad.Devices)
                    {
                        var tempELP = new DeviceELPElement();
                        if (dev.IsDeviceDataHeader) tempELP = new DeviceELPElement(false);
                        tempELP.ELPValueChanged += dev.InputChanged;
                        dev.ELPs.Add(tempELP);
                    }
                }
                de.ELPs[column].ELP = tb.Text;
                ad.NotifyMinerForELPRescan();
            }
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
            if (DataContext is AlgoELPData ad) ad.NotifyMinerForELPRescan();
        }
        private void DualParameterInput_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckDualParamBoxValidAndUpdateIfOK(sender);
            if (DataContext is AlgoELPData ad) ad.NotifyMinerForELPRescan();
        }
        private void SingleParameterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSingleParams(sender);
            if (DataContext is AlgoELPData ad) ad.NotifyMinerForELPRescan();
        }
        private void SingleParameterInput_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSingleParams(sender);
            if (DataContext is AlgoELPData ad) ad.NotifyMinerForELPRescan();
        }
    }
}


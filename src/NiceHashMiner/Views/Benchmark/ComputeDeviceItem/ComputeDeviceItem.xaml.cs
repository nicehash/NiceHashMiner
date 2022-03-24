using NHM.Common;
using NHMCore;
using NiceHashMiner.ViewModels.Models;
using NiceHashMiner.Views.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static NHMCore.Translations;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    /// <summary>
    /// Interaction logic for ComputeDeviceItem.xaml
    /// </summary>
    public partial class ComputeDeviceItem : UserControl
    {
        private DeviceData _deviceData;
        public static RoutedEventHandler ClosedHandler = null;

        public ComputeDeviceItem()
        {
            InitializeComponent();

 
            DataContextChanged += ComputeDeviceItem_DataContextChanged;
            AlgorithmsGrid.Visibility = Visibility.Collapsed;
            WindowUtils.Translate(this);
        }


        private void ComputeDeviceItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DeviceData dd)
            {
                _deviceData = dd;
                DeviceActionsButtonContext.DataContext = dd;

                return;
            }
            //throw new Exception("ComputeDeviceItem_DataContextChanged e.NewValue must be of type DeviceData");
        }


        private void Collapse()
        {
            AlgorithmsGrid.Visibility = Visibility.Collapsed;
            AlgorithmsGridToggleButton.IsChecked = false;
            AlgorithmsGridToggleButtonHidden.IsChecked = false;
        }

        private void Expand()
        {
            AlgorithmsGrid.Visibility = Visibility.Visible;
            AlgorithmsGridToggleButton.IsChecked = true;
            AlgorithmsGridToggleButtonHidden.IsChecked = true;
        }
         
        private void DropDownAlgorithms_Button_Click(object sender, RoutedEventArgs e)
        {
            var tb = e.Source as ToggleButton;
            if (EnableDisableCheckBox == tb) return; // don't trigger algo dropdown if we click disable button
            if (ToggleButtonActions == tb) return; // don't trigger algo dropdown if we click disable button
            if (tb.IsChecked.Value)
            {
                Expand();
            }
            else
            {
                Collapse();
            }
        }

        private readonly HashSet<ToggleButton> _toggleButtonsGuard = new HashSet<ToggleButton>();
        private void Action_Button_Click(object sender, RoutedEventArgs e)
        {

            if (sender is ToggleButton tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                _toggleButtonsGuard.Add(tButton);
                DeviceActionsButtonContext.IsOpen = true;
                ClosedHandler += (s, e2) =>
                {
                    _toggleButtonsGuard.Remove(tButton);
                    tButton.IsChecked = false;
                    DeviceActionsButtonContext.IsOpen = false;
                    DeviceActionsButtonContext.Closed -= ClosedHandler;
                };
                DeviceActionsButtonContext.Closed += ClosedHandler;
            }
        }



        #region Algorithm sorting
        private void SortAlgorithmButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByAlgorithm();
        }

        private void SortMinerButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByPlugin();
        }

        private void SortSpeedButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsBySpeeds();
        }

        private void SortPayingButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByPaying();
        }

        private void SortStatusButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByStatus();
        }

        private void SortEnabledButtonClick(object sender, RoutedEventArgs e)
        {
            _deviceData?.OrderAlgorithmsByEnabled();
        }
        #endregion Algorithm sorting


        private void DeviceActionsButtonContext_Loaded(object sender, RoutedEventArgs e)
        {
            if (DeviceActionsButtonContext.Template.FindName("ActionMenu", DeviceActionsButtonContext) is DeviceQuickActionMenu ActionsMenu)
            {
                var myControl = ActionsMenu.deviceActionsGrid;
                WindowUtils.Translate(myControl);
            }    
        }
    }
}

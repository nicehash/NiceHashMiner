using NHMCore;
using NiceHashMiner.ViewModels.Models;
using NiceHashMiner.Views.Common;
using System.Collections.Generic;
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
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) =>
                {
                    _toggleButtonsGuard.Remove(tButton);
                    tButton.IsChecked = false;
                    DeviceActionsButtonContext.Closed -= closedHandler;
                };
                DeviceActionsButtonContext.Closed += closedHandler;
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

        private void Button_Click_ClearAllSpeeds(object sender, RoutedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            {
                Title = Tr("Set default settings?"),
                Description = Tr("Are you sure you would like to clear all speeds for {0}?", _deviceData.Dev.FullName),
                OkText = Tr("Yes"),
                CancelText = Tr("No"),
                AnimationVisible = Visibility.Collapsed
            };
            DeviceActionsButtonContext.IsOpen = false;
            nhmConfirmDialog.OKClick += (s, e1) => { _deviceData.ClearAllSpeeds(); };
            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }

        private async void Button_Click_StopBenchmarking(object sender, RoutedEventArgs e)
        {
            DeviceActionsButtonContext.IsOpen = false;
            await ApplicationStateManager.StopSingleDevicePublic(_deviceData.Dev);
        }

        private async void Button_Click_StartBenchmarking(object sender, RoutedEventArgs e)
        {
            DeviceActionsButtonContext.IsOpen = false;
            await ApplicationStateManager.StartSingleDevicePublic(_deviceData.Dev);
        }

        private void Button_Click_EnablebenchmarkedOnly(object sender, RoutedEventArgs e)
        {
            DeviceActionsButtonContext.IsOpen = false;
            _deviceData.EnablebenchmarkedOnly();
        }

        private void DeviceActionsButtonContext_Loaded(object sender, RoutedEventArgs e)
        {
            var myControl = (Grid)DeviceActionsButtonContext.Template.FindName("deviceActionsGrid", DeviceActionsButtonContext);
            WindowUtils.Translate(myControl);
        }
    }
}

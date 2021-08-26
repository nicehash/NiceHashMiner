using NHMCore;
using NiceHashMiner.ViewModels.Models;
using NiceHashMiner.Views.Common;
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
using static NHMCore.Translations;


namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    /// <summary>
    /// Interaction logic for DeviceQuickActionMenu.xaml
    /// </summary>
    public partial class DeviceQuickActionMenu : UserControl
    {
        //private ContextMenu DeviceActionsButtonContext;
        private DeviceData _deviceData;
        private readonly HashSet<ToggleButton> _toggleButtonsGuard = new HashSet<ToggleButton>();
        public DeviceQuickActionMenu()
        {
            InitializeComponent();
            DataContextChanged += ComputeDeviceItem_DataContextChanged;
            WindowUtils.Translate(this);
        }


        private void ComputeDeviceItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DeviceData dd)
            {
                _deviceData = dd;
                DataContext = dd;
                //DeviceActionsButtonContext.DataContext = dd;

                return;
            }
            //throw new Exception("ComputeDeviceItem_DataContextChanged e.NewValue must be of type DeviceData");
        }

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


            //DeviceActionsButtonContext.IsOpen = false;
            nhmConfirmDialog.OKClick += (s, e1) => { _deviceData.ClearAllSpeeds(); };
            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }

        private async void Button_Click_StopBenchmarking(object sender, RoutedEventArgs e)
        {
            //DeviceActionsButtonContext.IsOpen = false;
            await ApplicationStateManager.StopSingleDevicePublic(_deviceData.Dev);
        }

        private async void Button_Click_StartBenchmarking(object sender, RoutedEventArgs e)
        {
            //DeviceActionsButtonContext.IsOpen = false;
            await ApplicationStateManager.StartSingleDevicePublic(_deviceData.Dev);
        }

        private void Button_Click_EnablebenchmarkedOnly(object sender, RoutedEventArgs e)
        {
            //DeviceActionsButtonContext.IsOpen = false;
            _deviceData.EnablebenchmarkedOnly();
        }

        private void Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            //var context = (ContextMenu)DeviceActionsButtonContext.Template.FindName("subContext", DeviceActionsButtonContext);
            if (sender is ToggleButton tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                //var context = DeviceActionsButtonContext.Template.FindName("subContext", DeviceActionsButtonContext) as ContextMenu;

                _toggleButtonsGuard.Add(tButton);
                subContext.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) =>
                {
                    _toggleButtonsGuard.Remove(tButton);
                    tButton.IsChecked = false;
                    subContext.Closed -= closedHandler;
                };
                subContext.Closed += closedHandler;
            }
        }

        //private void Action_Button_Click(object sender, RoutedEventArgs e)
        //{

        //    if (sender is ToggleButton tButton && !_toggleButtonsGuard.Contains(tButton))
        //    {
        //        _toggleButtonsGuard.Add(tButton);
        //        //lastPlacementTarget = DeviceActionsButtonContext.Placement;
        //        DeviceActionsButtonContext.IsOpen = true;
        //        RoutedEventHandler closedHandler = null;
        //        closedHandler += (s, e2) =>
        //        {
        //            _toggleButtonsGuard.Remove(tButton);
        //            tButton.IsChecked = false;
        //            DeviceActionsButtonContext.Closed -= closedHandler;
        //        };
        //        DeviceActionsButtonContext.Closed += closedHandler;
        //    }
        //}

        //private void DeviceActionsButtonContext_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //Logger.Info("INFO", "LOAD");

        //    //var myControl = (Grid)DeviceActionsButtonContext.Template.FindName("deviceActionsGrid", DeviceActionsButtonContext);
        //    //deviceActionsGrid
        //    WindowUtils.Translate(deviceActionsGrid);
        //}
    }
}

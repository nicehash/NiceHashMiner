using NHM.Common;
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
        private DeviceData _deviceData;
        private readonly HashSet<ToggleButton> _toggleButtonsGuard = new HashSet<ToggleButton>();
        public DeviceQuickActionMenu()
        {
            InitializeComponent();
            DataContextChanged += QuickActionMenu_DataContextChanged;
            WindowUtils.Translate(this);
        }


        private void QuickActionMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DeviceData dd)
            {
                _deviceData = dd;
                DataContext = dd;
                subContext.DataContext = dd;

                return;
            }
            //throw new Exception("ComputeDeviceItem_DataContextChanged e.NewValue must be of type DeviceData");
        }

        private void Button_Click_ClearAllSpeeds(object sender, RoutedEventArgs e)
        {
            TryCloseParentContextMenu();
            var nhmConfirmDialog = new CustomDialog()
            {
                Title = Tr("Set default settings?"),
                Description = Tr("Are you sure you would like to clear all speeds for {0}?", _deviceData.Dev.FullName),
                OkText = Tr("Yes"),
                CancelText = Tr("No"),
                AnimationVisible = Visibility.Collapsed
            };


            nhmConfirmDialog.OKClick += (s, e1) => { _deviceData.ClearAllSpeeds(); };
            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }

        private async void Button_Click_StopBenchmarking(object sender, RoutedEventArgs e)
        {
            TryCloseParentContextMenu();
            await ApplicationStateManager.StopSingleDevicePublic(_deviceData.Dev);
        }

        private async void Button_Click_StartBenchmarking(object sender, RoutedEventArgs e)
        {
            TryCloseParentContextMenu();
            await ApplicationStateManager.StartSingleDevicePublic(_deviceData.Dev);
        }

        private void Button_Click_EnablebenchmarkedOnly(object sender, RoutedEventArgs e)
        {
            TryCloseParentContextMenu();
            _deviceData.EnableBenchmarkedOnly();
        }

        private void Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                _toggleButtonsGuard.Add(tButton);
                subContext.PlacementTarget = tButton;

                subContext.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) =>
                {
                    TryCloseParentContextMenu();
                    _toggleButtonsGuard.Remove(tButton);
                    tButton.IsChecked = false;
                    subContext.Closed -= closedHandler;
                    subContext.IsOpen = false;
                };
                subContext.Closed += closedHandler;
            }
        }

        private void subContext_Loaded(object sender, RoutedEventArgs e)
        {
            if (subContext.Template.FindName("CopyMenu", subContext) is DeviceDataCopy ActionsMenu)
            {
                var myControl = ActionsMenu.DeviceSelection;
                WindowUtils.Translate(myControl);
            }
        }
        private void TryCloseParentContextMenu()
        {
            try
            {
                ComputeDeviceItem.ClosedHandler.Invoke(this, new RoutedEventArgs());
            }
            catch (Exception ex)
            {
                Logger.Error("DeviceQuickActionsMenu", $"{ex.Message}");
            }
        }
    }
}

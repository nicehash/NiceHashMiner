﻿using NHM.Common;
using NHMCore;
using NiceHashMiner.Views.Common;
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
using static NHMCore.Translations;
using System.Windows.Controls.Primitives;
using NiceHashMiner.ViewModels.Models;


namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    /// <summary>
    /// Interaction logic for ActionMenuIdle.xaml
    /// </summary>
    public partial class ActionMenuIdle : UserControl
    {

        private DeviceData _deviceData;
        private readonly HashSet<ToggleButton> _toggleButtonsGuard = new HashSet<ToggleButton>();
        public ActionMenuIdle()
        {
            InitializeComponent();
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

        private void Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            TryCloseParentContextMenu();
            if (sender is ToggleButton tButton &&
                !_toggleButtonsGuard.Contains(tButton) &&
                GetSubContextMenu(out ContextMenu subContext))
            {
                _toggleButtonsGuard.Add(tButton);
                subContext.PlacementTarget = tButton;

                subContext.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) =>
                {
                    _toggleButtonsGuard.Remove(tButton);
                    tButton.IsChecked = false;
                    subContext.Closed -= closedHandler;
                    subContext.IsOpen = false;
                };
                subContext.Closed += closedHandler;
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

        private bool GetSubContextMenu(out ContextMenu subcontextBtn)
        {
            subcontextBtn = null;
            var subContext = Application.Current.TryFindResource("subContext") as ContextMenu;
            if (subContext != null)
            {
                subcontextBtn = subContext;
                return true;
            }
            return false;
        }

        private void subContext_Loaded(object sender, RoutedEventArgs e)
        {
            if (GetSubContextMenu(out ContextMenu subContext) &&
                subContext.Template.FindName("CopyMenu", subContext) is DeviceDataCopy ActionsMenu)
            {
                var myControl = ActionsMenu.DeviceSelection;
                WindowUtils.Translate(myControl);
            }
        }


    }
}
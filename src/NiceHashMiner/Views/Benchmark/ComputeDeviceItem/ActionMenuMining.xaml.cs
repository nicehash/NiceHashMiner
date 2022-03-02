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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    /// <summary>
    /// Interaction logic for ActionMenuMining.xaml
    /// </summary>
    public partial class ActionMenuMining : UserControl
    {
        private DeviceData _deviceData;
        public ActionMenuMining()
        {
            InitializeComponent();
            DataContextChanged += QuickActionMenu_DataContextChanged;
            WindowUtils.Translate(this);
        }
        private async void Button_Click_StopBenchmarking(object sender, RoutedEventArgs e)
        {
            TryCloseParentContextMenu();
            await ApplicationStateManager.StopSingleDevicePublic(_deviceData.Dev);
        }
        private void QuickActionMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DeviceData dd)
            {
                _deviceData = dd;
                DataContext = dd;
                if (GetSubContextMenu(out ContextMenu subContext))
                {
                    subContext.DataContext = dd;
                }
                return;
            }
            //throw new Exception("ComputeDeviceItem_DataContextChanged e.NewValue must be of type DeviceData");
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

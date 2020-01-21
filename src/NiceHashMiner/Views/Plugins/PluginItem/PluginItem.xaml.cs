using NiceHashMiner.ViewModels.Plugins;
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

namespace NiceHashMiner.Views.Plugins.PluginItem
{
    /// <summary>
    /// Interaction logic for PluginItem.xaml
    /// </summary>
    public partial class PluginItem : UserControl
    {
        private PluginEntryVM _vm;
        public PluginItem()
        {
            InitializeComponent();
            Collapse();

            DataContextChanged += PluginEntry_DataContextChanged;
        }

        private void PluginEntry_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm = e.NewValue as PluginEntryVM; // ?? throw new InvalidOperationException("DataContext must be of type `PluginEntryVM`");
            PluginActionsButtonContext.DataContext = _vm;
            if (!_vm.Plugin.Supported)
            {
                mainPluginGrid.ToolTip = "Not compatible with your hardware.";
            }
        }

        private void Collapse()
        {
            DetailsGrid.Visibility = Visibility.Collapsed;
            DetailsToggleButton.IsChecked = false;
            DetailsToggleButtonText.Text = "Details";
        }

        private void Expand()
        {
            DetailsGrid.Visibility = Visibility.Visible;
            DetailsToggleButton.IsChecked = true;
            DetailsToggleButtonText.Text = "Less Details";
        }

        private void ToggleDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailsToggleButton.IsChecked.Value)
            {
                Expand();
            }
            else
            {
                Collapse();
            }
        }

        private readonly HashSet<ToggleButton> _toggleButtonsGuard = new HashSet<ToggleButton>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                _toggleButtonsGuard.Add(tButton);
                PluginActionsButtonContext.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) => {
                    _toggleButtonsGuard.Remove(tButton);
                    tButton.IsChecked = false;
                    PluginActionsButtonContext.Closed -= closedHandler;
                };
                PluginActionsButtonContext.Closed += closedHandler;
            }
            
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_vm.Load.IsInstalling) return;

            await _vm.InstallOrUpdatePlugin();
        }

        private void Button_Click_Uninstall(object sender, RoutedEventArgs e)
        {
            PluginActionsButtonContext.IsOpen = false;
            _vm.UninstallPlugin();
        }

        private void Button_Click_ShowInternals(object sender, RoutedEventArgs e)
        {
            PluginActionsButtonContext.IsOpen = false;
            _vm.ShowPluginInternals();
        }
    }
}

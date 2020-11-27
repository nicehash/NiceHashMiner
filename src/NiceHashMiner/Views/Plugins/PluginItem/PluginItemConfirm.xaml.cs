using NHMCore;
using NHMCore.Mining.Plugins;
using NiceHashMiner.ViewModels.Plugins;
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

namespace NiceHashMiner.Views.Plugins.PluginItem
{
    /// <summary>
    /// Interaction logic for PluginItem.xaml
    /// </summary>
    public partial class PluginItemConfirm : UserControl
    {
        private PluginEntryVM _vm;
        public PluginItemConfirm()
        {
            InitializeComponent();
            Collapse();

            DataContextChanged += PluginEntry_DataContextChanged;
            WindowUtils.Translate(this);
        }

        public event EventHandler<RoutedEventArgs> OnAcceptOrDecline;

        private void PluginEntry_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm = e.NewValue as PluginEntryVM; // ?? throw new InvalidOperationException("DataContext must be of type `PluginEntryVM`");
            if (!_vm?.Plugin?.Supported ?? false)
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
            DetailsToggleButtonText.Text = Translations.Tr(DetailsToggleButtonText.Text);
        }

        private async void Button_Click_Install(object sender, RoutedEventArgs e)
        {
            if (_vm.Load.IsInstalling) return;
            _vm.Plugin.IsUserActionRequired = false;
            //AcceptedPlugins.Add(_vm.Plugin.PluginUUID);
            OnAcceptOrDecline?.Invoke(sender, e);
            await _vm.ConfirmInstallOrUpdatePlugin();
        }

        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            _vm.Plugin.IsUserActionRequired = false;
            //AcceptedPlugins.Remove(_vm.Plugin.PluginUUID);
            OnAcceptOrDecline?.Invoke(sender, e);
            _vm.UninstallPlugin();
        }
    }
}

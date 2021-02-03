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
        private PluginPackageInfoCR _vm;
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
            _vm = e.NewValue as PluginPackageInfoCR; // ?? throw new InvalidOperationException("DataContext must be of type `PluginEntryVM`");
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

        private void Button_Click_Install(object sender, RoutedEventArgs e)
        {
            _vm.IsUserActionRequired = false;
            AcceptedPlugins.Add(_vm.PluginUUID);
            OnAcceptOrDecline?.Invoke(sender, e);
        }

        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            _vm.IsUserActionRequired = false;
            AcceptedPlugins.Remove(_vm.PluginUUID);
            MinerPluginsManager.RemovePlugin(_vm.PluginUUID);
            OnAcceptOrDecline?.Invoke(sender, e);
        }
    }
}

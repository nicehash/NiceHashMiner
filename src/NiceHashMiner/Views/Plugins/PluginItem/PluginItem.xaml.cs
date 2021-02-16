using NHMCore;
using NHMCore.Mining.Plugins;
using NiceHashMiner.ViewModels.Plugins;
using NiceHashMiner.Views.Common;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static NHMCore.Translations;

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
            WindowUtils.Translate(this);
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
            DetailsToggleButtonText.Text = Translations.Tr(DetailsToggleButtonText.Text);
        }

        private readonly HashSet<ToggleButton> _toggleButtonsGuard = new HashSet<ToggleButton>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                _toggleButtonsGuard.Add(tButton);
                PluginActionsButtonContext.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) =>
                {
                    _toggleButtonsGuard.Remove(tButton);
                    tButton.IsChecked = false;
                    PluginActionsButtonContext.Closed -= closedHandler;
                };
                PluginActionsButtonContext.Closed += closedHandler;
            }

        }

        private async void Button_Click_Install(object sender, RoutedEventArgs e)
        {
            if (_vm.Load.IsInstalling) return;
            if (AcceptedPlugins.IsAccepted(_vm.Plugin.PluginUUID))
            {
                await _vm.InstallOrUpdatePlugin();
            }
            else
            {
                var dialog = new CustomDialog(400, 300)
                {
                    // Translate this???
                    Title = "Disclaimer on usage of 3rd party software",
                    Description = "NiceHash Miner integrates 3rd party mining software via the miner plugin system. However, since this is 3rd party software that is fully closed-source, we have no chance to inspect it in any way. NiceHash can not vouch for using that software and is refusing to take any responsibility for any damage caused, security breaches, loss of data or funds, system or hardware error, and other issues. By agreeing to this disclaimer you take full responsibility for using these closed-source miners as they are.",
                    OkText = Tr("I ACCEPT"),
                    CancelText = Tr("CANCEL"),
                    AnimationVisible = Visibility.Collapsed
                };
                dialog.OKClick += async (s, e1) =>
                {
                    AcceptedPlugins.Add(_vm.Plugin.PluginUUID);
                    await _vm.InstallOrUpdatePlugin();
                };
                CustomDialogManager.ShowModalDialog(dialog);
            }
        }

        private async void Button_Click_Uninstall(object sender, RoutedEventArgs e)
        {
            AcceptedPlugins.Remove(_vm.Plugin.PluginUUID);
            PluginActionsButtonContext.IsOpen = false;
            await _vm.UninstallPlugin();
        }

        private void Button_Click_ShowInternals(object sender, RoutedEventArgs e)
        {
            PluginActionsButtonContext.IsOpen = false;
            _vm.ShowPluginInternals();
        }

        private void PluginAutoUpdateToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent == ToggleButton.UncheckedEvent)
            {
                _vm.Plugin.IsAutoUpdateEnabled = false;
            }
            else
            {
                _vm.Plugin.IsAutoUpdateEnabled = true;
            }
            _vm.Plugin.OnPropertyChanged(nameof(_vm.Plugin.IsAutoUpdateEnabled));
        }

        private void PluginActionsButtonContext_Loaded(object sender, RoutedEventArgs e)
        {
            var myControl = (Grid)PluginActionsButtonContext.Template.FindName("pluginActionsGrid", PluginActionsButtonContext);
            WindowUtils.Translate(myControl);
        }
    }
}

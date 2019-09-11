using System;
using System.Windows;
using System.Windows.Controls;
using NHM.Wpf.ViewModels.Models;
using NHM.Wpf.ViewModels.Plugins;

namespace NHM.Wpf.Views.Plugins.Controls
{
    /// <summary>
    /// Interaction logic for PluginEntry.xaml
    /// </summary>
    public partial class PluginEntry : UserControl
    {
        public event EventHandler<PluginEventArgs> InstallClick;
        public event EventHandler<PluginEventArgs> DetailsClick;

        private PluginEntryVM _vm;

        public PluginEntry()
        {
            InitializeComponent();

            DataContextChanged += PluginEntry_DataContextChanged;
        }

        private void PluginEntry_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm = e.NewValue as PluginEntryVM; // ?? throw new InvalidOperationException("DataContext must be of type `PluginEntryVM`");
        }

        private async void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            InstallClick?.Invoke(this, new PluginEventArgs(_vm));
            await _vm.InstallRemovePlugin();
        }

        private void DetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            DetailsClick?.Invoke(this, new PluginEventArgs(_vm));
        }
    }
}

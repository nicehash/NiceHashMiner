using NHM.Wpf.ViewModels.Models;
using NHM.Wpf.ViewModels.Models.Placeholders;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NHM.Wpf.Windows.Plugins
{
    /// <summary>
    /// Interaction logic for PluginEntry.xaml
    /// </summary>
    public partial class PluginEntry : UserControl
    {
        public event EventHandler<PluginEventArgs> InstallClick;
        public event EventHandler<PluginEventArgs> DetailsClick;

        public static readonly DependencyProperty PluginProperty = DependencyProperty.Register(
            nameof(Plugin),
            typeof(PluginPackageInfoCR),
            typeof(PluginEntry),
            new PropertyMetadata());

        public PluginPackageInfoCR Plugin
        {
            get => (PluginPackageInfoCR) GetValue(PluginProperty);
            set => SetValue(PluginProperty, value);
        }

        public PluginEntry()
        {
            InitializeComponent();
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            InstallClick?.Invoke(this, new PluginEventArgs(Plugin));
        }

        private void DetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            DetailsClick?.Invoke(this, new PluginEventArgs(Plugin));
        }
    }
}

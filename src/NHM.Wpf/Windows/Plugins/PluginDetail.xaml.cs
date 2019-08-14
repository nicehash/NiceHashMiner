using NHM.Wpf.ViewModels.Plugins;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NHM.Wpf.Windows.Plugins
{
    /// <summary>
    /// Interaction logic for PluginDetail.xaml
    /// </summary>
    public partial class PluginDetail : UserControl
    {
        public event EventHandler<RoutedEventArgs> BackClick;

        public PluginDetail()
        {
            InitializeComponent();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            BackClick?.Invoke(this, e);
        }

        private async void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is PluginDetailVM vm)
                await vm.InstallRemovePlugin();
        }
    }
}

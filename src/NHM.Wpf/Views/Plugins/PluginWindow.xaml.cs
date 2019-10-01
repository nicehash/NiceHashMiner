using System.Windows;
using NHM.Wpf.ViewModels.Models;
using NHM.Wpf.ViewModels.Plugins;
using NHM.Wpf.Views.Common;

namespace NHM.Wpf.Views.Plugins
{
    /// <summary>
    /// Interaction logic for PluginWindow.xaml
    /// </summary>
    public partial class PluginWindow : Window
    {
        private readonly PluginVM _vm;

        public PluginWindow()
        {
            InitializeComponent();

            _vm = this.AssertViewModel<PluginVM>();
            WindowUtils.InitWindow(this);
        }

        private void PluginEntry_OnDetailsClick(object sender, PluginEventArgs e)
        {
            _vm.SetDetails(e.ViewModel);
        }

        private void PluginDetail_OnBackClick(object sender, RoutedEventArgs e)
        {
            _vm.SetToList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }
    }
}

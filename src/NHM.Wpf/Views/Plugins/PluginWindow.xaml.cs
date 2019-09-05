using System.Windows;
using NHM.Wpf.ViewModels.Models;
using NHM.Wpf.ViewModels.Plugins;

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

            if (DataContext is PluginVM vm)
                _vm = vm;
            else
            {
                _vm = new PluginVM();
                DataContext = _vm;
            }
        }

        private void PluginEntry_OnDetailsClick(object sender, PluginEventArgs e)
        {
            _vm.SetDetails(e.ViewModel);
        }

        private void PluginDetail_OnBackClick(object sender, RoutedEventArgs e)
        {
            _vm.SetToList();
        }
    }
}

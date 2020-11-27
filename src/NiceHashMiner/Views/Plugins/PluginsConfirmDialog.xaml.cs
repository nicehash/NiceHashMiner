using NiceHashMiner.ViewModels;
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

namespace NiceHashMiner.Views.Plugins
{
    /// <summary>
    /// Interaction logic for PluginsConfirmDialog.xaml
    /// </summary>
    public partial class PluginsConfirmDialog : UserControl
    {
        int _pluginsToAccept = 0;
        public PluginsConfirmDialog()
        {
            InitializeComponent();

            this.DataContextChanged += PluginsConfirmDialog_DataContextChanged;
        }

        private void PluginsConfirmDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as MainVM;
            _pluginsToAccept = vm?.Plugins?.Where(p => p.Plugin.IsUserActionRequired).Count() ?? 0;
            if (_pluginsToAccept == 0) CustomDialogManager.HideCurrentModal();
        }

        private void OnAcceptOrDecline(object sender, RoutedEventArgs e)
        {
            _pluginsToAccept--;
            if (_pluginsToAccept == 0) CustomDialogManager.HideCurrentModal();
        }


    }
}

using System;
using System.Windows.Controls;
using NHM.Wpf.ViewModels.Models;

namespace NHM.Wpf.Views.Plugins
{
    /// <summary>
    /// Interaction logic for PluginList.xaml
    /// </summary>
    public partial class PluginList : UserControl
    {
        public event EventHandler<PluginEventArgs> DetailsClick;

        public PluginList()
        {
            InitializeComponent();
        }

        private void PluginEntry_OnDetailsClick(object sender, PluginEventArgs e)
        {
            DetailsClick?.Invoke(sender, e);
        }
    }
}

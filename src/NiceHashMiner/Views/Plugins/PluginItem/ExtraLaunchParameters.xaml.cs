using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NHMCore.Mining.Plugins;
using NiceHashMiner.ViewModels.Plugins;
using System.Linq;
using NiceHashMiner.ViewModels;
using System.Collections.Generic;
using NHMCore.Mining;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Windows.Media;

namespace NiceHashMiner.Views.Plugins.PluginItem
{
    /// <summary>
    /// Interaction logic for ExtraLaunchParameters.xaml
    /// </summary>
    public partial class ExtraLaunchParameters : UserControl
    {
        public ExtraLaunchParameters()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => {
                if (e.NewValue is PluginEntryVM pluginVM)
                {
                    DataContext = pluginVM;
                    return;
                }
                throw new Exception("unsupported datacontext type");
            };
        }

        private void ShowDevices_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(button.Content.ToString() == "Show Devices for selected algorithm")
            {
                button.Content = "Hide devices";
            }
            else
            {
                button.Content = "Show Devices for selected algorithm";
            }
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            CustomDialogManager.HideCurrentModal();
        }
    }
}

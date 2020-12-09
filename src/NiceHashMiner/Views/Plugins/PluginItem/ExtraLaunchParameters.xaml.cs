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
        private PluginEntryVM _pluginVM;
        public ExtraLaunchParameters()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => {
                if (e.NewValue is PluginEntryVM pluginVM)
                {
                    _pluginVM = pluginVM;
                    DataContext = _pluginVM;
                    return;
                }
                throw new Exception("unsupported datacontext type");
            };
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //var elpTextBox = (sender as TextBox);
            //var tag = elpTextBox.Tag.ToString();
            //if(tag == "Plugin")
            //{
            //    foreach (var container in AlgorithmsContainers)
            //    {
            //        container.ExtraLaunchParameters = tbx_plugin_default.Text;
            //    }
            //}
            //else if (tag.Contains('#'))
            //{
            //    var container = AlgorithmsContainers.Where(ac => ac.ComputeDevice.FullName == tag).FirstOrDefault();
            //    container.ExtraLaunchParameters = elpTextBox.Text;
            //}
            //else
            //{
            //    foreach (var container in AlgorithmsContainers.Where(ac => ac.AlgorithmName == tag))
            //    {
            //        container.ExtraLaunchParameters = elpTextBox.Text;
            //    }
            //}
        }

        private void ShowDevices_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(button.Content.ToString() == "Show Devices for selected algorithm")
            {
                button.Content = "Hide devices";
                //lbx_devices.ItemsSource = AlgorithmsContainers.Where(ac => ac.AlgorithmName == button.Tag.ToString());
                //sp_devices.Visibility = Visibility.Visible;
            }
            else
            {
                button.Content = "Show Devices for selected algorithm";
                //sp_devices.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            CustomDialogManager.HideCurrentModal();
        }
    }
}

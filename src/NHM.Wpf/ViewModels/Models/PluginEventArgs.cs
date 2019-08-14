using NHM.Wpf.ViewModels.Plugins;
using System;

namespace NHM.Wpf.ViewModels.Models
{
    public class PluginEventArgs : EventArgs
    {
        public PluginEntryVM ViewModel { get; }

        public PluginEventArgs(PluginEntryVM vm)
        {
            ViewModel = vm;
        }
    }
}

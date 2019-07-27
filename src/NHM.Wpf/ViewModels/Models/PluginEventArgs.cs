using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Wpf.ViewModels.Models.Placeholders;
using NHM.Wpf.ViewModels.Plugins;

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

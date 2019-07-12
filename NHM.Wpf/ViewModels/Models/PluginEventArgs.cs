using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Wpf.ViewModels.Models.Placeholders;

namespace NHM.Wpf.ViewModels.Models
{
    public class PluginEventArgs : EventArgs
    {
        public PluginPackageInfoCR Plugin { get; }

        public PluginEventArgs(PluginPackageInfoCR plugin)
        {
            Plugin = plugin;
        }
    }
}

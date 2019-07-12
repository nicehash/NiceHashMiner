using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Wpf.ViewModels.Models.Placeholders;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginEntryVM : BaseVM
    {
        public PluginPackageInfoCR Plugin { get; }

        public PluginEntryVM(PluginPackageInfoCR plugin)
        {
            Plugin = plugin;
        }

        public async Task InstallPlugin()
        {
            // TODO
        }
    }
}

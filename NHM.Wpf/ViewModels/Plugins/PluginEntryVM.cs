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

        public bool IsSupported => Plugin.SupportedDevicesAlgorithms.Keys.Contains("NVIDIA");
        public string InstallString => IsSupported ? "Install" : "Not Supported";

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

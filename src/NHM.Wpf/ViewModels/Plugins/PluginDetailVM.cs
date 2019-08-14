using NHM.Wpf.ViewModels.Models;
using NiceHashMiner.Mining.Plugins;
using System.Linq;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginDetailVM : PluginEntryVM, IPluginPageVM
    {
        public string SupportedDeviceString
        {
            get
            {
                return string.Join(", ", Plugin.SupportedDevicesAlgorithms.Keys);
            }
        }

        public string SupportedAlgorithmString
        {
            get
            {
                return string.Join("\n", Plugin.SupportedDevicesAlgorithms
                    .Select(kvp =>
                    {
                        var algosString = string.Join("\n", kvp.Value.Select(a => $"    - {a}"));
                        return $"{kvp.Key}:\n{algosString}";
                    }));
            }
        }

        public PluginDetailVM(PluginPackageInfoCR plugin, LoadProgress instProg)
            : base(plugin, instProg)
        { }

        public PluginDetailVM(PluginEntryVM vm)
            : this(vm.Plugin, vm.Load)
        { }
    }
}

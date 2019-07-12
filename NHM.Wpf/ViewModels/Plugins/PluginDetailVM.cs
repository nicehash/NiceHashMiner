using System.Linq;
using NHM.Wpf.ViewModels.Models.Placeholders;

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

        public PluginDetailVM(PluginPackageInfoCR plugin) 
            : base(plugin)
        { }
    }
}

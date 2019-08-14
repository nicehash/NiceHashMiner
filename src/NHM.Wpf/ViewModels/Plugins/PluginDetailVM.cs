using System.Linq;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginDetailVM : PluginEntryVM, IPluginPageVM
    {
        public string SupportedDeviceString
        {
            get
            {
                return string.Join(", ", FilteredSupportedAlgorithms.Keys);
            }
        }

        public string SupportedAlgorithmString
        {
            get
            {
                return string.Join("\n", FilteredSupportedAlgorithms
                    .Select(kvp =>
                    {
                        var algosString = string.Join("\n", kvp.Value.Select(a => $"    - {a}"));
                        return $"{kvp.Key}:\n{algosString}";
                    }));
            }
        }

        public PluginDetailVM(PluginEntryVM vm)
            : base(vm.Plugin, vm.Load)
        { }
    }
}

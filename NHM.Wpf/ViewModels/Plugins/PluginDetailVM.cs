using NHM.Wpf.ViewModels.Models.Placeholders;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginDetailVM : PluginEntryVM, IPluginPageVM
    {
        public PluginDetailVM(PluginPackageInfoCR plugin) 
            : base(plugin)
        { }
    }
}

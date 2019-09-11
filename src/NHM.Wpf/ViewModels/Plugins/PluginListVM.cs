using NHMCore.Mining.Plugins;
using System.Collections.ObjectModel;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginListVM : IPluginPageVM
    {
        public ObservableCollection<PluginEntryVM> Plugins { get; }

        public PluginListVM()
        {
            Plugins = new ObservableCollection<PluginEntryVM>();
        }

        public void PopulatePlugins()
        {
            foreach (var plugin in MinerPluginsManager.RankedPlugins)
            {
                Plugins.Add(new PluginEntryVM(plugin));
            }
        }
    }
}

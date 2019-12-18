using NHM.Common;
using NHMCore.Mining.Plugins;
using System.Collections.Generic;

namespace NHMCore.ApplicationState
{
    public class MinerPluginsManagerState : NotifyChangedBase
    {
        public static MinerPluginsManagerState Instance { get; } = new MinerPluginsManagerState();

        private MinerPluginsManagerState() { }



        private List<PluginPackageInfoCR> _rankedPlugins = null;
        public List<PluginPackageInfoCR> RankedPlugins
        {
            get => _rankedPlugins;
            set
            {
                _rankedPlugins = value;
                OnPropertyChanged();
            }
        }
    }
}

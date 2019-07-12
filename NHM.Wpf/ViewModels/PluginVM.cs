using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Wpf.ViewModels.Models.Placeholders;

namespace NHM.Wpf.ViewModels
{
    public class PluginVM : BaseVM
    {
        public ObservableCollection<PluginPackageInfoCR> Plugins { get; }

        public PluginVM()
        {
            var plugins = new List<PluginPackageInfo>
            {
                new PluginPackageInfo { PluginName = "CryptoDredge", PluginVersion = new Version("1.5"), PluginAuthor = "info@nicehash.com" },
                new PluginPackageInfo { PluginName = "ZEnemy", PluginVersion = new Version("1.4"), PluginAuthor = "info@nicehash.com" },
                new PluginPackageInfo { PluginName = "WildRig", PluginVersion = new Version("1.2"), PluginAuthor = "info@nicehash.com" }
            };

            Plugins = new ObservableCollection<PluginPackageInfoCR>(
                plugins.Select(p => new PluginPackageInfoCR { LocalInfo = p }));
        }

        public async Task InstallPluginAsync(PluginPackageInfoCR plugin)
        {
            // TODO
        }

        public void SetDetails(PluginPackageInfoCR plugin)
        {

        }
    }
}

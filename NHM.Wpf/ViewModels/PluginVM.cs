using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels
{
    public class PluginVM : BaseVM
    {
        public class FakePlugin
        {
            public string Name { get; set; }
            public Version Version { get; set; }
            public string Author { get; set; }
        }

        public ObservableCollection<FakePlugin> Plugins { get; }

        public PluginVM()
        {
            Plugins = new ObservableCollection<FakePlugin>
            {
                new FakePlugin { Name = "CryptoDredge", Version = new Version("1.5"), Author = "info@nicehash.com" },
                new FakePlugin { Name = "ZEnemy", Version = new Version("1.4"), Author = "info@nicehash.com" },
                new FakePlugin { Name = "WildRig", Version = new Version("1.2"), Author = "info@nicehash.com" }
            };
        }
    }
}

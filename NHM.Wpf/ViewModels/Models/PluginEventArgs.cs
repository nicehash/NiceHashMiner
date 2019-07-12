using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels.Models
{
    public class PluginEventArgs : EventArgs
    {
        public PluginVM.FakePlugin Plugin { get; }

        public PluginEventArgs(PluginVM.FakePlugin plugin)
        {
            Plugin = plugin;
        }
    }
}

using NHMCore.Mining;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Plugins
{
    public class PluginAlgorithmEntry : BaseVM
    {
        public List<AlgorithmContainer> Devices { get; private set; }
        public string Name { get; private set; } = "Not available";

        public PluginAlgorithmEntry(List<AlgorithmContainer> devices)
        {
            Name = devices?.FirstOrDefault()?.AlgorithmName ?? "Not available";
            Devices = devices;
        }
    }
}

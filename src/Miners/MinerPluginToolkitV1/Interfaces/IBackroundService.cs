using MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Interfaces
{
    public interface IBackroundService
    {
        bool ServiceEnabled { get; set; }
        void Start(IEnumerable<MiningPair> miningPairs);
        void Stop(IEnumerable<MiningPair> miningPairs = null);
    }
}

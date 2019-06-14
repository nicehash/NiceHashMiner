using MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMiner
{
    public static class BrokenMinerPluginFactory
    {
        public static BrokenMinerPlugin Create()
        {
            return new BrokenMinerPlugin();
        }
    }
}

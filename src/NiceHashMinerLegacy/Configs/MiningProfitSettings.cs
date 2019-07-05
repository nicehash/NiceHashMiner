using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Configs
{
    public class MiningProfitSettings
    {
        public static MiningProfitSettings Instance { get; } = new MiningProfitSettings();

        private MiningProfitSettings()
        { }

        public double MinimumProfit { get; set; } = 0;
        public bool MineRegardlessOfProfit { get; set; } = true;

        public bool IsMinimumProfitProfitEnabled => !MineRegardlessOfProfit;
    }
}

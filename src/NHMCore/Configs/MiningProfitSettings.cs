
namespace NHMCore.Configs
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

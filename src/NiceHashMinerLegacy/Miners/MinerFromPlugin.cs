using MinerPlugin;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NiceHashMinerLegacy.Common.Device;
using CommonAlgorithm = NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMiner.Plugin;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Miners
{
    // pretty much just implement what we need and ignore everything else
    public class MinerFromPlugin : Miner
    {
        private readonly IMiner _miner;
        public MinerFromPlugin(string pluginUUID) : base(pluginUUID)
        {
            var plugin = MinerPluginsManager.GetPluginWithUuid(pluginUUID);
            _miner = plugin.CreateMiner();
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            IsUpdatingApi = true;
            var apiData = await _miner.GetMinerStatsDataAsync();
            IsUpdatingApi = false;

            if (apiData.AlgorithmSpeedsTotal.Count == 1)
            {
                var algoSpeed = apiData.AlgorithmSpeedsTotal.First();
                var ret = new ApiData(algoSpeed.AlgorithmType);
                ret.Speed = algoSpeed.Speed;
                ret.PowerUsage = apiData.PowerUsageTotal;
                return ret;
            } 

            return null;
        }

        // TODO this thing 
        public override void Start(string url, string btcAdress, string worker)
        {
            // TODO global state right here
            var location = Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation];
            var username = $"{btcAdress}.{worker}";
            _miner.InitMiningLocationAndUsername(location, username);

            var pluginPairs = this.MiningSetup.MiningPairs
                .Where(pair => pair.Algorithm is PluginAlgorithm)
                .Select(pair => new MinerPlugin.MiningPair
                {
                    Device = pair.Device.PluginDevice,
                    Algorithm = ((PluginAlgorithm) pair.Algorithm).BaseAlgo
                });
            _miner.InitMiningPairs(pluginPairs);

            _miner.StartMining();
            IsRunning = true;
        }

        // we are setting 5 minutes on the others so lets keep it this way
        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 min
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            IsRunning = false;
            _miner.StopMining();
        }

        #region DEAD_CODE_NEVER_CALL_THIS
        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            throw new NotImplementedException();
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            throw new NotImplementedException();
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            throw new NotImplementedException();
        }
        #endregion DEAD_CODE_NEVER_CALL_THIS
    }
}

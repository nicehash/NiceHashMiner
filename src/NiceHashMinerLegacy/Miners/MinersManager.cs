using NiceHashMiner.Devices;
using NiceHashMiner.Miners.IntegratedPlugins;
using NiceHashMinerLegacy.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners
{
    public static class MinersManager
    {
        private readonly static MiningSession _curMiningSession = new MiningSession(DemoUser.BTC);

        // TODO remove deviant checkers Forms code from Mining sessiong and get rid of the headless
        public static void StopAllMiners(bool headless = true)
        {
            _curMiningSession.StopAllMiners(headless);
            EthlargementIntegratedPlugin.Instance.Stop();
        }

        public static List<int> GetActiveMinersIndexes()
        {
            return _curMiningSession.ActiveDeviceIndexes;
        }

        public static void UpdateMiningSession(IEnumerable<ComputeDevice> devices, string username)
        {
            _curMiningSession.UpdateMiningSession(devices, username);
        }

        public static void RestartMiners()
        {
            _curMiningSession.RestartMiners();
        }

        public static async Task MinerStatsCheck()
        {
            await _curMiningSession.MinerStatsCheck();
        }
    }
}

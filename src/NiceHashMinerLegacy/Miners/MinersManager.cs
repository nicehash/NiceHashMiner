using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners
{
    public static class MinersManager
    {
        // old system without SerialTaskQueue
#if false
        private readonly static MiningSession _curMiningSession = new MiningSession(DemoUser.BTC);

        // TODO remove deviant checkers Forms code from Mining sessiong and get rid of the headless
        public static void StopAllMiners(bool headless = true)
        {
            _curMiningSession.StopAllMiners(headless);
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
#else
        // TODO remove deviant checkers Forms code from Mining sessiong and get rid of the headless
        public static void StopAllMiners(bool headless = true)
        {
            MiningManager.StopAllMiners();
        }

        public static List<int> GetActiveMinersIndexes()
        {
            return MiningManager.GetActiveMinersIndexes();
        }

        public static void UpdateMiningSession(IEnumerable<ComputeDevice> devices, string username)
        {
            MiningManager.UpdateMiningSession(devices, username);
        }

        public static void RestartMiners()
        {
            MiningManager.RestartMiners();
        }

        public static async Task MinerStatsCheck()
        {
            await MiningManager.MinerStatsCheck();
        }
    }
#endif
}

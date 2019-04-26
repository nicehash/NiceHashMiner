// TESTNET
#if TESTNET || TESTNETDEV
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;

namespace NiceHashMiner.Miners
{
    public static class MinersManager
    {
        private static MiningSession _curMiningSession;

        public static void StopAllMiners(bool headless)
        {
            _curMiningSession?.StopAllMiners(headless);
            EthlargementOld.Stop();
            _curMiningSession = null; // TODO consider not nulling a mining session
        }

        public static void EnsureMiningSession(string username)
        {
            if (_curMiningSession == null)
            {
                _curMiningSession = new MiningSession(new List<ComputeDevice>(), username);
            }
        }

        public static void UpdateUsedDevices(IEnumerable<ComputeDevice> devices)
        {
            _curMiningSession?.UpdateUsedDevices(devices);
        }

        public static void RestartMiners()
        {
            _curMiningSession?.RestartMiners();
        }

        public static async Task MinerStatsCheck()
        {
            if (_curMiningSession != null) await _curMiningSession.MinerStatsCheck();
        }
    }
}
#endif

using NiceHashMiner.Devices;
using NiceHashMiner.Miners.IntegratedPlugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners
{
    public static class MinersManager
    {
        private static MiningSession _curMiningSession;

        // TODO remove deviant checkers Forms code from Mining sessiong and get rid of the headless
        public static void StopAllMiners(bool headless = true)
        {
            _curMiningSession?.StopAllMiners(headless);
            EthlargementIntegratedPlugin.Instance.Stop();
            _curMiningSession = null; // TODO consider not nulling a mining session
        }

        public static List<int> GetActiveMinersIndexes()
        {
            return _curMiningSession != null ? _curMiningSession.ActiveDeviceIndexes : new List<int>();
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

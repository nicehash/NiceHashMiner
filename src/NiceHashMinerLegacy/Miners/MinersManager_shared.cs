using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
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
            // PRODUCTION
#if !(TESTNET || TESTNETDEV)
            _curMiningSession?.StopAllMiners();
#endif
            // TESTNET
#if TESTNET || TESTNETDEV
            _curMiningSession?.StopAllMiners(headless);
#endif
            EthlargementOld.Stop();
            _curMiningSession = null; // TODO consider not nulling a mining session
        }

        public static List<int> GetActiveMinersIndexes()
        {
            return _curMiningSession != null ? _curMiningSession.ActiveDeviceIndexes : new List<int>();
        }

        // PRODUCTION
#if !(TESTNET || TESTNETDEV)
        public static bool StartInitialize(IMainFormRatesComunication mainFormRatesComunication,
            string miningLocation, string username)
        {
            _curMiningSession = new MiningSession(AvailableDevices.Devices,
                mainFormRatesComunication, miningLocation, username);

            return _curMiningSession.IsMiningEnabled;
        }
#endif

        // TESTNET
#if TESTNET || TESTNETDEV
        public static void EnsureMiningSession(string username)
        {
            if (_curMiningSession == null)
            {
                _curMiningSession = new MiningSession(new List<ComputeDevice>(), username);
            }
        }
#endif

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

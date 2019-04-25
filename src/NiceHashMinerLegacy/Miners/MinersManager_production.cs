// PRODUCTION
#if !(TESTNET || TESTNETDEV)
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners
{
    public static class MinersManager
    {
        private static MiningSession _curMiningSession;

        public static void StopAllMiners()
        {
            _curMiningSession?.StopAllMiners();
            EthlargementOld.Stop();
            _curMiningSession = null;
        }

        public static List<int> GetActiveMinersIndexes()
        {
            return _curMiningSession != null ? _curMiningSession.ActiveDeviceIndexes : new List<int>();
        }

        public static bool StartInitialize(IMainFormRatesComunication mainFormRatesComunication,
            string miningLocation, string worker, string btcAdress)
        {
            _curMiningSession = new MiningSession(AvailableDevices.Devices,
                mainFormRatesComunication, miningLocation, worker, btcAdress);

            return _curMiningSession.IsMiningEnabled;
        }

        public static async Task MinerStatsCheck()
        {
            if (_curMiningSession != null) await _curMiningSession.MinerStatsCheck();
        }
    }
}
#endif

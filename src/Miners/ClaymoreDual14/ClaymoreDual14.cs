using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1.ClaymoreCommon;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClaymoreDual14
{
    public class ClaymoreDual14 : ClaymoreBase, IAfterStartMining
    {
        public ClaymoreDual14(string uuid, Dictionary<string, int> mappedIDs) : base(uuid, mappedIDs)
        {
            _started = DateTime.UtcNow;
        }


        // figure out how to fix API workaround without this started time
        private DateTime _started;

        public void AfterStartMining()
        {
            _started = DateTime.UtcNow;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            var elapsedSeconds = DateTime.UtcNow.Subtract(_started).Seconds;
            if (elapsedSeconds < 15)
            {
                return api;
            }

            var miningDevices = _miningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = IsDual() ? new AlgorithmType[] { _algorithmType, _algorithmSecondType } : new AlgorithmType[] { _algorithmType };
            // multiply dagger API data 
            var ad = await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, DevFee, DualDevFee, algorithmTypes);
            if (ad.AlgorithmSpeedsPerDevice != null)
            {
                // speed is in khs
                ad.AlgorithmSpeedsPerDevice = ad.AlgorithmSpeedsPerDevice.Select(pair => new KeyValuePair<string, IReadOnlyList<(AlgorithmType type, double speed)>>(pair.Key, pair.Value.Select((ts) => (ts.type, ts.speed * 1000)).ToList())).ToDictionary(x => x.Key, x => x.Value);
            }
            return ad;
        }
    }
}

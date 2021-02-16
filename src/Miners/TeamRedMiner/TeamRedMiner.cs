using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NHM.Common.StratumServiceHelpers;

namespace TeamRedMiner
{
    public class TeamRedMiner : MinerBase
    {
        private int _apiPort;

        // the order of intializing devices is the order how the API responds
        private Dictionary<int, string> _initOrderMirrorApiOrderUUIDs = new Dictionary<int, string>();
        private int _openClAmdPlatformNum;
        // command line parts
        private string _devices;


        public TeamRedMiner(string uuid) : base(uuid)
        {
        }

        private string AlgoName => PluginSupportedAlgorithms.AlgorithmName(_algorithmType);

        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = $"-a {AlgoName} -o {url} -u {username} --platform={_openClAmdPlatformNum} --opencl_order -d {_devices} --api_listen=127.0.0.1:{_apiPort} {_extraLaunchParameters}";
            return cmd;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var (apiDevsResult, response) = await APIHelpers.GetApiDevsRootAsync(_apiPort, _logGroup);
            var ad = new ApiData();
            ad.ApiResponse = response;
            if (apiDevsResult == null) return ad;

            try
            {
                var deviveStats = apiDevsResult.DEVS;
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                // the devices have ordered ids by -d parameter, so -d 4,2 => 4=0;2=1
                foreach (var kvp in _initOrderMirrorApiOrderUUIDs)
                {
                    var gpuID = kvp.Key;
                    var gpuUUID = kvp.Value;

                    var deviceStats = deviveStats
                        .Where(devStat => gpuID == devStat.GPU)
                        .FirstOrDefault();
                    if (deviceStats == null) continue;

                    var speedHS = deviceStats.KHS_av * 1000;
                    totalSpeed += speedHS;
                    perDeviceSpeedInfo.Add(gpuUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, speedHS * (1 - DevFee * 0.01)) });
                    // check PowerUsage API
                }
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }

        protected override void Init()
        {
            // Order pairs and parse ELP
            var miningPairsList = _miningPairs.ToList();
            _devices = string.Join(",", miningPairsList.Select(p => p.Device.ID));

            var openClAmdPlatformResult = MinerToolkit.GetOpenCLPlatformID(_miningPairs);
            _openClAmdPlatformNum = openClAmdPlatformResult.Item1;
            bool openClAmdPlatformNumUnique = openClAmdPlatformResult.Item2;
            if (!openClAmdPlatformNumUnique)
            {
                Logger.Error(_logGroup, "Initialization of miner failed. Multiple OpenCLPlatform IDs found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }

            for (int i = 0; i < miningPairsList.Count; i++)
            {
                _initOrderMirrorApiOrderUUIDs[i] = miningPairsList[i].Device.UUID;
            }
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }
    }
}

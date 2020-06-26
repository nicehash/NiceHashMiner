using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace MiniZ
{
    public class MiniZ : MinerBase, IAfterStartMining
    {
        private const double DevFee = 2.0;
        private int _apiPort;
        private string _devices;
        private DateTime _started;

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public MiniZ(string uuid, Dictionary<string, int> mappedDeviceIds) : base(uuid)
        {
            _mappedDeviceIds = mappedDeviceIds;
        }
        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        public void AfterStartMining()
        {
            _started = DateTime.UtcNow;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            var elapsedSeconds = DateTime.UtcNow.Subtract(_started).Seconds;
            if (elapsedSeconds < 10)
            {
                return api;
            }

            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            var perDevicePowerInfo = new Dictionary<string, int>();
            var totalSpeed = 0d;
            var totalPowerUsage = 0;

            try
            {
                JsonApiResponse resp = null;
                using (var client = new TcpClient("127.0.0.1", _apiPort))
                using (var nwStream = client.GetStream())
                {
                    var bytesToSend = Encoding.ASCII.GetBytes("{\"id\":\"0\", \"method\":\"getstat\"}\\n");
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    api.ApiResponse = respStr;
                    resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr);
                    client.Close();
                }

                // return if we got nothing
                var respOK = resp != null && resp.error == null;
                if (respOK == false) return api;

                var results = resp.result;

                var gpus = _miningPairs.Select(pair => pair.Device).Cast<CUDADevice>();

                foreach (var gpu in gpus)
                {
                    var currentDevStats = results.Where(r => int.Parse(r.busid.Split(':')[1], System.Globalization.NumberStyles.HexNumber) == gpu.PCIeBusID).FirstOrDefault();
                    if (currentDevStats == null) continue;
                    totalSpeed += currentDevStats.speed_sps;
                    perDeviceSpeedInfo.Add(gpu.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentDevStats.speed_sps * (1 - DevFee * 0.01)) });
                    totalPowerUsage += (int)currentDevStats.gpu_power_usage * 1000; //reported in W
                    perDevicePowerInfo.Add(gpu.UUID, (int)currentDevStats.gpu_power_usage * 1000);
                }
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            api.PowerUsageTotal = totalPowerUsage;
            api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            api.PowerUsagePerDevice = perDevicePowerInfo;
            return api;
        }

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by _mappedDeviceIds
            pairsList.Sort((a, b) => _mappedDeviceIds[a.Device.UUID].CompareTo(_mappedDeviceIds[b.Device.UUID]));
            return pairsList;
        }

        protected override void Init()
        {
            var mappedDevIDs = _miningPairs.Select(p => _mappedDeviceIds[p.Device.UUID]);
            _devices = string.Join(",", mappedDevIDs);
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();

            var algo = AlgorithmName(_algorithmType);

            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var split = urlWithPort.Split(':');
            var url = split[0];
            var port = split[1];

            var cmd = $"--par={algo} --server={url} --port={port} --user={username} --cuda-devices={_devices} --telemetry={_apiPort} {_extraLaunchParameters}";

            if (_algorithmType == AlgorithmType.ZHash)
            {
                cmd += " --pers=auto";
            }

            return cmd;
        }
    }
}

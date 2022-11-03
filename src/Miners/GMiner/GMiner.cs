using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MP.GMiner
{
    public class GMiner : MinerBase, IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();
        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public GMiner(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedDeviceIds = mappedIDs;
        }

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by mapped ids
            pairsList.Sort((a, b) => _mappedDeviceIds[a.Device.UUID].CompareTo(_mappedDeviceIds[b.Device.UUID]));
            return pairsList;
        }

        public override async Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/stat");
                api.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                var apiDevices = summary.devices;

                foreach (var miningPair in _miningPairs)
                {
                    var deviceUUID = miningPair.Device.UUID;
                    var minerID = _mappedDeviceIds[deviceUUID];
                    var apiDevice = apiDevices.Find(apiDev => apiDev.gpu_id == minerID);
                    if (apiDevice == null) continue;

                    totalSpeed += apiDevice.speed;
                    var kPower = apiDevice.power_usage * 1000;
                    totalPowerUsage += kPower;
                    if (_algorithmSecondType == AlgorithmType.NONE)
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)> { (_algorithmType, apiDevice.speed * (1 - DevFee * 0.01)) });
                    }
                    else
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)> {
                            (_algorithmType, apiDevice.speed * (1 - DevFee * 0.01)) });
                    }
                    perDevicePowerInfo.Add(deviceUUID, kPower);
                }
                api.PowerUsageTotal = totalPowerUsage;
                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }

        protected override void Init()
        {
            // separator " "
            _devices = string.Join(" ", _miningPairs.Select(p => _mappedDeviceIds[p.Device.UUID]));
        }
        private bool _disposed = false;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                try
                {
                    _httpClient.Dispose();
                }
                catch (Exception) { }
            }
            _disposed = true;
        }
        ~GMiner()
        {
            Dispose(false);
        }
    }
}

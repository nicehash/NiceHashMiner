using MP.Joker.Settings;
using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static MP.Joker.PluginEngines;

namespace MP.Joker
{
    public class JokerMiner : MinerBase
    {
        protected AlgorithmType _algorithmSecondType = AlgorithmType.NONE;
        private string _devices;
        private int _apiPort;
        PluginEngine _pluginEngine;

        private MinerSettings _minerSettings = null;

        private DateTime _started = DateTime.MinValue;

        private readonly HttpClient _httpClient = new HttpClient();

        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        // the order of intializing devices is the order how the API responds
        protected Dictionary<string, int> _mappedDeviceIds;

        private int _openClAmdPlatformNum;

        // the order of intializing devices is the order how the API responds
        private Dictionary<int, string> _initOrderMirrorApiOrderUUIDs = new Dictionary<int, string>();

        internal JokerMiner(string uuid, Dictionary<string, int> mappedDeviceIds, MinerSettings minerSettings, PluginEngine pluginEngine, int openClAmdPlatformNum) : base(uuid)
        {
            _mappedDeviceIds = mappedDeviceIds;
            _minerSettings = minerSettings;
            _pluginEngine = pluginEngine;
            _openClAmdPlatformNum = openClAmdPlatformNum;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        #region GetMinerStatsDataAsync

        public override Task<ApiData> GetMinerStatsDataAsync()
        {
            switch (_pluginEngine)
            {
                case PluginEngine.CryptoDredge: return CryptoDredgeGetMinerStatsDataAsync();
                case PluginEngine.GMiner: return GMinerGetMinerStatsDataAsync();
                case PluginEngine.LolMiner: return LolMinerGetMinerStatsDataAsync();
                case PluginEngine.MiniZ: return MiniZGetMinerStatsDataAsync();
                case PluginEngine.NanoMiner: return NanoMinerGetMinerStatsDataAsync();
                case PluginEngine.NBMiner: return NBMinerGetMinerStatsDataAsync();
                case PluginEngine.Phoenix: return PhoenixGetMinerStatsDataAsync();
                case PluginEngine.SRBMiner: return SRBMinerGetMinerStatsDataAsync();
                case PluginEngine.TeamRedMiner: return TeamRedMinerGetMinerStatsDataAsync();
                case PluginEngine.TRex: return TRexGetMinerStatsDataAsync();
                case PluginEngine.TTMiner: return TTMinerGetMinerStatsDataAsync();
                case PluginEngine.WildRig: return WildRigGetMinerStatsDataAsync();
                case PluginEngine.XMRig: return XMRigGetMinerStatsDataAsync();
                case PluginEngine.ZEnemy: return ZEnemyGetMinerStatsDataAsync();
                
                default: return GetMinerStatsDataAsyncSTUB();
            }
        }

        #region CryptoDredge
        
        private struct IdPowerHash
        {
            public int id;
            public int power;
            public double speed;
        }
        public async Task<ApiData> CryptoDredgeGetMinerStatsDataAsync()
        {
            var api = new ApiData();
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            var perDevicePowerInfo = new Dictionary<string, int>();
            var totalSpeed = 0d;
            var totalPowerUsage = 0;

            try
            {
                var result = await ApiDataHelpers.GetApiDataAsync(_apiPort, "summary", _logGroup);
                api.ApiResponse = result;
                if (result == "") return api;

                //total speed
                if (!string.IsNullOrEmpty(result))
                {
                    try
                    {
                        var summaryOptvals = result.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var optvalPairs in summaryOptvals)
                        {
                            var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (pair.Length != 2) continue;
                            if (pair[0] == "KHS")
                            {
                                totalSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }

                var threadsApiResult = await ApiDataHelpers.GetApiDataAsync(_apiPort, "threads", _logGroup);
                if (!string.IsNullOrEmpty(threadsApiResult))
                {
                    try
                    {
                        var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        var apiDevices = new List<IdPowerHash>();

                        foreach (var gpu in gpus)
                        {
                            var gpuOptvalPairs = gpu.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            var gpuData = new IdPowerHash();
                            foreach (var optvalPairs in gpuOptvalPairs)
                            {
                                var optval = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                                if (optval.Length != 2) continue;
                                if (optval[0] == "GPU")
                                {
                                    gpuData.id = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                }
                                if (optval[0] == "POWER")
                                {
                                    gpuData.power = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                }
                                if (optval[0] == "KHS")
                                {
                                    gpuData.speed = double.Parse(optval[1], CultureInfo.InvariantCulture) * 1000; // HPS
                                }
                            }
                            apiDevices.Add(gpuData);
                        }

                        foreach (var miningPair in _miningPairs)
                        {
                            var deviceUUID = miningPair.Device.UUID;
                            var deviceID = miningPair.Device.ID;

                            var apiDevice = apiDevices.Find(apiDev => apiDev.id == deviceID);
                            if (apiDevice.Equals(default(IdPowerHash))) continue;
                            perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, apiDevice.speed * (1 - DevFee * 0.01)) });
                            perDevicePowerInfo.Add(deviceUUID, apiDevice.power);
                            totalPowerUsage += apiDevice.power;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
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
        #endregion CryptoDredge

        #region GMiner

        internal static class GMiner
        {
            [Serializable]
            internal class Device
            {
                public int gpu_id { get; set; }
                public string bus_id { get; set; }
                public string name { get; set; }
                public double speed { get; set; }
                public double speed2 { get; set; }
                public int accepted_shares { get; set; }
                public int accepted_shares2 { get; set; }
                public int rejected_shares { get; set; }
                public int rejected_shares2 { get; set; }
                public int temperature { get; set; }
                public int temperature_limit { get; set; }
                public int power_usage { get; set; }
            }

            [Serializable]
            internal class JsonApiResponse
            {
                public int uptime { get; set; }
                public string server { get; set; }
                public string user { get; set; }
                public string algorithm { get; set; }
                public double electricity { get; set; }
                public int total_accepted_shares { get; set; }
                public int total_rejected_shares { get; set; }
                public List<Device> devices { get; set; }
                public int speedRatePrecision { get; set; }
                public string speedUnit { get; set; }
                public string powerUnit { get; set; }
            }
        }

        public async Task<ApiData> GMinerGetMinerStatsDataAsync()
        {
            // lazy init
            //if (_httpClient == null) _httpClient = new HttpClient();
            var ad = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/stat");
                ad.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<GMiner.JsonApiResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalSpeed2 = 0d;
                var totalPowerUsage = 0;
                foreach (var gpu in gpus)
                {
                    var currentDevStats = summary.devices.Where(devStats => devStats.gpu_id == _mappedDeviceIds[gpu.UUID]).FirstOrDefault();
                    if (currentDevStats == null) continue;
                    totalSpeed += currentDevStats.speed;
                    totalSpeed2 += currentDevStats.speed2;
                    if (_algorithmSecondType == AlgorithmType.NONE)
                    {
                        perDeviceSpeedInfo.Add(gpu.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentDevStats.speed * (1 - DevFee * 0.01)) });
                    }
                    else
                    {
                        // only one dual algo here
                        perDeviceSpeedInfo.Add(gpu.UUID, new List<(AlgorithmType type, double speed)>() {
                            (_algorithmType, currentDevStats.speed * (1 - 3.0 * 0.01)),
                            (_algorithmSecondType, currentDevStats.speed2 * (1 - DevFee * 0.01))
                        });
                    }
                    var kPower = currentDevStats.power_usage * 1000;
                    totalPowerUsage += kPower;
                    perDevicePowerInfo.Add(gpu.UUID, kPower);
                }
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                //CurrentMinerReadStatus = MinerApiReadStatus.NETWORK_EXCEPTION;
            }

            return ad;
        }


        #endregion GMiner

        #region LolMiner

        internal static class LolMiner
        {
            [Serializable]
            internal class Session
            {
                public int Active_GPUs { get; set; }
                public double Performance_Summary { get; set; }
                public string Performance_Unit { get; set; }
            }

            [Serializable]
            internal class GPU
            {
                public int Index { get; set; }
                public string Name { get; set; }
                public double Performance { get; set; }
            }

            [Serializable]
            internal class ApiJsonResponse
            {
                public Session Session { get; set; }
                public List<GPU> GPUs { get; set; }
            }
        }

        private static int GetMultiplier(string speedUnit)
        {
            switch (speedUnit)
            {
                case "mh/s": return 1000000; //1M
                case "kh/s": return 1000; //1k
                default: return 1;
            }
        }
        public async Task<ApiData> LolMinerGetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var summaryApiResult = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/summary");
                ad.ApiResponse = summaryApiResult;
                var summary = JsonConvert.DeserializeObject<LolMiner.ApiJsonResponse>(summaryApiResult);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var multiplier = GetMultiplier(summary.Session.Performance_Unit);
                var totalSpeed = summary.Session.Performance_Summary * multiplier;

                var totalPowerUsage = 0;
                var perDevicePowerInfo = new Dictionary<string, int>();

                var apiDevices = summary.GPUs;

                foreach (var pair in _miningPairs)
                {
                    var gpuUUID = pair.Device.UUID;
                    var gpuID = _mappedDeviceIds[gpuUUID];
                    var currentStats = summary.GPUs.Where(devStats => devStats.Index == gpuID).FirstOrDefault();
                    if (currentStats == null) continue;
                    perDeviceSpeedInfo.Add(gpuUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentStats.Performance * multiplier * (1 - DevFee * 0.01)) });
                }

                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsageTotal = totalPowerUsage;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }


        #endregion LolMiner

        #region MiniZ

        internal static class MiniZ
        {
            [Serializable]
            internal class Result
            {
                public uint gpuid { get; set; }
                public uint cudaid { get; set; }
                public string busid { get; set; }
                public uint gpu_status { get; set; }
                public int temperature { get; set; }
                public uint gpu_power_usage { get; set; }
                public uint speed_sps { get; set; }
            }

            [Serializable]
            internal class JsonApiResponse
            {
                public object error { get; set; }
                public List<Result> result { get; set; }
            }
        }

        public async Task<ApiData> MiniZGetMinerStatsDataAsync()
        {
            if (_started == DateTime.MinValue) _started = DateTime.Now;
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
                MiniZ.JsonApiResponse resp = null;
                using (var client = new TcpClient("127.0.0.1", _apiPort))
                using (var nwStream = client.GetStream())
                {
                    var bytesToSend = Encoding.ASCII.GetBytes("{\"id\":\"0\", \"method\":\"getstat\"}\\n");
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    api.ApiResponse = respStr;
                    resp = JsonConvert.DeserializeObject<MiniZ.JsonApiResponse>(respStr);
                    client.Close();
                }

                // return if we got nothing
                var respOK = resp != null && resp.error == null;
                if (respOK == false) return api;

                var results = resp.result;

                var gpus = _miningPairs
                    .Select(pair => pair.Device)
                    .Where(dev => dev is IGpuDevice)
                    .Cast<IGpuDevice>();

                foreach (var gpu in gpus)
                {
                    var currentDevStats = results.Where(r => int.Parse(r.busid.Split(':')[1], NumberStyles.HexNumber) == gpu.PCIeBusID).FirstOrDefault();
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


        #endregion MiniZ

        #region NanoMiner

        internal static class NanoMiner
        {
            [Serializable]
            public class JsonApiResponse
            {
                public List<Dictionary<string, Dictionary<string, object>>> Algorithms { get; set; }
                public List<Dictionary<string, DeviceData>> Devices { get; set; }
            }

            [Serializable]
            public class DeviceData
            {
                public string Name { get; set; }
                public string Platform { get; set; }
                public string Pci { get; set; }
                public int Temperature { get; set; }
                public double Power { get; set; }
            }

            [Serializable]
            public class HashrateStats
            {
                public double Accepted { get; set; }
                public double Denied { get; set; }
                public double Hashrate { get; set; }
            }

            public struct DeviceStatsData
            {
                // hashrate stuff
                public double Accepted { get; set; }
                public double Denied { get; set; }
                public double Hashrate { get; set; }

                // device monitoring
                public int Temperature { get; set; }
                public double Power { get; set; }

                public DeviceStatsData(HashrateStats hashrate, DeviceData deviceData)
                {
                    Accepted = hashrate?.Accepted ?? 0;
                    Denied = hashrate?.Denied ?? 0;
                    Hashrate = hashrate?.Hashrate ?? 0;
                    Temperature = deviceData?.Temperature ?? 0;
                    Power = deviceData?.Power ?? 0d;
                }
            }

            public static class JsonApiHelpers
            {
                public static double HashrateFromApiData(string data, string logGroup)
                {
                    try
                    {
                        var hashSplit = data.Substring(data.IndexOf("Hashrate")).Replace("\"", "").Split(':');
                        var hash = hashSplit[1].Substring(0, hashSplit[1].IndexOf('\r'));
                        return double.Parse(hash, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(logGroup, $"Failed parsing hashrate: {e.Message}");
                        return 0.0;
                    }
                }

                private static Dictionary<string, object> GetAlgorithmStats(JsonApiResponse apiResponse)
                {
                    if (apiResponse == null) return null;
                    if (apiResponse.Algorithms == null) return null;
                    var algo = apiResponse.Algorithms.FirstOrDefault();
                    if (algo == null) return null;
                    return algo.FirstOrDefault().Value;
                }

                public static Dictionary<string, DeviceStatsData> ParseJsonApiResponse(JsonApiResponse apiResponse, Dictionary<string, int> mappedIDs)
                {
                    var ret = new Dictionary<string, DeviceStatsData>();

                    // API seems to return these two by algorithm we mine 1 at a time so get first or default
                    var devs = apiResponse?.Devices?.FirstOrDefault();
                    var algos = GetAlgorithmStats(apiResponse);

                    if (devs != null && algos != null && mappedIDs != null)
                    {
                        // get all keys and filter out 
                        var keys = new HashSet<string>();
                        foreach (var key in devs.Keys) keys.Add(key);
                        foreach (var key in algos.Keys) keys.Add(key);
                        keys.RemoveWhere(key => !key.Contains("GPU"));

                        foreach (var key in keys)
                        {
                            if (key.Contains("GPU"))
                            {
                                var keyGPUStrID = key.Split(' ').LastOrDefault();
                                if (!int.TryParse(keyGPUStrID, out var minerID)) continue;
                                var devUUIDPair = mappedIDs.Where(kvp => kvp.Value == minerID).FirstOrDefault();
                                if (devUUIDPair.Equals(default(KeyValuePair<string, int>))) continue;
                                var devUUID = devUUIDPair.Key;
                                var hashrate = JsonConvert.DeserializeObject<HashrateStats>(algos[key].ToString());
                                ret[devUUID] = new DeviceStatsData(hashrate, devs[key]);
                            }
                            else if (key.Contains("Total") && algos.ContainsKey(key))
                            {
                                ret[key] = new DeviceStatsData(null, devs[key]);
                            }
                            else
                            {
                                // what??
                            }
                        }
                    }
                    return ret;
                }
            }
        }

        public async Task<ApiData> NanoMinerGetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/stats");
                api.ApiResponse = result;
                var apiResponse = JsonConvert.DeserializeObject<NanoMiner.JsonApiResponse>(result);
                var parsedApiResponse = NanoMiner.JsonApiHelpers.ParseJsonApiResponse(apiResponse, _mappedDeviceIds);

                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                foreach (var miningPair in _miningPairs)
                {
                    var deviceUUID = miningPair.Device.UUID;
                    if (parsedApiResponse.ContainsKey(deviceUUID))
                    {
                        var stat = parsedApiResponse[deviceUUID];
                        var currentPower = (int)stat.Power;
                        totalPowerUsage += currentPower;
                        var hashrate = stat.Hashrate * (1 - DevFee * 0.01);
                        totalSpeed += hashrate;
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, hashrate) });
                        perDevicePowerInfo.Add(deviceUUID, currentPower);
                    }
                    else
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, 0) });
                        perDevicePowerInfo.Add(deviceUUID, 0);
                    }
                }

                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
                api.PowerUsageTotal = totalPowerUsage;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }


        #endregion NanoMiner

        #region NBMiner

        internal static class NBMiner
        {
            internal class DeviceApi
            {
                public double core_clock { get; set; }
                public double core_utilization { get; set; }
                public double fan { get; set; }
                public string hashrate { get; set; }
                public string hashrate2 { get; set; }
                public double hashrate2_raw { get; set; }
                public double hashrate_raw { get; set; }
                public int id { get; set; }
                public string info { get; set; }
                public double mem_clock { get; set; }
                public double mem_utilization { get; set; }
                public double power { get; set; }
                public double temperature { get; set; }
            }

            internal class Miner
            {
                public List<DeviceApi> devices { get; set; }
                public string total_hashrate { get; set; }
                public string total_hashrate2 { get; set; }
                public double total_hashrate2_raw { get; set; }
                public double total_hashrate_raw { get; set; }
                public double total_power_consume { get; set; }
            }

            //internal class Stratum
            //{
            //    public int accepted_shares { get; set; }
            //    public string algorithm { get; set; }
            //    public string difficulty { get; set; }
            //    public int latency { get; set; }
            //    public int rejected_shares { get; set; }
            //    public string url { get; set; }
            //    public bool use_ssl { get; set; }
            //    public string user { get; set; }
            //}

            internal class JsonApiResponse
            {
                public Miner miner { get; set; }
                //public int start_time { get; set; }
                //public Stratum stratum { get; set; }
                public string version { get; set; }
            }
        }

        public async Task<ApiData> NBMinerGetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/api/v1/status");
                api.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<NBMiner.JsonApiResponse>(result);

                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalSpeed2 = 0d;
                var totalPowerUsage = 0;

                var apiDevices = summary.miner.devices;

                foreach (var miningPair in _miningPairs)
                {
                    var deviceUUID = miningPair.Device.UUID;
                    var minerID = _mappedDeviceIds[deviceUUID];
                    var apiDevice = apiDevices.Find(apiDev => apiDev.id == minerID);
                    if (apiDevice == null) continue;

                    totalSpeed += apiDevice.hashrate_raw;
                    totalSpeed2 += apiDevice.hashrate2_raw;
                    var kPower = (int)apiDevice.power * 1000;
                    totalPowerUsage += kPower;
                    if (_algorithmSecondType == AlgorithmType.NONE)
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)> { (_algorithmType, apiDevice.hashrate_raw * (1 - DevFee * 0.01)) });
                    }
                    else
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)> {
                            (_algorithmType, apiDevice.hashrate_raw * (1 - DevFee * 0.01)),
                            (_algorithmSecondType, apiDevice.hashrate2_raw * (1 - DevFee * 0.01)) });
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


        #endregion NBMiner

        #region Phoenix

        internal static class ClaymoreAPIHelpers
        {
#pragma warning disable IDE1006 // Naming Styles
            public class JsonApiResponse
            {
                public List<string> result { get; set; }
                public int id { get; set; }
                public object error { get; set; }
            }
#pragma warning restore IDE1006 // Naming Styles
            const string _jsonStatsApiCall = "{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}\n";

            private static readonly List<double> _emptySpeeds = new List<double>();

            public static async Task<ApiData> GetMinerStatsDataAsync(int apiPort, IReadOnlyList<BaseDevice> miningDevices, string logGroup, double DevFee, double DualDevFee, params AlgorithmType[] algorithmTypes)
            {
                var ad = new ApiData();

                var firstAlgoType = AlgorithmType.NONE;
                var secondAlgoType = AlgorithmType.NONE;

                bool isDual = algorithmTypes.Count() > 1;

                if (algorithmTypes.Count() > 0) firstAlgoType = algorithmTypes[0];
                if (algorithmTypes.Count() > 1) secondAlgoType = algorithmTypes[1];

                var totalSpeed = new List<(AlgorithmType type, double speed)>();
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();

                JsonApiResponse resp = null;
                try
                {
                    var bytesToSend = Encoding.ASCII.GetBytes(_jsonStatsApiCall);
                    using (var client = new TcpClient("127.0.0.1", apiPort))
                    using (var nwStream = client.GetStream())
                    {
                        await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                        var bytesToRead = new byte[client.ReceiveBufferSize];
                        var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                        var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                        ad.ApiResponse = respStr;
                        resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr);
                    }
                    if (resp != null && resp.error == null)
                    {
                        if (resp.result != null && resp.result.Count > 3)
                        {
                            var speeds = TransformSpeedsList(resp.result[3]);
                            var hasSecond = isDual && resp.result.Count > 5;
                            var secondarySpeeds = hasSecond ? TransformSpeedsList(resp.result[5]) : _emptySpeeds;
                            var primaryTotalSpeed = 0d;
                            var secondaryTotalSpeed = 0d;

                            for (int i = 0; i < miningDevices.Count(); i++)
                            {
                                var dev = miningDevices[i];
                                var uuid = dev.UUID;

                                var primaryCurrentSpeed = speeds.Count > i ? speeds[i] : 0d;
                                var secondaryCurrentSpeed = secondarySpeeds.Count > i ? secondarySpeeds[i] : 0d;

                                primaryTotalSpeed += primaryCurrentSpeed;
                                secondaryTotalSpeed += secondaryCurrentSpeed;

                                var perDeviceSpeeds = new List<(AlgorithmType type, double speed)>() { (firstAlgoType, primaryCurrentSpeed * (1 - DevFee * 0.01)) };
                                if (isDual)
                                {
                                    perDeviceSpeeds.Add((secondAlgoType, secondaryCurrentSpeed * (1 - DualDevFee * 0.01)));
                                }
                                perDeviceSpeedInfo.Add(uuid, perDeviceSpeeds);
                                // no power usage info
                                perDevicePowerInfo.Add(uuid, -1);
                            }

                            totalSpeed.Add((firstAlgoType, primaryTotalSpeed * (1 - DevFee * 0.01)));
                            if (isDual)
                            {
                                totalSpeed.Add((secondAlgoType, secondaryTotalSpeed * (1 - DualDevFee * 0.01)));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                }
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
                ad.PowerUsageTotal = -1;
                return ad;
            }

            private static List<double> TransformSpeedsList(string speedsStr)
            {
                var ret = new List<double>();
                var speeds = speedsStr.Split(';');
                foreach (var speed in speeds)
                {
                    double parsedSpeed = 0;
                    try
                    {
                        parsedSpeed = double.Parse(speed, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        parsedSpeed = 0;
                    }
                    ret.Add(parsedSpeed);
                }
                return ret;
            }
        }

        public async Task<ApiData> PhoenixGetMinerStatsDataAsync()
        {
            var miningDevices = _miningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = new AlgorithmType[] { _algorithmType };
            // multiply dagger API data 
            var ad = await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, DevFee, 0.0, algorithmTypes);
            if (ad.AlgorithmSpeedsPerDevice != null)
            {
                // speed is in khs
                ad.AlgorithmSpeedsPerDevice = ad.AlgorithmSpeedsPerDevice.Select(pair => new KeyValuePair<string, IReadOnlyList<(AlgorithmType type, double speed)>>(pair.Key, pair.Value.Select((ts) => (ts.type, ts.speed * 1000)).ToList())).ToDictionary(x => x.Key, x => x.Value);
            }
            return ad;
        }

        #endregion Phoenix

        #region SRBMiner

        internal static class SRBMiner
        {
            [Serializable]
            internal class Device
            {
                public int id { get; set; }
                public int bus_id { get; set; }
                public string device { get; set; }
            }

            [Serializable]
            internal class AlgorithmInfo
            {
                public int id { get; set; }
                public string name { get; set; }
                public Hashrate hashrate { get; set; }
            }

            [Serializable]
            internal class Hashrate
            {
                public Dictionary<string, double> gpu { get; set; }
            }

            [Serializable]
            internal class ApiJsonResponse
            {
                public double hashrate_total_now { get; set; }
                public List<Device> gpu_devices { get; set; }
                public List<AlgorithmInfo> algorithms { get; set; }
            }
        }

        public async Task<ApiData> SRBMinerGetMinerStatsDataAsync()
        {
            // lazy init
            //if (_httpClient == null) _httpClient = new HttpClient();
            var ad = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}");
                ad.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<SRBMiner.ApiJsonResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                var amdDevices = gpus.Cast<AMDDevice>();
                foreach (var gpu in amdDevices)
                {
                    var algorithmDevices = summary.algorithms.FirstOrDefault().hashrate.gpu;
                    var deviceName = summary.gpu_devices.Where(dev => dev.bus_id == gpu.PCIeBusID).FirstOrDefault().device;
                    var currentDevStats = algorithmDevices.Where(dev => dev.Key == deviceName).FirstOrDefault().Value;
                    if (currentDevStats == 0) continue;

                    totalSpeed += currentDevStats;
                    perDeviceSpeedInfo.Add(gpu.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentDevStats * (1 - DevFee * 0.01)) });
                }
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                //CurrentMinerReadStatus = MinerApiReadStatus.NETWORK_EXCEPTION;
            }

            return ad;
        }

        #endregion SRBMiner

        #region TeamRedMiner

        internal static class TeamRedMiner
        {
            #region JSON Generated code
#pragma warning disable IDE1006 // Naming Styles
            public class ApiSTATUS
            {
                public string STATUS { get; set; }
                public int When { get; set; }
                public int Code { get; set; }
                public string Msg { get; set; }
                public string Description { get; set; }
            }

            public class ApiSUMMARY
            {
                public int Elapsed { get; set; }

                [JsonProperty("MHS av")]
                public double MHS_av { get; set; }

                [JsonProperty("MHS 5s")]
                public double MHS_5s { get; set; }

                [JsonProperty("KHS av")]
                public int KHS_av { get; set; }

                [JsonProperty("KHS 5s")]
                public int KHS_5s { get; set; }

                [JsonProperty("Found Blocks")]
                public int Found_Blocks { get; set; }

                public int Getworks { get; set; }
                public int Accepted { get; set; }
                public int Rejected { get; set; }

                [JsonProperty("Hardware Errors")]
                public int Hardware_Errors { get; set; }

                public double Utility { get; set; }
                public int Discarded { get; set; }
                public int Stale { get; set; }

                [JsonProperty("Get Failures")]
                public int Get_Failures { get; set; }

                [JsonProperty("Local Work")]
                public int Local_Work { get; set; }

                [JsonProperty("Remote Failures")]
                public int Remote_Failures { get; set; }

                [JsonProperty("Network Blocks")]
                public int Network_Blocks { get; set; }

                [JsonProperty("Total MH")]
                public double Total_MH { get; set; }

                [JsonProperty("Work Utility")]
                public double Work_Utility { get; set; }

                [JsonProperty("Difficulty Accepted")]
                public double Difficulty_Accepted { get; set; }

                [JsonProperty("Difficulty Rejected")]
                public double Difficulty_Rejected { get; set; }

                [JsonProperty("Difficulty Stale")]
                public double Difficulty_Stale { get; set; }

                [JsonProperty("Best Share")]
                public double Best_Share { get; set; }

                [JsonProperty("Device Hardware%")]
                public double Device_HardwarePerc { get; set; }

                [JsonProperty("Device Rejected%")]
                public double Device_RejectedPerc { get; set; }

                [JsonProperty("Pool Rejected%")]
                public double Pool_RejectedPerc { get; set; }

                [JsonProperty("Pool Stale%")]
                public double Pool_StalePerc { get; set; }

                [JsonProperty("Last getwork")]
                public int Last_getwork { get; set; }
            }

            // JSON API: {"command": "summary"}
            public class ApiSummaryRoot
            {
                public List<ApiSTATUS> STATUS { get; set; }
                public List<ApiSUMMARY> SUMMARY { get; set; }
                public int id { get; set; }
            }

            public class DEV
            {
                public int GPU { get; set; }
                public string Enabled { get; set; }
                public string Status { get; set; }
                public double Temperature { get; set; }

                //[JsonProperty("Fan Speed")]
                //public int Fan_Speed { get; set; }

                //[JsonProperty("Fan Percent")]
                //public int Fan_Percent { get; set; }

                //[JsonProperty("GPU Clock")]
                //public int GPU_Clock { get; set; }

                //[JsonProperty("Memory Clock")]
                //public int Memory_Clock { get; set; }

                [JsonProperty("GPU Voltage")]
                public double GPU_Voltage { get; set; }

                [JsonProperty("GPU Activity")]
                public int GPU_Activity { get; set; }

                public int Powertune { get; set; }

                //[JsonProperty("MHS av")]
                //public double MHS_av { get; set; }

                //[JsonProperty("MHS 5s")]
                //public double MHS_5s { get; set; }

                [JsonProperty("KHS av")]
                public double KHS_av { get; set; }

                [JsonProperty("KHS 5s")]
                public double KHS_5s { get; set; }

                public int Accepted { get; set; }
                public int Rejected { get; set; }

                [JsonProperty("Hardware_Errors")]
                public int Hardware_Errors { get; set; }

                //public double Utility { get; set; }
                //public string Intensity { get; set; }
                //public int XIntensity { get; set; }
                //public int RawIntensity { get; set; }

                //[JsonProperty("Last Share Pool")]
                //public int Last_Share_Pool { get; set; }

                //[JsonProperty("Last Share Time")]
                //public int Last_Share_Time { get; set; }

                //[JsonProperty("Total MH")]
                //public double Total_MH { get; set; }

                [JsonProperty("Diff1 Work")]
                public double Diff1_Work { get; set; }

                [JsonProperty("Difficulty Accepted")]
                public double Difficulty_Accepted { get; set; }

                [JsonProperty("Difficulty Rejected")]
                public double Difficulty_Rejected { get; set; }

                [JsonProperty("Last Share Difficulty")]
                public double Last_Share_Difficulty { get; set; }

                [JsonProperty("Last Valid Work")]
                public int Last_Valid_Work { get; set; }

                [JsonProperty("Device Hardware%")]
                public double Device_HardwarePerc { get; set; }

                [JsonProperty("Device Rejected%")]
                public double Device_RejectedPerc { get; set; }

                [JsonProperty("Device Elapsed")]
                public int Device_Elapsed { get; set; }
            }

            // JSON API: {"command": "devs"}
            public class ApiDevsRoot
            {
                public List<ApiSTATUS> STATUS { get; set; }
                public List<DEV> DEVS { get; set; }
                public int id { get; set; }
            }
#pragma warning restore IDE1006 // Naming Styles
            #endregion JSON Generated code

            public static class APIHelpers
            {
                const string jsonDevsApiCall = "{\"command\": \"devs\"}";

                private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Culture = CultureInfo.InvariantCulture
                };

                public static ApiDevsRoot ParseApiDevsRoot(string respStr)
                {
                    var resp = JsonConvert.DeserializeObject<ApiDevsRoot>(respStr, _jsonSettings);
                    return resp;
                }

                public static async Task<(ApiDevsRoot root, string response)> GetApiDevsRootAsync(int port, string logGroup)
                {
                    try
                    {
                        using (var client = new TcpClient("127.0.0.1", port))
                        using (var nwStream = client.GetStream())
                        {
                            var bytesToSend = Encoding.ASCII.GetBytes(jsonDevsApiCall);
                            await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                            var bytesToRead = new byte[client.ReceiveBufferSize];
                            var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                            var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                            client.Close();
                            var resp = JsonConvert.DeserializeObject<ApiDevsRoot>(respStr, _jsonSettings);
                            return (resp, respStr);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                        return (null, null);
                    }
                }
            }

        }

        public async Task<ApiData> TeamRedMinerGetMinerStatsDataAsync()
        {
            var (apiDevsResult, response) = await TeamRedMiner.APIHelpers.GetApiDevsRootAsync(_apiPort, _logGroup);
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


        #endregion TeamRedMiner

        #region TRex

        internal static class TRex
        {
            [Serializable]
            internal class Gpu
            {
                public int device_id { get; set; }
                public string efficiency { get; set; }
                public int fan_speed { get; set; }
                public int gpu_id { get; set; }
                public int hashrate { get; set; }
                public double intensity { get; set; }
                public string name { get; set; }
                public int power { get; set; }
                public int temperature { get; set; }
                public string vendor { get; set; }
            }

            [Serializable]
            internal class JsonApiResponse
            {
                public string algorithm { get; set; }
                public string api { get; set; }
                public string cuda { get; set; }
                public string description { get; set; }
                public double difficulty { get; set; }
                public int gpu_total { get; set; }
                public List<Gpu> gpus { get; set; }
                public int hashrate { get; set; }
                public string name { get; set; }
                public string os { get; set; }
            }

        }

        public async Task<ApiData> TRexGetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var summaryApiResult = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/summary");
                ad.ApiResponse = summaryApiResult;
                var summary = JsonConvert.DeserializeObject<TRex.JsonApiResponse>(summaryApiResult);

                var gpuDevices = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = summary.hashrate;
                var totalPowerUsage = 0.0;

                foreach (var gpuDevice in gpuDevices)
                {
                    var currentStats = summary.gpus.Where(devStats => devStats.gpu_id == gpuDevice.ID).FirstOrDefault();
                    if (currentStats == null) continue;
                    perDeviceSpeedInfo.Add(gpuDevice.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentStats.hashrate * (1 - DevFee * 0.01)) });
                    var kPower = currentStats.power * 1000;
                    totalPowerUsage += kPower;
                    perDevicePowerInfo.Add(gpuDevice.UUID, kPower);
                }
                ad.PowerUsageTotal = Convert.ToInt32(totalPowerUsage);
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;

            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }


        #endregion TRex

        #region TTMiner

        public async Task<ApiData> TTMinerGetMinerStatsDataAsync()
        {
            if (_started == DateTime.MinValue) _started = DateTime.Now;
            var api = new ApiData();
            var elapsedSeconds = DateTime.Now.Subtract(_started).Seconds;
            if (elapsedSeconds < 15)
            {
                return api;
            }

            var miningDevices = _miningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = new AlgorithmType[] { _algorithmType };
            return await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, DevFee, 0.0, algorithmTypes);
        }

        #endregion TTMiner

        #region WildRig

        internal static class WildRig
        {
            internal class JsonApiResponse
            {
                public Hashrate hashrate { get; set; }
            }

            internal class Hashrate
            {
                public List<List<int>> threads { get; set; }
            }
        }

        public async Task<ApiData> WildRigGetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}");
                ad.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<WildRig.JsonApiResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                var hashrate = summary.hashrate;
                if (hashrate != null)
                {
                    for (int i = 0; i < gpus.Count(); i++)
                    {
                        var deviceSpeed = hashrate.threads.ElementAtOrDefault(i).FirstOrDefault();
                        totalSpeed += deviceSpeed;
                        perDeviceSpeedInfo.Add(gpus.ElementAt(i)?.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, deviceSpeed * (1 - DevFee * 0.01)) });
                    }
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


        #endregion WildRig

        #region XMRig

        internal static class XMRig
        {
            [Serializable]
            public class Hashrate
            {
                public List<double?> total { get; set; }
            }

            [Serializable]
            public class JsonApiResponse
            {
                public string version { get; set; }
                public Hashrate hashrate { get; set; }
            }
        }

        public async Task<ApiData> XMRigGetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/1/summary");
                api.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<XMRig.JsonApiResponse>(result);

                var totalSpeed = 0d;
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                // init per device sums
                foreach (var pair in _miningPairs)
                {
                    var uuid = pair.Device.UUID;
                    var currentSpeed = summary.hashrate.total.FirstOrDefault() ?? 0d;
                    totalSpeed += currentSpeed;
                    perDeviceSpeedInfo.Add(uuid, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentSpeed * (1 - DevFee * 0.01)) });
                    // no power usage info
                    perDevicePowerInfo.Add(uuid, -1);
                }

                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
                api.PowerUsageTotal = -1;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }


        #endregion XMRig

        #region ZEnemy

        public static class CCMinerAPIHelpers
        {
            public static Task<string> GetApiDataSummary(int port, string logGroup)
            {
                var dataToSend = ApiDataHelpers.GetHttpRequestNhmAgentString("summary");
                return ApiDataHelpers.GetApiDataAsync(port, dataToSend, logGroup);
            }

            public static Task<string> GetApiDataThreads(int port, string logGroup)
            {
                var dataToSend = ApiDataHelpers.GetHttpRequestNhmAgentString("threads");
                return ApiDataHelpers.GetApiDataAsync(port, dataToSend, logGroup);
            }

            private struct IdPowerHash
            {
                public int id;
                public int power;
                public double speed;
            }

            public static async Task<ApiData> GetMinerStatsDataAsync(int port, AlgorithmType algorithmType, IEnumerable<MiningPair> miningPairs, string logGroup, double devFee)
            {
                var summaryApiResult = await GetApiDataSummary(port, logGroup);
                double totalSpeed = 0;
                int totalPower = 0;
                if (!string.IsNullOrEmpty(summaryApiResult))
                {
                    try
                    {
                        var summaryOptvals = summaryApiResult.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var optvalPairs in summaryOptvals)
                        {
                            var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (pair.Length != 2) continue;
                            if (pair[0] == "KHS")
                            {
                                totalSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }
                var threadsApiResult = await GetApiDataThreads(port, logGroup);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();

                if (!string.IsNullOrEmpty(threadsApiResult))
                {
                    try
                    {
                        var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        var apiDevices = new List<IdPowerHash>();

                        foreach (var gpu in gpus)
                        {
                            var gpuOptvalPairs = gpu.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            var gpuData = new IdPowerHash();
                            foreach (var optvalPairs in gpuOptvalPairs)
                            {
                                var optval = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                                if (optval.Length != 2) continue;
                                if (optval[0] == "GPU")
                                {
                                    gpuData.id = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                }
                                if (optval[0] == "POWER")
                                {
                                    gpuData.power = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                }
                                if (optval[0] == "KHS")
                                {
                                    gpuData.speed = double.Parse(optval[1], CultureInfo.InvariantCulture) * 1000; // HPS
                                }
                            }
                            apiDevices.Add(gpuData);
                        }

                        foreach (var miningPair in miningPairs)
                        {
                            var deviceUUID = miningPair.Device.UUID;
                            var deviceID = miningPair.Device.ID;

                            var apiDevice = apiDevices.Find(apiDev => apiDev.id == deviceID);
                            if (apiDevice.Equals(default(IdPowerHash))) continue;
                            perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (algorithmType, apiDevice.speed * (1 - devFee * 0.01)) });
                            perDevicePowerInfo.Add(deviceUUID, apiDevice.power);
                            totalPower += apiDevice.power;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }
                var ad = new ApiData();
                ad.PowerUsageTotal = totalPower;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
                ad.ApiResponse = summaryApiResult;

                return ad;
            }

        }

        public Task<ApiData> ZEnemyGetMinerStatsDataAsync()
        {
            return CCMinerAPIHelpers.GetMinerStatsDataAsync(_apiPort, _algorithmType, _miningPairs, _logGroup, DevFee);
        }

        #endregion ZEnemy

        public async Task<ApiData> GetMinerStatsDataAsyncSTUB()
        {
            // TODO stubs here for now
            var ad = new ApiData();
            ad.PowerUsageTotal = 0;
            ad.PowerUsagePerDevice = new Dictionary<string, int>();
            try
            {
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                await Task.Delay(100);
                foreach (var (device, algo) in _miningPairs.Select(p => (device: p.Device, algorithm: p.Algorithm)))
                {
                    var gpuUUID = device.UUID;
                    perDeviceSpeedInfo.Add(gpuUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, algo.Speeds.FirstOrDefault() * (1 - DevFee * 0.01)) });
                }
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }

        #endregion GetMinerStatsDataAsync

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by mapped ids
            pairsList.Sort((a, b) => _mappedDeviceIds[a.Device.UUID].CompareTo(_mappedDeviceIds[b.Device.UUID]));
            return pairsList;
        }

        protected override void Init()
        {
            _devices = string.Join(_minerSettings.DevicesSeparator, _miningPairs.Select(p => _mappedDeviceIds[p.Device.UUID]));

            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmSecondType = dualType.Item1;
            var ok = dualType.Item2;
            if (!ok) _algorithmSecondType = AlgorithmType.NONE;

            var miningPairsArray = _miningPairs.ToArray();
            for (int i = 0; i < miningPairsArray.Length; i++)
            {
                _initOrderMirrorApiOrderUUIDs[i] = miningPairsArray[i].Device.UUID;
            }
        }

        private static (string url, string port, bool ok) SplitUrlWithPort(string urlWithPort)
        {
            var port = string.Join("", urlWithPort
                .Reverse()
                .TakeWhile(char.IsDigit)
                .Reverse());
            var url = urlWithPort.Replace($":{port}", "");
            return (url, port, int.TryParse(port, out var _)); 
        }

        private static string GetCommandLineTemplate(MinerSettings minerSettings, AlgorithmType algo)
        {
            if (minerSettings.AlgorithmCommandLine?.ContainsKey(algo) ?? false) return minerSettings.AlgorithmCommandLine[algo];
            return minerSettings.DefaultCommandLine;
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, _minerSettings.NhmConectionType);
            var (url, port, _) = SplitUrlWithPort(urlWithPort);
            var algo = AlgorithmName(_algorithmType);
            var commandLine = GetCommandLineTemplate(_minerSettings, _algorithmType)
                .Replace(MinerSettings.USERNAME_TEMPLATE, _username)
                .Replace(MinerSettings.API_PORT_TEMPLATE, $"{_apiPort}")
                .Replace(MinerSettings.POOL_URL_TEMPLATE, url)
                .Replace(MinerSettings.POOL_PORT_TEMPLATE, port)
                .Replace(MinerSettings.ALGORITHM_TEMPLATE, algo)
                .Replace(MinerSettings.DEVICES_TEMPLATE, _devices)
                .Replace(MinerSettings.OPEN_CL_AMD_PLATFORM_NUM, $"{_openClAmdPlatformNum}")
                .Replace(MinerSettings.EXTRA_LAUNCH_PARAMETERS_TEMPLATE, _extraLaunchParameters);

            return commandLine;
        }
    }
}

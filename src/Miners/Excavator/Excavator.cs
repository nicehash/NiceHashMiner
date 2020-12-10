using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.CCMinerCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Excavator
{
    public class Excavator : MinerBase
    {
        private int _apiPort;

        public Excavator(string uuid) : base(uuid)
        {}

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        public override async Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                const string speeds = @"{""id"":123456789,""method"":""worker.list"",""params"":[]}" + "\r\n";
                var response = await ApiDataHelpers.GetApiDataAsync(_apiPort, speeds, _logGroup);
                ad.ApiResponse = response;
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(response);
                var gpus = _miningPairs.Select(pair => pair.Device.UUID);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                //var totalSpeed2 = 0d;
                //var totalPowerUsage = 0;
                foreach (var gpu in gpus)
                {
                    var speed = summary.workers.Where(w => w.device_uuid == gpu).SelectMany(w => w.algorithms.Select(a => a.speed)).Sum();
                    totalSpeed += speed;
                    perDeviceSpeedInfo.Add(gpu, new List<(AlgorithmType type, double speed)>() { (_algorithmType, speed) });
                }
                ad.PowerUsageTotal = 0;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
                Logger.Info("EXCAVATOR", response);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Logger.Error("EXCAVATOR-API_ERR", e.ToString());
            }
            return ad;
        }

        protected override void Init() {}

        private static string CmdJSONString(string miningLocation, string username, params string[] uuids)
        {
            const string DEVICE = @"		{""id"":3,""method"":""worker.add"",""params"":[""daggerhashimoto"",""_DEV_ID_""]}";
            const string TEMPLATE = @"
[
	{""time"":0,""commands"":[
		{""id"":1,""method"":""subscribe"",""params"":[""nhmp._MINING_LOCATION_.nicehash.com:3200"",""_PUT_YOUR_BTC_HERE_""]}
	]},
	{""time"":1,""commands"":[
        {""id"":1,""method"":""algorithm.add"",""params"":[""daggerhashimoto""]}
    ]},
	{""time"":2,""commands"":[
_DEVICES_
	]},
	{""time"":10,""commands"":[
		{""id"":1,""method"":""worker.reset"",""params"":[""0""]}
	]},
	{""time"":15,""loop"":15,""commands"":[
		{""id"":1,""method"":""algorithm.print.speeds"",""params"":[]},
		{""id"":1,""method"":""worker.reset"",""params"":[""0""]}
	]}
]";
            var devices = string.Join(",\n", uuids.Select(uuid => DEVICE.Replace("_DEV_ID_", uuid)));
            return TEMPLATE
                .Replace("_MINING_LOCATION_", miningLocation)
                .Replace("_PUT_YOUR_BTC_HERE_", username)
                .Replace("_DEVICES_", devices);
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            var uuids = _miningPairs.Select(p => p.Device).Cast<CUDADevice>().Select(gpu => gpu.UUID);
            var ids = _miningPairs.Select(p => p.Device).Cast<CUDADevice>().Select(gpu => gpu.PCIeBusID);
            //var algo = AlgorithmName(_algorithmType);
            // "--algo {algo} --url={urlWithPort} --user {_username} 
            var (_, cwd) = GetBinAndCwdPaths();
            var fileName = $"cmd_{string.Join("_", ids)}.json";
            //Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            //var logName = $"log_{string.Join("_", ids)}_{unixTimestamp}.log";
            File.WriteAllText(Path.Combine(cwd, fileName), CmdJSONString(_miningLocation, _username, uuids.ToArray()));
            var commandLine = $"-p {_apiPort} -c {fileName}";
            return commandLine;
        }
    }
}

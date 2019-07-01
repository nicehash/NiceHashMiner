using MinerPlugin;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NiceHashMinerLegacy.Common.Device;
using CommonAlgorithm = NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMiner.Plugin;
using NiceHashMiner.Configs;
using NiceHashMiner.Miners.IntegratedPlugins;
using NiceHashMiner.Devices;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common;
using MinerPluginToolkitV1.Interfaces;
using System.Threading;

namespace NiceHashMiner.Miners
{
    // pretty much just implement what we need and ignore everything else
    public class MinerFromPlugin : Miner
    {
        private readonly PluginContainer _plugin;
        private readonly IMiner _miner;

        private readonly SemaphoreSlim _apiSemaphore = new SemaphoreSlim(1, 1);

        public MinerFromPlugin(string pluginUUID, List<MiningPair> miningPairs, string groupKey) : base(pluginUUID, miningPairs, groupKey)
        {
            _plugin = MinerPluginsManager.GetPluginWithUuid(pluginUUID);
            _miner = _plugin.CreateMiner();
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            var apiData = new ApiData();
            if (!IsUpdatingApi)
            {
                IsUpdatingApi = true;
                await _apiSemaphore.WaitAsync();
                try
                {
                    apiData = await _miner.GetMinerStatsDataAsync();
                }
                catch (Exception e)
                {
                    Logger.Error(MinerTag(), $"GetSummaryAsync error: {e.Message}");
                }
                finally
                {
                    IsUpdatingApi = false;
                    _apiSemaphore.Release();
                }
            }
            
            UpdateApiTimestamp(apiData);

            // TODO workaround plugins should return this info
            // create empty stub if it is null
            if (apiData == null)
            {
                Logger.Debug(MinerTag(), "GetSummary returned null... Will create ZERO fallback");
                apiData = new ApiData();
            }
            if (apiData.AlgorithmSpeedsPerDevice == null)
            {
                apiData = new ApiData();
                var perDevicePowerDict = new Dictionary<string, int>();
                var perDeviceSpeedsDict = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDeviceSpeeds = MiningPairs.Select(pair => (pair.Device.UUID, pair.Algorithm.IDs.Select(type => new AlgorithmTypeSpeedPair(type, 0d))));
                foreach (var kvp in perDeviceSpeeds)
                {
                    var uuid = kvp.Item1; // kvp.UUID compiler doesn't recognize ValueTypes lib???
                    perDeviceSpeedsDict[uuid] = kvp.Item2.ToList();
                    perDevicePowerDict[uuid] = 0;
                }
                apiData.AlgorithmSpeedsPerDevice = perDeviceSpeedsDict;
                apiData.PowerUsagePerDevice = perDevicePowerDict;
                apiData.PowerUsageTotal = 0;
                apiData.AlgorithmSpeedsTotal = perDeviceSpeedsDict.First().Value;
            } else if(apiData.AlgorithmSpeedsPerDevice != null && apiData.PowerUsagePerDevice.Count == 0)
            {
                var perDevicePowerDict = new Dictionary<string, int>();
                foreach (var kvp in MiningPairs)
                {
                    var uuid = kvp.Device.UUID;
                    perDevicePowerDict[uuid] = 0;
                }
                apiData.PowerUsagePerDevice = perDevicePowerDict;
                apiData.PowerUsageTotal = 0;
            }

            // TODO temporary here move it outside later
            MiningStats.UpdateGroup(apiData, _plugin.PluginUUID, _plugin.Name);

            return apiData;
        }

        // TODO this thing 
        public override void Start(string miningLocation, string username)
        {
            if (_isEnded) return;
            _miner.InitMiningLocationAndUsername(miningLocation, username);
            _miner.InitMiningPairs(MiningPairs);
            EthlargementIntegratedPlugin.Instance.Start(MiningPairs);
            _miner.StartMining();
            IsRunning = true;
            // maxTimeout = ConfigManager.GeneralConfig.CoolDownCheckEnabled
            var maxTimeout = _plugin.GetApiMaxTimeout();
            MinerApiWatchdog.AddGroup(GroupKey, maxTimeout, DateTime.UtcNow);
        }

        public override void Stop()
        {
            // TODO thing about this case, closing opening on switching
            // EthlargementIntegratedPlugin.Instance.Stop(_miningPairs);
            MinerApiWatchdog.RemoveGroup(GroupKey);
            MiningStats.RemoveGroup(MiningPairs.Select(pair => pair.Device.UUID), _plugin.PluginUUID);
            IsRunning = false;
            _miner.StopMining();
            //if (_miner is IDisposable disposableMiner)
            //{
            //    disposableMiner.Dispose();
            //}
        }

        #region MinerApiWatchdog
        private double _lastSpeedsTotalSum = 0d;
        private double _lastPerDevSpeedsTotalSum = 0d;

        private void UpdateApiTimestamp(ApiData apiData)
        {
            // we will not update api timestamps if we have no data or speeds are zero
            if (apiData == null)
            {
                // TODO debug log no api data
                return;
            }
            if (apiData.AlgorithmSpeedsTotal == null && apiData.AlgorithmSpeedsPerDevice == null)
            {
                // TODO debug log cannot get speeds
                return;
            }
            var speedsTotalSum = apiData.AlgorithmSpeedsTotal?.Select(p => p.Speed).Sum() ?? 0d;
            var perDevSpeedsTotalSum = apiData.AlgorithmSpeedsPerDevice?.Values.SelectMany(pl => pl).Select(p => p.Speed).Sum() ?? 0d;
            if (speedsTotalSum == 0d && perDevSpeedsTotalSum == 0d)
            {
                // TODO debug log speeds are zero
                return;
            }
            if (speedsTotalSum == _lastSpeedsTotalSum && perDevSpeedsTotalSum == _lastPerDevSpeedsTotalSum)
            {
                // TODO debug log speeds seem to be stuck
                return;
            }
            // update 
            _lastSpeedsTotalSum = speedsTotalSum;
            _lastPerDevSpeedsTotalSum = perDevSpeedsTotalSum;
            MinerApiWatchdog.UpdateApiTimestamp(GroupKey, DateTime.UtcNow);
        }
        #endregion MinerApiWatchdog


    }
}

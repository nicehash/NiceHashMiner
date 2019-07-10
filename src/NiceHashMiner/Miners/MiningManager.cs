using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners.Grouping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Benchmarking;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using NHM.Common.Enums;
using Timer = System.Timers.Timer;
using NHM.Common;
using NiceHashMiner.Miners.IntegratedPlugins;
using MinerPlugin;
using System.Threading;

namespace NiceHashMiner.Miners
{
    public static class MiningManager
    {
        private const string Tag = "MiningManager";
        private const string DoubleFormat = "F12";

        private static readonly AlgorithmSwitchingManager _switchingManager;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private static string _username = DemoUser.BTC;
        private static List<MiningDevice> _miningDevices = new List<MiningDevice>();
        private static Dictionary<string, Miner> _runningMiners = new Dictionary<string, Miner>();

        // assume profitable
        private static bool _isProfitable = true;
        // assume we have internet
        private static bool _isConnectedToInternet = true;


        public static bool IsMiningEnabled => _miningDevices.Count > 0;

        private static bool IsCurrentlyIdle => !IsMiningEnabled || !_isConnectedToInternet || !_isProfitable;

        static MiningManager()
        {
            ApplicationStateManager.OnInternetCheck += OnInternetCheck;

            _switchingManager = new AlgorithmSwitchingManager();
            _switchingManager.SmaCheck += SwichMostProfitableGroupUpMethod;
        }

        // For PRODUCTION
        public static List<int> GetActiveMinersIndexes()
        {
            var minerIDs = new List<int>();
            if (!IsCurrentlyIdle)
            {
                _semaphore.Wait();
                try
                {
                    foreach (var miner in _runningMiners.Values)
                    {
                        minerIDs.AddRange(miner.DevIndexes);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return minerIDs;
        }
        
        private static void OnInternetCheck(object sender, bool isConnectedToInternet)
        {
            _isConnectedToInternet = isConnectedToInternet;
        }


        #region Start/Stop

        public static async Task StopAllMiners()
        {
            EthlargementIntegratedPlugin.Instance.Stop();
            await _semaphore.WaitAsync();
            try
            {
                foreach (var groupMiner in _runningMiners.Values)
                {
                    groupMiner.End();
                }
                _runningMiners.Clear();

                //// TODO enable StabilityAnalyzer
                //// Speed stability analyzer was here or deviant algo checker
                //foreach (var miningDev in _miningDevices)
                //{
                //    var deviceUuid = miningDev.Device.Uuid;
                //    foreach (var algorithm in miningDev.Algorithms)
                //    {
                //        var speedID = $"{deviceUuid}-{algorithm.AlgorithmStringID}";
                //        var isDeviant = BenchmarkingAnalyzer.IsDeviant(speedID);
                //        if (isDeviant)
                //        {
                //            var stableSpeeds = BenchmarkingAnalyzer.GetStableSpeeds(speedID);
                //            if (stableSpeeds != null)
                //            {
                //                algorithm.Speeds = stableSpeeds;
                //            }
                //        }
                //    }
                //}
            }
            finally
            {
                _semaphore.Release();
            }

            _switchingManager?.Stop();
            ApplicationStateManager.ClearRatesAll();
            //_internetCheckTimer?.Stop();
        }

        public static async Task PauseAllMiners()
        {
            EthlargementIntegratedPlugin.Instance.Stop();
            await _semaphore.WaitAsync();
            try
            {
                foreach (var groupMiner in _runningMiners.Values)
                {
                    groupMiner.End();
                }
                _runningMiners.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
            ApplicationStateManager.ClearRatesAll();
        }

        #endregion Start/Stop

        // TODO make Task
        public static async Task UpdateMiningSession(IEnumerable<ComputeDevice> devices, string username)
        {
            _switchingManager?.Stop();
            await _semaphore.WaitAsync();
            try
            {
                _username = username;
                // TODO check out if there is a change
                _miningDevices = GroupSetupUtils.GetMiningDevices(devices, true);
                if (_miningDevices.Count > 0)
                {
                    GroupSetupUtils.AvarageSpeeds(_miningDevices);
                    //// TODO enable StabilityAnalyzer
                    //// set benchmarked speeds for BenchmarkingAnalyzer
                    //foreach (var miningDev in _miningDevices)
                    //{
                    //    var deviceUuid = miningDev.Device.Uuid;
                    //    foreach (var algorithm in miningDev.Algorithms)
                    //    {
                    //        var speedID = $"{deviceUuid}-{algorithm.AlgorithmStringID}";
                    //        var benchmarkSpeed = new BenchmarkingAnalyzer.BenchmarkSpeed {
                    //            PrimarySpeed = algorithm.BenchmarkSpeed,
                    //            SecondarySpeed = algorithm.SecondaryAveragedSpeed,
                    //        };
                    //        BenchmarkingAnalyzer.SetBenchmarkSpeeds(speedID, benchmarkSpeed);
                    //    }
                    //}
                }
            }
            finally
            {
                _semaphore.Release();
            }
            _switchingManager?.ForceUpdate();
        }

        public static async Task RestartMiners(string username)
        {
            _switchingManager?.Stop();
            await _semaphore.WaitAsync();
            try
            {
                _username = username;
                // STOP
                foreach (var key in _runningMiners.Keys)
                {
                    await _runningMiners[key].StopTask();
                }
                // START
                var miningLocation = StratumService.SelectedServiceLocation;
                foreach (var key in _runningMiners.Keys)
                {
                    await _runningMiners[key].StartTask(miningLocation, _username);
                }
            }
            finally
            {
                _semaphore.Release();
            }
            _switchingManager?.ForceUpdate();
        }


        // full of state
        private static bool CheckIfProfitable(double currentProfit, bool log = true)
        {
            if (ConfigManager.IsMiningRegardlesOfProfit) {
                if (log) Logger.Info(Tag, $"Mine always regardless of profit");
                return true;
            }

            // TODO FOR NOW USD ONLY
            var currentProfitUsd = (currentProfit * ExchangeRateApi.GetUsdExchangeRate());
            var minProfit = ConfigManager.GeneralConfig.MinimumProfit;
            _isProfitable = currentProfitUsd >= minProfit;
            if (log)
            {
                Logger.Info(Tag, $"Current global profit = {currentProfitUsd.ToString("F8")} USD/Day");
                if (!_isProfitable)
                {
                    Logger.Info(Tag, $"Current global profit = NOT PROFITABLE, MinProfit: {minProfit.ToString("F8")} USD/Day");
                }
                else
                {
                    var profitabilityInfo = minProfit.ToString("F8") + " USD/Day";
                    Logger.Info(Tag, $"Current global profit = IS PROFITABLE, MinProfit: {profitabilityInfo}");
                }
            }

            return _isProfitable;
        }

        private static bool CheckIfShouldMine(double currentProfit, bool log = true)
        {
            var isProfitable = CheckIfProfitable(currentProfit, log);

            ApplicationStateManager.SetProfitableState(isProfitable);
            ApplicationStateManager.DisplayNoInternetConnection(!_isConnectedToInternet);

            if (!_isConnectedToInternet && log) Logger.Info(Tag, $"No internet connection! Not able to mine.");
            
            // if profitable and connected to internet mine
            var shouldMine = isProfitable && _isConnectedToInternet;
            return shouldMine;
        }

        private static async void SwichMostProfitableGroupUpMethod(object sender, SmaUpdateEventArgs e)
        {
            await SwichMostProfitableGroupUpMethodTask(sender, e);
        }


        private static void CalculateAndUpdateMiningDevicesProfits(Dictionary<AlgorithmType, double> profits)
        {
            foreach (var device in _miningDevices)
            {
                // update/calculate profits
                device.CalculateProfits(profits);
            }
        }

        private static (double currentProfit, double prevStateProfit) GetCurrentAndPreviousProfits()
        {
            var currentProfit = 0.0d;
            var prevStateProfit = 0.0d;
            foreach (var device in _miningDevices)
            {
                // check if device has profitable algo
                if (device.HasProfitableAlgo())
                {
                    currentProfit += device.GetCurrentMostProfitValue;
                    prevStateProfit += device.GetPrevMostProfitValue;
                }
            }
            return (currentProfit, prevStateProfit);
        }

        private static List<MiningPair> GetProfitableMiningPairs()
        {
            var profitableMiningPairs = new List<MiningPair>();
            foreach (var device in _miningDevices)
            {
                // check if device has profitable algo
                if (!device.HasProfitableAlgo()) continue;
                profitableMiningPairs.Add(device.GetMostProfitablePair());
            }
            return profitableMiningPairs;
        }

        private static async Task SwichMostProfitableGroupUpMethodTask(object sender, SmaUpdateEventArgs e)
        {
            CalculateAndUpdateMiningDevicesProfits(e.NormalizedProfits);
            var (currentProfit, prevStateProfit) = GetCurrentAndPreviousProfits();

            // log profits scope
            {
                var stringBuilderFull = new StringBuilder();
                stringBuilderFull.AppendLine("Current device profits:");
                foreach (var device in _miningDevices)
                {
                    var stringBuilderDevice = new StringBuilder();
                    stringBuilderDevice.AppendLine($"\tProfits for {device.Device.Uuid} ({device.Device.GetFullName()}):");
                    foreach (var algo in device.Algorithms)
                    {
                        stringBuilderDevice.AppendLine(
                            $"\t\tPROFIT = {algo.CurrentProfit.ToString(DoubleFormat)}" +
                            $"\t(SPEED = {algo.AvaragedSpeed:e5}" +
                            $"\t\t| NHSMA = {algo.CurNhmSmaDataVal:e5})" +
                            $"\t[{algo.AlgorithmStringID}]"
                        );
                        if (algo is PluginAlgorithm dualAlg && dualAlg.IsDual)
                        {
                            stringBuilderDevice.AppendLine(
                                $"\t\t\t\t\t  Secondary:\t\t {dualAlg.SecondaryAveragedSpeed:e5}" +
                                $"\t\t\t\t  {dualAlg.SecondaryCurNhmSmaDataVal:e5}"
                            );
                        }
                    }
                    // most profitable
                    stringBuilderDevice.AppendLine(
                        $"\t\tMOST PROFITABLE ALGO: {device.GetMostProfitableString()}, PROFIT: {device.GetCurrentMostProfitValue.ToString(DoubleFormat)}");
                    stringBuilderFull.AppendLine(stringBuilderDevice.ToString());
                }
                Logger.Info(Tag, stringBuilderFull.ToString());
            }

            // check if should mine
            // Only check if profitable inside this method when getting SMA data, cheching during mining is not reliable
            if (CheckIfShouldMine(currentProfit) == false)
            {
                foreach (var device in _miningDevices)
                {
                    device.SetNotMining();
                }
                await PauseAllMiners();
                return;
            }

            // check profit threshold
            Logger.Info(Tag, $"PrevStateProfit {prevStateProfit}, CurrentProfit {currentProfit}");
            if (prevStateProfit > 0 && currentProfit > 0)
            {
                var a = Math.Max(prevStateProfit, currentProfit);
                var b = Math.Min(prevStateProfit, currentProfit);
                //double percDiff = Math.Abs((PrevStateProfit / CurrentProfit) - 1);
                var percDiff = ((a - b)) / b;
                if (percDiff < ConfigManager.GeneralConfig.SwitchProfitabilityThreshold)
                {
                    // don't switch
                    Logger.Info(Tag, $"Will NOT switch profit diff is {percDiff}, current threshold {ConfigManager.GeneralConfig.SwitchProfitabilityThreshold}");
                    // RESTORE OLD PROFITS STATE
                    foreach (var device in _miningDevices)
                    {
                        device.RestoreOldProfitsState();
                    }
                    return;
                }
                Logger.Info(Tag, $"Will SWITCH profit diff is {percDiff}, current threshold {ConfigManager.GeneralConfig.SwitchProfitabilityThreshold}");
            }

            await _semaphore.WaitAsync();
            var hasChanged = false;
            try
            {
                // group new miners 
                var newGroupedMiningPairs = GroupingUtils.GetGroupedMiningPairs(GetProfitableMiningPairs());
                // check newGroupedMiningPairs and running Groups to figure out what to start/stop and keep running
                var currentRunningGroups = _runningMiners.Keys.ToArray();
                // check which groupMiners should be stopped and which ones should be started and which to keep running
                var noChangeGroupMinersKeys = newGroupedMiningPairs.Where(pair => currentRunningGroups.Contains(pair.Key)).Select(pair => pair.Key).OrderBy(uuid => uuid).ToArray();
                var toStartMinerGroupKeys = newGroupedMiningPairs.Where(pair => !currentRunningGroups.Contains(pair.Key)).Select(pair => pair.Key).OrderBy(uuid => uuid).ToArray();
                var toStopMinerGroupKeys = currentRunningGroups.Where(runningKey => !newGroupedMiningPairs.Keys.Contains(runningKey)).OrderBy(uuid => uuid).ToArray();
                // first stop currently running
                foreach (var stopKey in toStopMinerGroupKeys)
                {
                    var stopGroup = _runningMiners[stopKey];
                    _runningMiners.Remove(stopKey);
                    await stopGroup.StopTask();
                    stopGroup.End();
                }
                // start new
                var miningLocation = StratumService.SelectedServiceLocation;
                foreach (var startKey in toStartMinerGroupKeys)
                {
                    var miningPairs = newGroupedMiningPairs[startKey];
                    var toStart = MinerFactory.CreateMinerForMining(miningPairs, startKey);
                    if (toStart == null)
                    {
                        Logger.Error(Tag, $"CreateMinerForMining for key='{startKey}' returned <null>");
                        continue;
                    }
                    _runningMiners[startKey] = toStart;
                    await toStart.StartTask(miningLocation, _username);
                }
                // log scope
                {
                    var stopLog = toStopMinerGroupKeys.Length > 0 ? string.Join(",", toStopMinerGroupKeys) : "EMTPY";
                    Logger.Info(Tag, $"Stop Old Mining: ({stopLog})");
                    var startLog = toStartMinerGroupKeys.Length > 0 ? string.Join(",", toStartMinerGroupKeys) : "EMTPY";
                    Logger.Info(Tag, $"Start New Mining : ({startLog})");
                    var sameLog = noChangeGroupMinersKeys.Length > 0 ? string.Join(",", noChangeGroupMinersKeys) : "EMTPY";
                    Logger.Info(Tag, $"No change : ({sameLog})");
                }
                hasChanged = toStartMinerGroupKeys.Length > 0 || toStopMinerGroupKeys.Length > 0;
            }
            finally
            {
                _semaphore.Release();
            }

            
            // There is a change in groups, change GUI
            if (hasChanged)
            {
                ApplicationStateManager.ClearRatesAll();
                await MinerStatsCheck();
            }
        }

        public static async Task MinerStatsCheck()
        {
            await _semaphore.WaitAsync();
            try
            {
                foreach (var m in _runningMiners.Values)
                {
                    // skip if not running or if await already in progress
                    if (!m.IsRunning || m.IsUpdatingApi) continue;
                    var ad = m.GetSummaryAsync();
                }
                // Update GUI
                ApplicationStateManager.RefreshRates();
                // now we shoud have new global/total rate display it
                var kwhPriceInBtc = ExchangeRateApi.GetKwhPriceInBtc();
                var profitInBTC = MiningStats.GetProfit(kwhPriceInBtc);
                ApplicationStateManager.DisplayTotalRate(profitInBTC);
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"Error occured while getting mining stats: {e.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
            // TODO put this somewhere else maybe
            await RestartStagnatedMiners();
        }

        private static async Task RestartStagnatedMiners()
        {
            var restartGroups = MinerApiWatchdog.GetTimedoutGroups(DateTime.UtcNow);
            if (restartGroups == null) return;
            await _semaphore.WaitAsync();
            try
            {
                var miningLocation = StratumService.SelectedServiceLocation;
                foreach (var groupKey in restartGroups)
                {
                    if (_runningMiners.ContainsKey(groupKey) == false) continue;

                    Logger.Info(Tag, $"Restarting miner group='{groupKey}' API timestamp exceeded");
                    await _runningMiners[groupKey].StopTask();
                    await _runningMiners[groupKey].StartTask(miningLocation, _username);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

using MinerPlugin;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using System.Diagnostics;

namespace MinerPluginToolkitV1
{
    // TODO there is no watchdog
    public abstract class MinerBase : IMiner, IBinAndCwdPathsGettter
    {
        private static ulong _MINER_COUNT_ID = 0;
        protected string _logGroup { get; private set; }
        protected string _uuid { get; private set; }
        protected Process _miningProcess;
        protected IEnumerable<MiningPair> _miningPairs;
        protected string _miningLocation;
        protected string _username;
        protected string _password;

        public MinerBase(string uuid)
        {
            _MINER_COUNT_ID++;
            _uuid = uuid;
            _logGroup = $"Miner{_MINER_COUNT_ID}-{uuid}";
        }

        // if stop is called then consider this miner obsolete
        CancellationTokenSource _stopMiner { get; } = new CancellationTokenSource();
        protected object _lock = new object();
        protected bool _stopCalled = false;

        public MinerOptionsPackage MinerOptionsPackage { get; set; }
        public MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables { get; set; }

        abstract public Task<ApiData> GetMinerStatsDataAsync();

        abstract protected void Init();

        public void InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            _miningPairs = miningPairs;
            //// update log group
            //_logGroup += _miningPairs.
            Init();
        }

        public void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x")
        {
            _miningLocation = miningLocation;
            _username = username;
            _password = password;
        }

        abstract public Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard);

        abstract public Tuple<string, string> GetBinAndCwdPaths();
        abstract protected string MiningCreateCommandLine();

        // most don't require extra enviorment vars
        protected virtual Dictionary<string, string> GetEnvironmentVariables() => null;



        // TODO check mining pairs
        public void StartMining()
        {
            try
            {
                if (this is IBeforeStartMining bsm)
                {
                    bsm.BeforeStartMining();
                }
                var binPathBinCwdPair = GetBinAndCwdPaths();
                var binPath = binPathBinCwdPair.Item1;
                var binCwd = binPathBinCwdPair.Item2;
                var commandLine = MiningCreateCommandLine();
                var environmentVariables = GetEnvironmentVariables();

                // Logging
                Logger.Info(_logGroup, $"Starting miner commandLine='{commandLine}'");
                // TODO this will not print content
                var environmentVariablesLog = environmentVariables == null ? "<null>" : string.Join(";", environmentVariables.Select(x => x.Key + "=" + x.Value));
                Logger.Info(_logGroup, $"Starting miner environmentVariables='{environmentVariablesLog}'"); // TODO log or debug???

                _miningProcess = MinerToolkit.CreateMiningProcess(binPath, binCwd, commandLine, environmentVariables);
                _miningProcess.Exited += MinerProcess_Exited;
                if (!_miningProcess.Start())
                {
                    Logger.Info(_logGroup, $"Error occured while starting a new process: {_miningProcess.ToString()}");
                    throw new InvalidOperationException("Could not start process: " + _miningProcess);
                }

                if (this is IAfterStartMining asm)
                {
                    asm.AfterStartMining();
                }
            }
            finally
            {
                lock (_lock)
                {
                    if (_stopCalled || _stopMiner.IsCancellationRequested)
                    try
                    {
                            StopMining();
                    }
                    catch
                    {

                    }
                }
            }
        }

        public virtual void StopMining()
        {
            lock (_lock)
            {
                try
                {
                    _stopCalled = true;
                    _stopMiner.Cancel();
                }
                catch
                {

                }
            }

            try
            {
                // remove on exited
                if (_miningProcess != null) _miningProcess.Exited -= MinerProcess_Exited;
                _miningProcess?.CloseMainWindow();
                // 5 seconds wait for shutdown
                if (!_miningProcess?.WaitForExit(5 * 1000) ?? false)
                {
                    _miningProcess?.Kill(); // TODO look for another gracefull shutdown
                }
            }
            catch (Exception e)
            {
                Logger.Info(_logGroup, $"Error occured while stopping the process: {e.Message}");
            }
        }

        private async void MinerProcess_Exited(object sender, EventArgs e)
        {
            await RestartMining();
        }

        // TODO consider using cancelation token instead of boolean stop 
        protected async Task RestartMining()
        {
            var isRestrat = false;
            try
            {
                await Task.Delay(500, _stopMiner.Token);
                isRestrat = !_stopMiner.IsCancellationRequested;
            }
            catch(Exception e)
            {
                Logger.Error(_logGroup, $"RestartMining error: {e.Message}");
            }
            finally
            {
                if (isRestrat)
                {
                    StartMining();
                }
            }
        }
    }
}

using MinerPlugin;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Algorithm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;

namespace MinerPluginToolkitV1
{
    // TODO there is no watchdog
    public abstract class MinerBase : IMiner, IBinAndCwdPathsGettter
    {
        private readonly string _uuid;
        protected MiningProcess _miningProcess;
        protected IEnumerable<MiningPair> _miningPairs;
        protected string _miningLocation;
        protected string _username;
        protected string _password;

        // if stop is called then consider this miner obsolete
        protected object _lock = new object();
        protected bool _stopCalled = false;

        public MinerOptionsPackage MinerOptionsPackage { get; set; }
        public MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables { get; set; }

        abstract public Task<ApiData> GetMinerStatsDataAsync();

        abstract protected void Init();

        public void InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            _miningPairs = miningPairs;
            Init();
        }

        public void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x")
        {
            _miningLocation = miningLocation;
            _username = username;
        }

        abstract public Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard);

        abstract public Tuple<string, string> GetBinAndCwdPaths();
        abstract protected string MiningCreateCommandLine();

        // most don't require extra enviorment vars
        protected virtual Dictionary<string, string> GetEnvironmentVariables() => null;



        // TODO check mining pairs
        public void StartMining()
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
            var p = MinerToolkit.CreateMiningProcess(binPath, binCwd, commandLine, environmentVariables);
            _miningProcess = new MiningProcess(p);
            p.Exited += MinerProcess_Exited;
            if (!p.Start())
            {
                throw new InvalidOperationException("Could not start process: " + p);
            }

            if (this is IAfterStartMining asm)
            {
                asm.AfterStartMining();
            }
        }

        public virtual void StopMining()
        {
            lock (_lock)
            {
                if (_stopCalled) return;
            }
            try
            {
                // remove on exited
                _miningProcess.Handle.Exited -= MinerProcess_Exited;
                lock (_lock)
                {
                    _stopCalled = true;
                }
                _miningProcess?.Handle?.CloseMainWindow();
                // 5 seconds wait for shutdown
                if (!_miningProcess?.Handle?.WaitForExit(5 * 1000) ?? false)
                {
                    _miningProcess?.Handle?.Kill(); // TODO look for another gracefull shutdown
                }
            }
            catch (Exception e)
            {
            }
        }

        private async void MinerProcess_Exited(object sender, EventArgs e)
        {
            await RestartMining();
        }

        // TODO consider using cancelation token instead of boolean stop 
        protected async Task RestartMining()
        {
            if (_stopCalled) return;
            await Task.Delay(500);
            StartMining();
        }
    }
}

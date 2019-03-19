using MinerPlugin;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Algorithm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;

namespace MinerPluginToolkitV1
{
    // TODO there is no watchdog
    public abstract class MinerBase : IMiner
    {
        private readonly string _uuid;
        protected MiningProcess _miningProcess;
        protected IEnumerable<(BaseDevice device, Algorithm algorithm)> _miningPairs;
        protected string _miningLocation;
        protected string _username;
        protected string _password;

        // if stop is called then consider this miner obsolete
        protected object _lock = new object();
        protected bool _stopCalled = false;

        public MinerOptionsPackage MinerOptionsPackage { get; set; }

        abstract public Task<ApiData> GetMinerStatsDataAsync();

        abstract protected void Init();

        public void InitMiningPairs(IEnumerable<(BaseDevice, Algorithm)> miningPairs)
        {
            _miningPairs = miningPairs;
            Init();
        }

        public void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x")
        {
            _miningLocation = miningLocation;
            _username = username;
        }

        abstract public Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard);

        abstract protected (string binPath, string binCwd) GetBinAndCwdPaths();
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

            var (binPath, binCwd) = GetBinAndCwdPaths();
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
            // remove on exited
            _miningProcess.Handle.Exited -= MinerProcess_Exited;
            lock (_lock)
            {
                _stopCalled = true;
            }
            _miningProcess?.Handle?.Kill(); // TODO look for another gracefull shutdown
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

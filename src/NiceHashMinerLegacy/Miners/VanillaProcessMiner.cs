using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public abstract class VanillaProcessMiner : Miner
    {
        private Process _process;

        protected VanillaProcessMiner(string minerDeviceName) : base(minerDeviceName)
        { }

        // BMiner throws a fit if started with NiceHashProcess so use System.Diagnostics.Process instead
        // WARNING ProcessHandle will be null so do not call methods that access it (currently _Stop() is the only
        // one and it is overridden here)
        // TODO is NiceHashProcess necessary or can we use System.Diagnostics.Process everywhere?
        protected override NiceHashProcess _Start(IReadOnlyDictionary<string, string> envVariables = null)
        {
            if (_isEnded)
            {
                return null;
            }

            _process = new Process();

            Ethlargement.CheckAndStart(MiningSetup);

            var nhmlDirectory = Directory.GetCurrentDirectory();
            _process.StartInfo.WorkingDirectory = System.IO.Path.Combine(nhmlDirectory, WorkingDirectory);
            _process.StartInfo.FileName = System.IO.Path.Combine(nhmlDirectory, Path);
            _process.StartInfo.Arguments = LastCommandLine;
            _process.Exited += (sender, args) =>
            {
                Miner_Exited();
            };
            _process.EnableRaisingEvents = true;
            _process.StartInfo.CreateNoWindow = ConfigManager.GeneralConfig.HideMiningWindows;

            _process.StartInfo.UseShellExecute = false;

            try
            {
                if (_process.Start())
                {
                    IsRunning = true;

                    _currentPidData = new MinerPidData
                    {
                        MinerBinPath = Path,
                        Pid = _process.Id
                    };
                    _allPidData.Add(_currentPidData);

                    Helpers.ConsolePrint(MinerTag(), "Starting miner " + ProcessTag() + " " + LastCommandLine);

                    StartCoolDownTimerChecker();
                }
                else
                {
                    Helpers.ConsolePrint(MinerTag(), "NOT STARTED " + ProcessTag() + " " + LastCommandLine);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " _Start: " + ex.Message);
            }

            return null;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            if (IsRunning)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Shutting down miner");
            }

            if (_process == null) return;

            try
            {
                _process.Kill();
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint(MinerTag(), e.Message);
            }

            _process.Close();
            _process = null;
        }
    }
}

using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NiceHashMiner.Miners
{
    public static class Ethlargement
    {
        private static Process _process;

        private static MiningSetup _cachedSetup;

        private static readonly object Lock = new object();

        public static bool Running => _process != null && !_process.HasExited;

        public static void CheckAndStart(MiningSetup setup)
        {
            lock (Lock)
            {
                _cachedSetup = setup;
                if (!ShouldRun(setup)) return;
                _process = new Process
                {
                    StartInfo =
                    {
                        FileName = MinerPaths.Data.EthLargement,
                        //CreateNoWindow = false
                    }
                };

                if (ConfigManager.GeneralConfig.HideMiningWindows || ConfigManager.GeneralConfig.MinimizeMiningWindows)
                {
                    _process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    //_process.StartInfo.UseShellExecute = true;
                }

                //e.StartInfo.UseShellExecute = false;
                _process.EnableRaisingEvents = true;
                _process.Exited += ExitEvent;

                try
                {
                    if (_process.Start())
                    {
                        Helpers.ConsolePrint("ETHLARGEMENT", "Starting ethlargement...");
                    }
                    else
                    {
                        Helpers.ConsolePrint("ETHLARGEMENT", "Couldn't start ethlargement");
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("ETHLARGEMENT", ex.Message);
                }
            }
        }

        private static void ExitEvent(object sender, EventArgs e)
        {
            Thread.Sleep(1000);
            CheckAndStart(_cachedSetup);
        }

        public static void Stop()
        {
            _cachedSetup = null;
            try
            {
                _process.CloseMainWindow();
                if (!_process.WaitForExit(10 * 1000))
                {
                    _process.Kill();
                }

                _process.Close();
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("ETHLARGEMENT", e.Message);
            }

            _process = null;
        }

        private static bool ShouldRun(MiningSetup setup)
        {
            if (Running || ConfigManager.GeneralConfig.Use3rdPartyMiners != Use3rdPartyMiners.YES ||
                setup == null)
                return false;

            return setup.MiningPairs.Any(p => p.CurrentExtraLaunchParameters.Contains("--ethlargement"));

            foreach (var p in setup.MiningPairs)
            {
                if (p.Algorithm.NiceHashID == AlgorithmType.DaggerHashimoto &&
                    p.Device is CudaComputeDevice cuda &&
                    cuda.ShouldRunEthlargement)
                    return true;
            }

            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public static class Ethlargement
    {
        private static int _pid = -1;

        private static object _lock = new object();

        public static bool Running => _pid != -1;

        public static void CheckAndStart(MiningSetup setup)
        {
            lock (_lock)
            {
                if (!ShouldRun(setup)) return;

                // Run ethlargement
                var e = new NiceHashProcess
                {
                    StartInfo =
                    {
                        FileName = MinerPaths.Data.EthLargement,
                        CreateNoWindow = false
                    }
                };

                if (ConfigManager.GeneralConfig.HideMiningWindows || ConfigManager.GeneralConfig.MinimizeMiningWindows)
                {
                    e.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    e.StartInfo.UseShellExecute = true;
                }

                e.StartInfo.UseShellExecute = false;

                try
                {
                    if (e.Start())
                    {
                        Helpers.ConsolePrint("ETHLARGEMENT", "Starting ethlargement...");

                        _pid = e.Id;
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

        public static void Stop()
        {

        }

        private static bool ShouldRun(MiningSetup setup)
        {
            if (Running || ConfigManager.GeneralConfig.Use3rdPartyMiners != Use3rdPartyMiners.YES)
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

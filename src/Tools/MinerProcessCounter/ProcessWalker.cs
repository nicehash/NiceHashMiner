using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MinerProcessCounter
{
    internal class PsInfo
    {
        public string ProcessName { get; set; }
        public string FileName { get; set; }
        public string Arguments { get; set; }
    }
    internal static class ProcessWalker
    {
        public static List<string> nameFilters = new List<string> {
            "excavator",
            "ccminer",
            "ethminer",
            "nheqminer",
            "sgminer",
            "xmr-stak",
            "NsGpuCNMiner",
            "EthMan",
            "EthDcrMiner64",
            "ZecMiner64",
            "zm",
            "OhGodAnETHlargementPill-r2",
            "miner",
            "Optiminer",
            "PhoenixMiner",
            "prospector",
            "t-rex",
            "TT-Miner",
            "nbminer",
            "teamredminer",
            "nanominer",
            "wildrig",
            "miniZ",
            "cpuminer-avx2",
            "cpuminer-zen",
            "CryptoDredge",
            "lolMiner",
            "xmrig",
            "z-enemy"
        };

        private static bool isFilterIncluded(string psName)
        {
            return nameFilters.Any(filter => psName.Contains(filter));
        }

        public static IEnumerable<PsInfo> ListRunning()
        {
            var snapshot = Process.GetProcesses();
            var filtered = snapshot.Where(p => isFilterIncluded(p.ProcessName));
            var mapped = filtered.Select(p => new PsInfo()
            {
                ProcessName = p.ProcessName,
                FileName = p.StartInfo.FileName,
                Arguments = p.StartInfo.Arguments,
            });
            return mapped;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
            "sgminer",
            "ccminer",
            "ccminer",
            "ccminer",
            "ccminer",
            "ccminer",
            "ccminer",
            "ccminer",
            "ccminer",
            "ccminer",
            "cpuminer-aes-sse42",
            "cpuminer-avx",
            "cpuminer-avx2-sha",
            "cpuminer-sse2",
            "cpuminer",
            "ethminer",
            "nheqminer",
            "sgminer",
            "sgminer",
            "xmr-stak",
            "xmr-stak",
            "xmrig",
            "NsGpuCNMiner",
            "EthMan",
            "EthDcrMiner64",
            "EthDcrMiner64",
            "EthDcrMiner64",
            "EthDcrMiner64",
            "EthDcrMiner64",
            "EthMan",
            "EthMan",
            "ZecMiner64",
            "zm",
            "OhGodAnETHlargementPill-r2",
            "miner",
            "miner",
            "Optiminer",
            "PhoenixMiner",
            "prospector",
            "t-rex",
            "TT-Miner",
            "nbminer",
            "teamredminer",
            "nanominer",
            "wildrig"
        };

        private static bool isFilterIncluded(string psName) {
            return nameFilters.Any(filter => psName.Contains(filter));
        }

        public static IEnumerable<PsInfo> ListRunning() {
            var snapshot = Process.GetProcesses();
            var filtered = snapshot.Where(p => isFilterIncluded(p.ProcessName));
            var mapped = filtered.Select(p => new PsInfo() {
                ProcessName = p.ProcessName,
                FileName = p.StartInfo.FileName,
                Arguments = p.StartInfo.Arguments,
            });
            return mapped;
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MP.Joker
{
    internal static class PluginEngines
    {
        public enum PluginEngine
        {
            Unknown = 0,
            CryptoDredge,
            GMiner,
            LolMiner, // signed
            MiniZ,
            NanoMiner,
            NBMiner, // signed
            Phoenix,
            SRBMiner,
            TeamRedMiner,
            TRex,
            TTMiner,
            WildRig,
            XMRig,
            ZEnemy,
        }

        private static readonly Dictionary<PluginEngine, string> MinerExecutableNames = new Dictionary<PluginEngine, string>
        {
            { PluginEngine.CryptoDredge, "CryptoDredge"},
            { PluginEngine.GMiner, "miner"},
            { PluginEngine.LolMiner, "lolMiner"},
            { PluginEngine.MiniZ, "miniZ"},
            { PluginEngine.NanoMiner, "nanominer"},
            { PluginEngine.NBMiner, "nbminer"},
            { PluginEngine.Phoenix, "PhoenixMiner"},
            { PluginEngine.SRBMiner, "SRBMiner"},
            { PluginEngine.TeamRedMiner, "teamredminer"},
            { PluginEngine.TRex, "t-rex"},
            { PluginEngine.TTMiner, "TT-Miner"},
            { PluginEngine.WildRig, "wildrig"},
            { PluginEngine.XMRig, "xmrig"},
            { PluginEngine.ZEnemy, "z-enemy"},
        };

        public static PluginEngine GuessMinerBinaryPluginEngine(string minerBinary)
        {
            return MinerExecutableNames
                .Where(kvp => minerBinary.StartsWith(kvp.Value))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
        }
    }
}

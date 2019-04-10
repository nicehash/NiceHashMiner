using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class CCMinerIntegratedMiner : MinerPluginToolkitV1.CCMinerCommon.CCMinerBase
    {
        public CCMinerIntegratedMiner(string uuid, string dirPath) : base(uuid)
        {
            _dirPath = dirPath;
            _noTimeLimitOption = "ccminer_klaust" == dirPath;
        }
        protected readonly string _dirPath;


        protected override string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.NeoScrypt: return "neoscrypt";
                case AlgorithmType.Blake2s: return "blake2s";
                case AlgorithmType.Keccak: return "keccak";
                case AlgorithmType.Skunk: return "skunk";
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.Lyra2REv3: return "lyra2v3";
                case AlgorithmType.MTP: return "mtp";
            }
            // TODO throw exception
            return "";
        }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin", _dirPath);
            var binPath = Path.Combine(binCwd, "ccminer.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}

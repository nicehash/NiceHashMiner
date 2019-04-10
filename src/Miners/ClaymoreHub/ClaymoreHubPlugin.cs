using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClaymoreHub
{

    public class ClaymoreHubPlugin : IMinerPlugin, IInitInternals
    {
        public string PluginUUID => "5e3b699e-2755-499c-bf4e-20d4aaef73df";

        public Version Version => new Version(1, 0);

        public string Name => "ClaymoreHub";

        public string Author => "Domen Kirn Krefl";


        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            var minDrivers = new Version(398, 26);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        //TODO how to set dual algorithms?? like this?
        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerDecred);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerBlake2s);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerKeccak);
        }
        private IEnumerable<Algorithm> GetSupportedAlgorithms(AMDDevice gpu)
        {
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerDecred);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerBlake2s);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerKeccak);
        }

        public IMiner CreateMiner()
        {
            return new Claymore(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Ethereum intensity. Default value is 8, you can decrease this value if you don't want Windows to freeze or if you have problems with stability. The most low GPU load is "-ethi 0".
	            ///Also "-ethi" can set intensity for every card individually, for example "-ethi 1,8,6".
                ///You can also specify negative values, for example, "-ethi -8192", it exactly means "global work size" parameter which is used in official miner.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_intensity_primary",
                    ShortName = "-ethi",
                    DefaultValue = "8",
                    Delimiter = ","
                },
                /// <summary>
                /// Decred/Siacoin/Lbry/Pascal intensity, or Ethereum fine-tuning value in ETH-only ASM mode. Default value is 30, you can adjust this value to get the best Decred/Siacoin/Lbry mining speed without reducing Ethereum mining speed. 
	            ///You can also specify values for every card, for example "-dcri 30,100,50".
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_intensity_secondary",
                    ShortName = "-dcri",
                    DefaultValue = "30",
                    Delimiter = ","
                },
                /// <summary>
                /// low intensity mode. Reduces mining intensity, useful if your cards are overheated. Note that mining speed is reduced too. 
	            /// More value means less heat and mining speed, for example, "-li 10" is less heat and mining speed than "-li 1". You can also specify values for every card, for example "-li 3,10,50".
                /// Default value is "0" - no low intensity mode.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_lowIntensity",
                    ShortName = "-li",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// set "1" to cancel my developer fee at all. In this mode some optimizations are disabled so mining speed will be slower by about 3%. 
	            /// By enabling this mode, I will lose 100% of my earnings, you will lose only about 2% of your earnings.
                /// So you have a choice: "fastest miner" or "completely free miner but a bit slower".
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "claymoreDual_noFee",
                    ShortName = "-nofee",
                    DefaultValue = "0",
                },
            }
        };
        #endregion Internal Settings
    }

}

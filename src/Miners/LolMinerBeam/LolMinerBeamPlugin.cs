using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;

namespace LolMinerBeam
{
    class LolMinerBeamPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeout
    {
        public Version Version => new Version(1, 3);

        public string Name => "LolMinerBeam";

        public string Author => "domen.kirnkrefl@nicehash.com";

        public string PluginUUID => "435f0820-7237-11e9-b20c-f9f12eb6d835";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 2).Cast<CUDADevice>();
            var openCLGpus = devices.Where(dev => dev is AMDDevice).Cast<AMDDevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedCUDAAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            foreach (var gpu in openCLGpus)
            {
                var algos = GetSupportedAMDAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedCUDAAlgorithms(CUDADevice dev)
        {
            const ulong minBeamMem = 4UL << 30;

            if (dev.GpuRam >= minBeamMem)
                yield return new Algorithm(PluginUUID, AlgorithmType.Beam);
        }

        private IEnumerable<Algorithm> GetSupportedAMDAlgorithms(AMDDevice dev)
        {
            const ulong minBeamMem = 4UL << 30;

            if (dev.GpuRam >= minBeamMem)
                yield return new Algorithm(PluginUUID, AlgorithmType.Beam);
        }

        public IMiner CreateMiner()
        {
            return new LolMinerBeam(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
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

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;

            var fileMinerReservedPorts = InternalConfigs.InitMinerReservedPorts(pluginRoot, _minerReservedApiPorts);
            if (fileMinerReservedPorts != null) _minerReservedApiPorts = fileMinerReservedPorts;
        }

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// When setting this parameter to 1, lolMiner will replace the “submitting share / share accepted” message pair by * symbols at the short statistics interval output.
                /// Every star stands for an accepted share.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_compactNotification",
                    ShortName = "--shortaccept",
                    DefaultValue = "0"
                },
                /// <summary>
                /// This parameter can be used to fix the sol/s output of a GPU to a fixed number of digits after the decimal delimiter.
                /// For example “DIGITS” : 0 will chop of all digits after the decimal delimiter. 
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_decimalDigits",
                    ShortName = "--digits",
                    DefaultValue = "0"
                },
                /// <summary>
                /// This parameter can be used to set a new location for the kernel directory. Absolute path are allowed, so you can freely place it when needed.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_kernelsLocation",
                    ShortName = "--kernelsdir",
                    DefaultValue = "./kernels"
                }
            }
        };
        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };
        #endregion Internal Settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "lolMiner.exe" });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }

        public TimeSpan GetApiMaxTimeout()
        {
            return new TimeSpan(0, 5, 0);
        }
    }
}

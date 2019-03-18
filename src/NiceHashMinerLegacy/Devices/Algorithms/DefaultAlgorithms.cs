using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Devices.Algorithms
{
    // TODO add checks for minimum drivers versions
    public static class DefaultAlgorithms
    {
        #region Limitations and filters
        const ulong MinDaggerHashimotoMemory = 3UL << 30; // 3GB
        const ulong MinZHashMemory = 1879047230; // 1.75GB
        const ulong MinBeamMemory = 3113849695; // 2.9GB
        const ulong MinGrinCuckaroo29Memory = 6012951136; // 5.6GB
        const ulong MinGrin31Mem = 8UL << 30; // 8GB

        private static readonly Dictionary<AlgorithmType, ulong> minMemoryPerAlgo = new Dictionary<AlgorithmType, ulong>
        {
            { AlgorithmType.DaggerHashimoto, MinDaggerHashimotoMemory },
            { AlgorithmType.ZHash, MinZHashMemory},
            { AlgorithmType.Beam, MinBeamMemory },
            { AlgorithmType.GrinCuckaroo29, MinGrinCuckaroo29Memory },
            { AlgorithmType.GrinCuckatoo31, MinGrin31Mem },
        };

        private static List<AlgorithmType> InsufficientDeviceMemoryAlgorithnms(ulong Ram, IEnumerable<AlgorithmType> algos)
        {
            var filterAlgorithms = new List<AlgorithmType>();
            foreach (var algo in algos)
            {
                if (minMemoryPerAlgo.ContainsKey(algo) == false) continue;
                var minRam = minMemoryPerAlgo[algo];
                if (Ram < minRam) filterAlgorithms.Add(algo);
            }
            return filterAlgorithms;
        }

        private static List<Algorithm> FilterAlgorithmsList(List<Algorithm> algos, IEnumerable<AlgorithmType> filterAlgos)
        {
            return algos.Where(a => filterAlgos.Contains(a.NiceHashID) == false).ToList();
        }

        private static List<Algorithm> FilterInsufficientRamAlgorithmsList(ulong Ram, List<Algorithm> algos)
        {
            var filterAlgos = InsufficientDeviceMemoryAlgorithnms(Ram, algos.Select(a => a.NiceHashID));
            return FilterAlgorithmsList(algos, filterAlgos);
        }

        #endregion Limitations and filters

        // ALL CPU, NVIDIA and AMD
        private static List<Algorithm> XmrStakAlgorithmsForDevice(ComputeDevice dev)
        {
            // multiple OpenCL GPUs seem to freeze the whole system
            var CryptoNightR_Enabled = dev.DeviceType != DeviceType.AMD;
            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNightHeavy, "cryptonight_heavy"),
                new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNightV8, "cryptonight_v8"),
                new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNightR, "cryptonight_r") { Enabled = CryptoNightR_Enabled },
            };
            return algos;
        }

        // NVIDIA and AMD
        private static List<Algorithm> ClaymoreDualAlgorithmsForDevice(ComputeDevice dev)
        {
            if (dev.DeviceType == DeviceType.CPU) return null;
            // SM5.0+
            if (dev is CudaComputeDevice cudaDev && cudaDev.SMMajor < 5) return null;

            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto),
                // duals disabled by default
                #pragma warning disable 0618
                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred) { Enabled = false },
                #pragma warning restore 0618
                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s) { Enabled = false },
                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak) { Enabled = false }
            };
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // NVIDIA and AMD
        private static List<Algorithm> PhoenixAlgorithmsForDevice(ComputeDevice dev)
        {
            if (dev.DeviceType == DeviceType.CPU) return null;
            // SM5.0+
            if (dev is CudaComputeDevice cudaDev && cudaDev.SMMajor < 5) return null;

            var algos = new List<Algorithm> {
                new Algorithm(MinerBaseType.Phoenix, AlgorithmType.DaggerHashimoto)
            };
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // NVIDIA and AMD
        private static List<Algorithm> GMinerAlgorithmsForDevice(ComputeDevice dev)
        {
            if (dev.DeviceType == DeviceType.CPU) return null;

            var algos = new List<Algorithm>();
            // CUDA 5.0+
            if (dev is CudaComputeDevice cudaDev && cudaDev.SMMajor >= 5)
            {
                algos = new List<Algorithm>
                {
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.ZHash),
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Beam),
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.GrinCuckaroo29),
                };
            }
            // AMD only allow Gcn4+
            if (dev is AmdComputeDevice amdDev && amdDev.IsGcn4 && dev.GpuRam >= MinBeamMemory)
            {
                algos = new List<Algorithm>
                {
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Beam),
                };
            }
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // AMD
        private static List<Algorithm> sgminerAlgorithmsForDevice(ComputeDevice dev)
        {
            if (dev.DeviceType != DeviceType.AMD) return null;

            const string RemDis = " --remove-disabled";
            const string DefaultParam = RemDis + AmdGpuDevice.DefaultParam;

            var NeoScryptExtraLaunchParameters = DefaultParam + "--nfactor 10 --xintensity    2 --thread-concurrency 8192 --worksize  64 --gpu-threads 4";
            if (!dev.Codename.Contains("Tahiti"))
            {
                NeoScryptExtraLaunchParameters =
                    AmdGpuDevice.DefaultParam +
                    "--nfactor 10 --xintensity    2 --thread-concurrency 8192 --worksize  64 --gpu-threads 2";
                Helpers.ConsolePrint("ComputeDevice", $"The GPU detected ({dev.Codename}) is not Tahiti. Changing default gpu-threads to 2.");
            }

            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.sgminer, AlgorithmType.NeoScrypt, "neoscrypt", false)
                {
                    ExtraLaunchParameters = NeoScryptExtraLaunchParameters
                },
                new Algorithm(MinerBaseType.sgminer, AlgorithmType.Keccak, "keccak")
                {
                    ExtraLaunchParameters = DefaultParam + "--intensity 15"
                },
                new Algorithm(MinerBaseType.sgminer, AlgorithmType.DaggerHashimoto, "ethash", false)
                {
                    ExtraLaunchParameters = RemDis + "--xintensity 512 -w 192 -g 1"
                },
                new Algorithm(MinerBaseType.sgminer, AlgorithmType.X16R, "x16r")
                {
                    ExtraLaunchParameters = "-X 256"
                },
            };

            // filter drivers algos issue
            if (dev.DriverDisableAlgos)
            {
                algos = FilterAlgorithmsList(algos, new List<AlgorithmType>{ AlgorithmType.NeoScrypt });
            }
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);

            return algos;
        }

        //// AMD disable this
        //private static List<Algorithm> ProspectorAlgorithmsForDevice(ComputeDevice dev)
        //{
        //    if (dev.DeviceType != DeviceType.AMD) return null;

        //    var algos = new List<Algorithm>
        //    {
        //        new Algorithm(MinerBaseType.Prospector, AlgorithmType.Skunk, "sigt", false),
        //    };
        //    // filter RAM requirements
        //    algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
        //    return algos;
        //}

        // NVIDIA
        private static List<Algorithm> ccminerAlgorithmsForDevice(ComputeDevice dev)
        {
            var cudaDev = dev as CudaComputeDevice;
            // CUDA SM3.0+
            if (cudaDev == null || cudaDev.SMMajor < 3) return null;

            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.ccminer, AlgorithmType.NeoScrypt, "neoscrypt"),
                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Blake2s, "blake2s"),
                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Keccak, "keccak"),
                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Skunk, "skunk"),
                new Algorithm(MinerBaseType.ccminer, AlgorithmType.X16R, "x16r"),
                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Lyra2REv3, "lyra2v3"),
                new Algorithm(MinerBaseType.ccminer, AlgorithmType.MTP, "mtp") { Enabled = false },
                // ccminer_alexis unstable disable it by default
                new Algorithm(MinerBaseType.ccminer_alexis, AlgorithmType.Keccak, "keccak") { Enabled = false }
            };
            if (cudaDev.SMMajor == 3)
            {
                // filter NeoScrypt and Lyra2REv3
                algos = FilterAlgorithmsList(algos, new List<AlgorithmType> { AlgorithmType.NeoScrypt, AlgorithmType.Lyra2REv3 });
            }
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // NVIDIA - TODO can also support AMD
        private static List<Algorithm> ethminerAlgorithmsForDevice(ComputeDevice dev)
        {
            var cudaDev = dev as CudaComputeDevice;
            // CUDA SM3.0+
            if (cudaDev == null || cudaDev.SMMajor < 3 ) return null;
            if (dev.Name.Contains("750") && dev.Name.Contains("Ti")) {
                Helpers.ConsolePrint("DefaultAlgorithms-ethminer",
                        "GTX 750Ti found! By default this device will be disabled for ethereum as it is generally too slow to mine on it.");
                return null;
            }
            

            const bool enabledByDefault = false;
            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.ethminer, AlgorithmType.DaggerHashimoto, "daggerhashimoto") {Enabled = enabledByDefault }
            };
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // NVIDIA
        private static List<Algorithm> EWBFAlgorithmsForDevice(ComputeDevice dev)
        {
            var cudaDev = dev as CudaComputeDevice;
            // CUDA SM5.0+
            if (cudaDev == null || cudaDev.SMMajor < 5) return null;

            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.EWBF, AlgorithmType.ZHash)
            };
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // NVIDIA
        private static List<Algorithm> trexAlgorithmsForDevice(ComputeDevice dev)
        {
            // TODO check if SM3.0 is compatible
            // CUDA SM3.0+
            var cudaDev = dev as CudaComputeDevice;
            if (cudaDev == null || cudaDev.SMMajor < 3) return null;

            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.trex, AlgorithmType.Skunk, "skunk"),
                new Algorithm(MinerBaseType.trex, AlgorithmType.X16R, "x16r")
            };
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // NVIDIA AMD
        private static List<Algorithm> BMinerAlgorithmsForDevice(ComputeDevice dev)
        {
            const bool enabledByDefault = false;
            var algos = new List<Algorithm>();
            // CUDA SM5.0+
            if (dev is CudaComputeDevice cudaDev && cudaDev.SMMajor >= 5)
            {
                algos = new List<Algorithm>
                {
                    new Algorithm(MinerBaseType.BMiner, AlgorithmType.ZHash) {Enabled = enabledByDefault },
                    new Algorithm(MinerBaseType.BMiner, AlgorithmType.DaggerHashimoto) {Enabled = enabledByDefault },
                    new Algorithm(MinerBaseType.BMiner, AlgorithmType.Beam) {Enabled = enabledByDefault },
                    new Algorithm(MinerBaseType.BMiner, AlgorithmType.GrinCuckaroo29) {Enabled = enabledByDefault }
                };
            }
            // only allow Gcn4+
            if (dev is AmdComputeDevice amdDev && amdDev.IsGcn4)
            {
                algos = new List<Algorithm>
                {
                    new Algorithm(MinerBaseType.BMiner, AlgorithmType.Beam) {Enabled = enabledByDefault }
                };
            }
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // NVIDIA
        private static List<Algorithm> TTMinerAlgorithmsForDevice(ComputeDevice dev)
        {
            // TODO check min version
            // CUDA SM5.0+
            var cudaDev = dev as CudaComputeDevice;
            if (cudaDev == null || cudaDev.SMMajor < 5) return null;

            const bool enabledByDefault = true;
            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.TTMiner, AlgorithmType.MTP) {Enabled = enabledByDefault },
                new Algorithm(MinerBaseType.TTMiner, AlgorithmType.Lyra2REv3) {Enabled = enabledByDefault },
            };
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // NVIDIA
        private static List<Version> nbMinerSMSupportedVersions = new List<Version>
        {
            new Version(6,0),
            new Version(6,1),
            new Version(7,0),
            new Version(7,5),
        };
        // NVIDIA
        private static List<Algorithm> NBMinerAlgorithmsForDevice(ComputeDevice dev)
        {
            var cudaDev = dev as CudaComputeDevice;
            if (cudaDev == null) return null;
            var cudaDevSMver = new Version(cudaDev.SMMajor, cudaDev.SMMinor);
            var supportedVersion = false;
            foreach (var supportedVer in nbMinerSMSupportedVersions)
            {
                if (supportedVer == cudaDevSMver)
                {
                    supportedVersion = true;
                    break;
                }
            }
            if (supportedVersion == false) return null;

            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.NBMiner, AlgorithmType.GrinCuckaroo29),
                new Algorithm(MinerBaseType.NBMiner, AlgorithmType.GrinCuckatoo31),
            };
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }

        // AMD
        private static List<Algorithm> TeamRedMinerAlgorithmsForDevice(ComputeDevice dev)
        {
            // CUDA SM6.1 ONLY
            var amdDev = dev as AmdComputeDevice;
            if (amdDev == null || amdDev.IsGcn4 == false) return null;

            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.TeamRedMiner, AlgorithmType.CryptoNightV8),
                new Algorithm(MinerBaseType.TeamRedMiner, AlgorithmType.CryptoNightR),
                new Algorithm(MinerBaseType.TeamRedMiner, AlgorithmType.Lyra2REv3),
                //new Algorithm(MinerBaseType.TeamRedMiner, AlgorithmType.Lyra2Z),
            };
            // filter RAM requirements
            algos = FilterInsufficientRamAlgorithmsList(dev.GpuRam, algos);
            return algos;
        }


        private delegate List<Algorithm> AlgorithmsForDevice(ComputeDevice dev);
        private static IReadOnlyList<AlgorithmsForDevice> algorithmsDelegates = new List<AlgorithmsForDevice>
        {
            XmrStakAlgorithmsForDevice,
            sgminerAlgorithmsForDevice,
            ccminerAlgorithmsForDevice,
            ethminerAlgorithmsForDevice,
        };

        private static IReadOnlyList<AlgorithmsForDevice> algorithmsDelegates3rdParty = new List<AlgorithmsForDevice>
        {
            ClaymoreDualAlgorithmsForDevice,
            PhoenixAlgorithmsForDevice,
            GMinerAlgorithmsForDevice,
            //ProspectorAlgorithmsForDevice,
            EWBFAlgorithmsForDevice,
            trexAlgorithmsForDevice,
            BMinerAlgorithmsForDevice,
            TTMinerAlgorithmsForDevice,
            NBMinerAlgorithmsForDevice,
            TeamRedMinerAlgorithmsForDevice,
        };

        public static List<Algorithm> GetAlgorithmsForDevice(ComputeDevice dev)
        {
            var ret = new List<Algorithm>();
            var delegates = new List<AlgorithmsForDevice>();
            delegates.AddRange(algorithmsDelegates);
            // TODO add 3rdparty checking
            delegates.AddRange(algorithmsDelegates3rdParty);

            foreach (var algorithmsFor in delegates)
            {
                var algorithms = algorithmsFor(dev);
                if (algorithms != null) ret.AddRange(algorithms);
            }
            return ret;
        }
    }
}

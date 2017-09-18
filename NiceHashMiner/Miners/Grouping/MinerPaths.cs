using NiceHashMiner.Configs;
using NiceHashMiner.Configs.ConfigJsonFile;
using NiceHashMiner.Devices;
using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NiceHashMiner.Miners.Grouping
{
    class MinerPathPackageFile : ConfigFile<MinerPathPackage>
    {
        public MinerPathPackageFile(string name)
            : base(FOLDERS.INTERNALS, String.Format("{0}.json", name), String.Format("{0}_old.json", name)) {
        }
    }

    public class MinerPathPackage
    {
        public string Name;
        public DeviceGroupType DeviceType;
        public List<MinerTypePath> MinerTypes;

        public MinerPathPackage(DeviceGroupType type, List<MinerTypePath> paths) {
            DeviceType = type;
            MinerTypes = paths;
            Name = DeviceType.ToString();
        }
    }

    public class MinerTypePath
    {
        public string Name;
        public MinerBaseType Type;
        public List<MinerPath> Algorithms;

        public MinerTypePath(MinerBaseType type, List<MinerPath> paths) {
            Type = type;
            Algorithms = paths;
            Name = type.ToString();
        }
    }

    public class MinerPath
    {
        public string Name;
        public AlgorithmType Algorithm;
        public string Path;

        public MinerPath(AlgorithmType algo, string path) {
            Algorithm = algo;
            Path = path;
            Name = Algorithm.ToString();
        }
    }
    /// <summary>
    /// MinerPaths, used just to store miners paths strings. Only one instance needed
    /// </summary>
    public static class MinerPaths
    {

        public static class Data {
            // root binary folder
            private const string _bin = @"bin";
            /// <summary>
            /// ccminers
            /// </summary>
            public const string ccminer_decred = _bin + @"\ccminer_decred\ccminer.exe";
            public const string ccminer_nanashi = _bin + @"\ccminer_nanashi\ccminer.exe";
            public const string ccminer_neoscrypt = _bin + @"\ccminer_neoscrypt\ccminer.exe";
            public const string ccminer_sp = _bin + @"\ccminer_sp\ccminer.exe";
            public const string ccminer_tpruvot = _bin + @"\ccminer_tpruvot\ccminer.exe";
            public const string ccminer_cryptonight = _bin + @"\ccminer_cryptonight\ccminer.exe";
            public const string ccminer_x11gost = _bin + @"\ccminer_x11gost\ccminer.exe";
            public const string ccminer_klaust = _bin + @"\ccminer_klaust\ccminer.exe";

            /// <summary>
            /// ethminers
            /// </summary>
            public const string ethminer = _bin + @"\ethminer\ethminer.exe";

            /// <summary>
            /// sgminers
            /// </summary>
            public const string sgminer_5_6_0_general = _bin + @"\sgminer-5-6-0-general\sgminer.exe";
            public const string sgminer_gm = _bin + @"\sgminer-gm\sgminer.exe";

            public const string nheqminer = _bin + @"\nheqminer_v0.4b\nheqminer.exe";
            public const string excavator = _bin + @"\excavator\excavator.exe";

            public const string XmrStackCPUMiner = _bin + @"\xmr-stak-cpu\xmr-stak-cpu.exe";

            public const string NONE = "";

            // root binary folder
            private const string _bin_3rdparty = @"bin_3rdparty";
            public const string ClaymoreZcashMiner = _bin_3rdparty + @"\claymore_zcash\ZecMiner64.exe";
            public const string ClaymoreCryptoNightMiner = _bin_3rdparty + @"\claymore_cryptonight\NsGpuCNMiner.exe";
            public const string OptiminerZcashMiner = _bin_3rdparty + @"\optiminer_zcash_win\Optiminer.exe";
            public const string ClaymoreDual = _bin_3rdparty + @"\claymore_dual\EthDcrMiner64.exe";
            public const string EWBF = _bin_3rdparty + @"\ewbf\miner.exe";
            public const string prospector = _bin_3rdparty + @"\prospector\prospector.exe";
        }

        // NEW START
        ////////////////////////////////////////////
        // Pure functions
        //public static bool IsMinerAlgorithmAvaliable(List<Algorithm> algos, MinerBaseType minerBaseType, AlgorithmType algorithmType) {
        //    return algos.FindIndex((a) => a.MinerBaseType == minerBaseType && a.NiceHashID == algorithmType) > -1;
        //}

        public static string GetPathFor(MinerBaseType minerBaseType, AlgorithmType algoType, DeviceGroupType devGroupType, bool def = false) {
            if (!def & configurableMiners.Contains(minerBaseType)) {
                // Override with internals
                var path = minerPathPackages.Find(p => p.DeviceType == devGroupType)
                    .MinerTypes.Find(p => p.Type == minerBaseType)
                    .Algorithms.Find(p => p.Algorithm == algoType);
                if (path != null) {
                    if (File.Exists(path.Path)) {
                        return path.Path;
                    } else {
                        Helpers.ConsolePrint("PATHS", String.Format("Path {0} not found, using defaults", path.Path));
                    }
                }
            }
            switch (minerBaseType) {
                case MinerBaseType.ccminer:
                    return NVIDIA_GROUPS.ccminer_path(algoType, devGroupType);
                case MinerBaseType.sgminer:
                    return AMD_GROUP.sgminer_path(algoType);
                case MinerBaseType.nheqminer:
                    return Data.nheqminer;
                case MinerBaseType.ethminer:
                    return Data.ethminer;
                case MinerBaseType.Claymore:
                    return AMD_GROUP.ClaymorePath(algoType);
                case MinerBaseType.OptiminerAMD:
                    return Data.OptiminerZcashMiner;
                case MinerBaseType.excavator:
                    return Data.excavator;
                case MinerBaseType.XmrStackCPU:
                    return Data.XmrStackCPUMiner;
                case MinerBaseType.ccminer_alexis:
                    return NVIDIA_GROUPS.ccminer_unstable_path(algoType, devGroupType);
                case MinerBaseType.experimental:
                    return EXPERIMENTAL.GetPath(algoType, devGroupType);
                case MinerBaseType.EWBF:
                    return Data.EWBF;
                case MinerBaseType.Prospector:
                    return Data.prospector;
            }
            return Data.NONE;
        }

        public static string GetPathFor(ComputeDevice computeDevice, Algorithm algorithm /*, Options: MinerPathsConfig*/) {
            if (computeDevice == null || algorithm == null) {
                return Data.NONE;
            }

            return GetPathFor(
                algorithm.MinerBaseType,
                algorithm.NiceHashID,
                computeDevice.DeviceGroupType
                );
        }

        public static bool IsValidMinerPath(string minerPath) {
            // TODO make a list of valid miner paths and check that instead
            return minerPath != null && Data.NONE != minerPath && minerPath != ""; 
        }

        /**
         * InitAlgorithmsMinerPaths gets and sets miner paths
         */
        public static List<Algorithm> GetAndInitAlgorithmsMinerPaths(List<Algorithm> algos, ComputeDevice computeDevice/*, Options: MinerPathsConfig*/) {
            var retAlgos = algos.FindAll((a) => a != null).ConvertAll((a) => {
                a.MinerBinaryPath = GetPathFor(computeDevice, a/*, Options*/);
                return a;
            });

            return retAlgos;
        }
        // NEW END

        ////// private stuff from here on
        static class NVIDIA_GROUPS {
            public static string ccminer_sm21_or_sm3x(AlgorithmType algorithmType) {
                if (AlgorithmType.Decred == algorithmType) {
                    return Data.ccminer_decred;
                }
                if (AlgorithmType.CryptoNight == algorithmType) {
                    return Data.ccminer_cryptonight;
                }
                return Data.ccminer_tpruvot;
            }

            public static string ccminer_sm5x_or_sm6x(AlgorithmType algorithmType) {
                if (AlgorithmType.Decred == algorithmType) {
                    return Data.ccminer_decred;
                }
                if (AlgorithmType.Lyra2RE == algorithmType 
                    || AlgorithmType.Lyra2REv2 == algorithmType) {
                    return Data.ccminer_nanashi;
                }
                if (AlgorithmType.CryptoNight == algorithmType) {
                    return Data.ccminer_cryptonight;
                }
                if (AlgorithmType.Lbry == algorithmType 
                    || AlgorithmType.X11Gost == algorithmType 
                    || AlgorithmType.Blake2s == algorithmType
                    || AlgorithmType.Skunk == algorithmType
                    || AlgorithmType.NeoScrypt == algorithmType) {
                    return Data.ccminer_tpruvot;
                }
                if (AlgorithmType.Sia == algorithmType
                    || AlgorithmType.Nist5 == algorithmType) {
                    return Data.ccminer_klaust;
                }

                return Data.ccminer_sp;
            }
            public static string ccminer_path(AlgorithmType algorithmType, DeviceGroupType nvidiaGroup) {
                // sm21 and sm3x have same settings
                if (nvidiaGroup == DeviceGroupType.NVIDIA_2_1 || nvidiaGroup == DeviceGroupType.NVIDIA_3_x) {
                    return NVIDIA_GROUPS.ccminer_sm21_or_sm3x(algorithmType);
                }
                // CN exception
                if (nvidiaGroup == DeviceGroupType.NVIDIA_6_x && algorithmType == AlgorithmType.CryptoNight) {
                    return Data.ccminer_tpruvot;
                }
                // sm5x and sm6x have same settings otherwise
                if (nvidiaGroup == DeviceGroupType.NVIDIA_5_x || nvidiaGroup == DeviceGroupType.NVIDIA_6_x) {
                    return NVIDIA_GROUPS.ccminer_sm5x_or_sm6x(algorithmType);
                }
                // TODO wrong case?
                return Data.NONE; // should not happen
            }

            public static string ccminer_unstable_path(AlgorithmType algorithmType, DeviceGroupType nvidiaGroup) {
                // sm5x and sm6x have same settings
                if (nvidiaGroup == DeviceGroupType.NVIDIA_5_x || nvidiaGroup == DeviceGroupType.NVIDIA_6_x) {
                    if (AlgorithmType.X11Gost == algorithmType || AlgorithmType.Nist5 == algorithmType) {
                        return Data.ccminer_x11gost;
                    }
                }
                // TODO wrong case?
                return Data.NONE; // should not happen
            }
        }

        static class AMD_GROUP {
            public static string sgminer_path(AlgorithmType type) {
                if (AlgorithmType.CryptoNight == type || AlgorithmType.DaggerHashimoto == type) {
                    return Data.sgminer_gm;
                }
                return Data.sgminer_5_6_0_general;
            }

            public static string ClaymorePath(AlgorithmType type) {
                if(AlgorithmType.Equihash == type) {
                    return Data.ClaymoreZcashMiner;
                } else if(AlgorithmType.CryptoNight == type) {
                    return Data.ClaymoreCryptoNightMiner;
                } else if (AlgorithmType.DaggerHashimoto == type) {
                    return Data.ClaymoreDual;
                }
                return Data.NONE; // should not happen
            }
        }

        // unstable miners, NVIDIA for now
        static class EXPERIMENTAL {
            public static string GetPath(AlgorithmType algoType, DeviceGroupType devGroupType) {
                if (devGroupType == DeviceGroupType.NVIDIA_6_x) {
                    return NVIDIA_GROUPS.ccminer_path(algoType, devGroupType);
                }
                return Data.NONE; // should not happen
            }
        }

        private static List<MinerPathPackage> minerPathPackages = new List<MinerPathPackage>();
        private static readonly List<MinerBaseType> configurableMiners = new List<MinerBaseType> {
            MinerBaseType.ccminer,
            MinerBaseType.sgminer
        };

        public static void InitializePackages() {
            var defaults = new List<MinerPathPackage>();
            for (var i = DeviceGroupType.NONE + 1; i < DeviceGroupType.LAST; i++) {
                var minerTypePaths = new List<MinerTypePath>();
                var package = GroupAlgorithms.CreateDefaultsForGroup(i);
                foreach (var type in configurableMiners) {
                    if (package.ContainsKey(type)) {
                        var minerPaths = new List<MinerPath>();
                        foreach (var algo in package[type]) {
                            minerPaths.Add(new MinerPath(algo.NiceHashID, GetPathFor(type, algo.NiceHashID, i, true)));
                        }
                        minerTypePaths.Add(new MinerTypePath(type, minerPaths));
                    }
                }
                if (minerTypePaths.Count > 0) {
                    defaults.Add(new MinerPathPackage(i, minerTypePaths));
                }
            }

            foreach (var pack in defaults) {
                var packageName = String.Format("MinerPathPackage_{0}", pack.Name);
                var packageFile = new MinerPathPackageFile(packageName);
                var readPack = packageFile.ReadFile();
                if (readPack == null) {   // read has failed
                    Helpers.ConsolePrint("MinerPaths", "Creating internal paths config " + packageName);
                    minerPathPackages.Add(pack);
                    packageFile.Commit(pack);
                } else {
                    Helpers.ConsolePrint("MinerPaths", "Loading internal paths config " + packageName);
                    var isChange = false;
                    foreach (var miner in pack.MinerTypes) {
                        var readMiner = readPack.MinerTypes.Find(x => x.Type == miner.Type);
                        if (readMiner != null) {  // file contains miner type
                            foreach (var algo in miner.Algorithms) {
                                if (!readMiner.Algorithms.Exists(x => x.Algorithm == algo.Algorithm)) {  // file does not contain algo on this miner
                                    Helpers.ConsolePrint("PATHS", String.Format("Algorithm {0} not found in miner {1} on device {2}. Adding default", algo.Name, miner.Name, pack.Name));
                                    readMiner.Algorithms.Add(algo);
                                    isChange = true;
                                }
                            }
                        } else {  // file does not contain miner type
                            Helpers.ConsolePrint("PATHS", String.Format("Miner {0} not found on device {1}", miner.Name, pack.Name));
                            readPack.MinerTypes.Add(miner);
                            isChange = true;
                        }
                    }
                    minerPathPackages.Add(readPack);
                    if (isChange) packageFile.Commit(readPack);
                }
            }
        }
    }
}

using NiceHashMiner.Configs.ConfigJsonFile;
using NiceHashMiner.Devices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Grouping
{
    public class MinerPathPackageFile : ConfigFile<MinerPathPackage>
    {
        public MinerPathPackageFile(string name)
            : base(Folders.Internals, $"{name}.json", $"{name}_old.json")
        { }
    }

    public class MinerPathPackage
    {
        public string Name;
        public DeviceGroupType DeviceType;
        public List<MinerTypePath> MinerTypes;

        public MinerPathPackage(DeviceGroupType type, List<MinerTypePath> paths)
        {
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

        public MinerTypePath(MinerBaseType type, List<MinerPath> paths)
        {
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

        public MinerPath(AlgorithmType algo, string path)
        {
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
        public static class Data
        {
            // root binary folder
            private const string Bin = @"bin";

            /// <summary>
            /// ccminers
            /// </summary>
            public const string CcminerDecred = Bin + @"\ccminer_decred\ccminer.exe";

            public const string CcminerNanashi = Bin + @"\ccminer_nanashi\ccminer.exe";
            public const string CcminerNeoscrypt = Bin + @"\ccminer_neoscrypt\ccminer.exe";
            public const string CcminerSp = Bin + @"\ccminer_sp\ccminer.exe";
            public const string CcminerTPruvot = Bin + @"\ccminer_tpruvot\ccminer.exe";
            public const string CcminerCryptonight = Bin + @"\ccminer_cryptonight\ccminer.exe";
            public const string CcminerX11Gost = Bin + @"\ccminer_x11gost\ccminer.exe";
            public const string CcminerKlausT = Bin + @"\ccminer_klaust\ccminer.exe";
            public const string CcminerX16R = Bin + @"\ccminer_x16r\ccminer.exe";

            /// <summary>
            /// ethminers
            /// </summary>
            public const string Ethminer = Bin + @"\ethminer\ethminer.exe";

            /// <summary>
            /// sgminers
            /// </summary>
            public const string Sgminer560General = Bin + @"\sgminer-5-6-0-general\sgminer.exe";

            public const string SgminerGm = Bin + @"\sgminer-gm\sgminer.exe";

            public const string Avermore = Bin + @"\avermore\sgminer.exe";

            public const string NhEqMiner = Bin + @"\nheqminer_v0.4b\NhEqMiner.exe";
            public const string Excavator = Bin + @"\excavator\excavator.exe";

            public const string XmrStackCpuMiner = Bin + @"\xmr-stak-cpu\xmr-stak-cpu.exe";
            public const string XmrStakAmd = Bin + @"\xmr-stak-amd\xmr-stak-amd.exe";
            public const string XmrStak = Bin + @"\xmr-stak\xmr-stak.exe";
            public const string Xmrig = Bin + @"\xmrig\xmrig.exe";
            public const string XmrStakHeavy = Bin + @"\xmr-stak_heavy\xmr-stak.exe";

            public const string CpuMiner = Bin + @"\cpuminer_opt\cpuminer.exe";

            public const string None = "";

            // root binary folder
            private const string Bin3rdParty = @"bin_3rdparty";

            public const string ClaymoreZcashMiner = Bin3rdParty + @"\claymore_zcash\ZecMiner64.exe";
            public const string ClaymoreCryptoNightMiner = Bin3rdParty + @"\claymore_cryptonight\NsGpuCNMiner.exe";

            public const string OptiminerZcashMiner = Bin3rdParty + @"\optiminer_zcash_win\Optiminer.exe";
            public const string ClaymoreDual = Bin3rdParty + @"\claymore_dual\EthDcrMiner64.exe";
            public const string Ewbf = Bin3rdParty + @"\ewbf\miner.exe";
            public const string Prospector = Bin3rdParty + @"\prospector\prospector.exe";
            public const string Dtsm = Bin3rdParty + @"\dtsm\zm.exe";

            public const string EthLargement = Bin3rdParty + @"\ethlargement\OhGodAnETHlargementPill-r2.exe";
        }

        // NEW START
        ////////////////////////////////////////////
        // Pure functions
        //public static bool IsMinerAlgorithmAvaliable(List<Algorithm> algos, MinerBaseType minerBaseType, AlgorithmType algorithmType) {
        //    return algos.FindIndex((a) => a.MinerBaseType == minerBaseType && a.NiceHashID == algorithmType) > -1;
        //}

        public static string GetPathFor(MinerBaseType minerBaseType, AlgorithmType algoType,
            DeviceGroupType devGroupType, bool def = false)
        {
            if (!def & ConfigurableMiners.Contains(minerBaseType))
            {
                // Override with internals
                var path = MinerPathPackages.Find(p => p.DeviceType == devGroupType)
                    .MinerTypes.Find(p => p.Type == minerBaseType)
                    .Algorithms.Find(p => p.Algorithm == algoType);
                if (path != null)
                {
                    if (File.Exists(path.Path))
                    {
                        return path.Path;
                    }
                    Helpers.ConsolePrint("PATHS", $"Path {path.Path} not found, using defaults");
                }
            }
            // Temp workaround
            if (minerBaseType == MinerBaseType.XmrStak && algoType == AlgorithmType.CryptoNightHeavy)
                return Data.XmrStakHeavy;

            switch (minerBaseType)
            {
                case MinerBaseType.ccminer:
                    return NvidiaGroups.Ccminer_path(algoType, devGroupType);
                case MinerBaseType.sgminer:
                    return AmdGroup.SgminerPath(algoType);
                case MinerBaseType.nheqminer:
                    return Data.NhEqMiner;
                case MinerBaseType.ethminer:
                    return Data.Ethminer;
                case MinerBaseType.Claymore:
                    return AmdGroup.ClaymorePath(algoType);
                case MinerBaseType.OptiminerAMD:
                    return Data.OptiminerZcashMiner;
                //case MinerBaseType.excavator:
                //    return Data.Excavator;
                case MinerBaseType.XmrStak:
                    return Data.XmrStak;
                case MinerBaseType.ccminer_alexis:
                    return NvidiaGroups.CcminerUnstablePath(algoType, devGroupType);
                case MinerBaseType.experimental:
                    return Experimental.GetPath(algoType, devGroupType);
                case MinerBaseType.EWBF:
                    return Data.Ewbf;
                case MinerBaseType.Prospector:
                    return Data.Prospector;
                case MinerBaseType.Xmrig:
                    return Data.Xmrig;
                case MinerBaseType.dtsm:
                    return Data.Dtsm;
                case MinerBaseType.cpuminer:
                    return Data.CpuMiner;
            }
            return Data.None;
        }

        public static string GetPathFor(ComputeDevice computeDevice,
            Algorithm algorithm /*, Options: MinerPathsConfig*/)
        {
            if (computeDevice == null || algorithm == null)
            {
                return Data.None;
            }

            return GetPathFor(
                algorithm.MinerBaseType,
                algorithm.NiceHashID,
                computeDevice.DeviceGroupType
            );
        }

        public static bool IsValidMinerPath(string minerPath)
        {
            // TODO make a list of valid miner paths and check that instead
            return minerPath != null && Data.None != minerPath && minerPath != "";
        }

        /**
         * InitAlgorithmsMinerPaths gets and sets miner paths
         */
        public static List<Algorithm> GetAndInitAlgorithmsMinerPaths(List<Algorithm> algos,
            ComputeDevice computeDevice /*, Options: MinerPathsConfig*/)
        {
            var retAlgos = algos.FindAll((a) => a != null).ConvertAll((a) =>
            {
                a.MinerBinaryPath = GetPathFor(computeDevice, a /*, Options*/);
                return a;
            });

            return retAlgos;
        }
        // NEW END

        ////// private stuff from here on
        private static class NvidiaGroups
        {
            private static string CcminerSM21(AlgorithmType algorithmType)
            {
                return AlgorithmType.CryptoNight == algorithmType ? Data.CcminerCryptonight : Data.CcminerDecred;
            }
            private static string CcminerSM3X(AlgorithmType algorithmType)
            {
                if (AlgorithmType.Decred == algorithmType)
                {
                    return Data.CcminerDecred;
                }
                if (AlgorithmType.CryptoNight == algorithmType)
                {
                    return Data.CcminerCryptonight;
                }
                return Data.CcminerTPruvot;
            }

            private static string CcminerSM5XOrSM6X(AlgorithmType algorithmType)
            {
                switch (algorithmType)
                {
                    case AlgorithmType.Decred:
                        return Data.CcminerDecred;
                    case AlgorithmType.Lyra2RE:
                    case AlgorithmType.Lyra2REv2:
                        return Data.CcminerNanashi;
                    case AlgorithmType.CryptoNight:
                        return Data.CcminerCryptonight;
                    case AlgorithmType.Lbry:
                    case AlgorithmType.X11Gost:
                    case AlgorithmType.Blake2s:
                    case AlgorithmType.Skunk:
                    case AlgorithmType.Keccak:
                    case AlgorithmType.Lyra2z:
                        return Data.CcminerTPruvot;
                    case AlgorithmType.Sia:
                    case AlgorithmType.Nist5:
                    case AlgorithmType.NeoScrypt:
                        return Data.CcminerKlausT;
                    case AlgorithmType.X16R:
                        return Data.CcminerX16R;
                }

                return Data.CcminerSp;
            }

            public static string Ccminer_path(AlgorithmType algorithmType, DeviceGroupType nvidiaGroup)
            {
                switch (nvidiaGroup)
                {
                    // sm21 and sm3x no longer have same settings since tpruvot dropped 21 support
                    case DeviceGroupType.NVIDIA_2_1:
                        return CcminerSM21(algorithmType);
                    case DeviceGroupType.NVIDIA_3_x:
                        return CcminerSM3X(algorithmType);
                    // CN exception
                    case DeviceGroupType.NVIDIA_6_x when algorithmType == AlgorithmType.CryptoNight:
                        return Data.CcminerTPruvot;
                    // sm5x and sm6x have same settings otherwise
                    case DeviceGroupType.NVIDIA_5_x:
                    case DeviceGroupType.NVIDIA_6_x:
                        return CcminerSM5XOrSM6X(algorithmType);
                }
                // TODO wrong case?
                return Data.None; // should not happen
            }

            public static string CcminerUnstablePath(AlgorithmType algorithmType, DeviceGroupType nvidiaGroup)
            {
                // sm5x and sm6x have same settings
                if ((nvidiaGroup == DeviceGroupType.NVIDIA_5_x || nvidiaGroup == DeviceGroupType.NVIDIA_6_x) &&
                    (AlgorithmType.X11Gost == algorithmType || AlgorithmType.Nist5 == algorithmType || AlgorithmType.Keccak == algorithmType))
                    return Data.CcminerX11Gost;
                // TODO wrong case?
                return Data.None; // should not happen
            }
        }

        private static class AmdGroup
        {
            public static string SgminerPath(AlgorithmType type)
            {
                switch (type)
                {
                    case AlgorithmType.CryptoNight:
                    case AlgorithmType.DaggerHashimoto:
                        return Data.SgminerGm;
                    case AlgorithmType.X16R:
                        return Data.Avermore;
                    default:
                        return Data.Sgminer560General;
                }
            }

            public static string ClaymorePath(AlgorithmType type)
            {
                switch (type)
                {
                    case AlgorithmType.Equihash:
                        return Data.ClaymoreZcashMiner;
                    case AlgorithmType.CryptoNightV7:
                        return Data.ClaymoreCryptoNightMiner;
                    case AlgorithmType.DaggerHashimoto:
                        return Data.ClaymoreDual;
                    default:
                        return Data.None;
                }
            }
        }

        // unstable miners, NVIDIA for now
        private static class Experimental
        {
            public static string GetPath(AlgorithmType algoType, DeviceGroupType devGroupType)
            {
                return devGroupType == DeviceGroupType.NVIDIA_6_x
                    ? NvidiaGroups.Ccminer_path(algoType, devGroupType)
                    : Data.None;
            }
        }

        private static readonly List<MinerPathPackage> MinerPathPackages = new List<MinerPathPackage>();

        private static readonly List<MinerBaseType> ConfigurableMiners = new List<MinerBaseType>
        {
            MinerBaseType.ccminer,
            MinerBaseType.sgminer
        };

        public static void InitializePackages()
        {
            var defaults = new List<MinerPathPackage>();
            for (var i = DeviceGroupType.NONE + 1; i < DeviceGroupType.LAST; i++)
            {
                var package = GroupAlgorithms.CreateDefaultsForGroup(i);
                var minerTypePaths = (from type in ConfigurableMiners
                    where package.ContainsKey(type)
                    let minerPaths = package[type].Select(algo =>
                        new MinerPath(algo.NiceHashID, GetPathFor(type, algo.NiceHashID, i, true))).ToList()
                    select new MinerTypePath(type, minerPaths)).ToList();
                if (minerTypePaths.Count > 0)
                {
                    defaults.Add(new MinerPathPackage(i, minerTypePaths));
                }
            }

            foreach (var pack in defaults)
            {
                var packageName = $"MinerPathPackage_{pack.Name}";
                var packageFile = new MinerPathPackageFile(packageName);
                var readPack = packageFile.ReadFile();
                if (readPack == null)
                {
                    // read has failed
                    Helpers.ConsolePrint("MinerPaths", "Creating internal paths config " + packageName);
                    MinerPathPackages.Add(pack);
                    packageFile.Commit(pack);
                }
                else
                {
                    Helpers.ConsolePrint("MinerPaths", "Loading internal paths config " + packageName);
                    var isChange = false;
                    foreach (var miner in pack.MinerTypes)
                    {
                        var readMiner = readPack.MinerTypes.Find(x => x.Type == miner.Type);
                        if (readMiner != null)
                        {
                            // file contains miner type
                            foreach (var algo in miner.Algorithms)
                            {
                                if (!readMiner.Algorithms.Exists(x => x.Algorithm == algo.Algorithm))
                                {
                                    // file does not contain algo on this miner
                                    Helpers.ConsolePrint("PATHS",
                                        $"Algorithm {algo.Name} not found in miner {miner.Name} on device {pack.Name}. Adding default");
                                    readMiner.Algorithms.Add(algo);
                                    isChange = true;
                                }
                            }
                        }
                        else
                        {
                            // file does not contain miner type
                            Helpers.ConsolePrint("PATHS", $"Miner {miner.Name} not found on device {pack.Name}");
                            readPack.MinerTypes.Add(miner);
                            isChange = true;
                        }
                    }
                    MinerPathPackages.Add(readPack);
                    if (isChange) packageFile.Commit(readPack);
                }
            }
        }
    }
}

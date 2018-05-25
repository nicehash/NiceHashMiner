using NiceHashMiner.Configs.ConfigJsonFile;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System.Collections.Generic;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public static class MinersSettingsManager
    {
        private class MinerReservedPortsFile : ConfigFile<
            Dictionary<MinerBaseType, Dictionary<string, Dictionary<AlgorithmType, List<int>>>>>
        {
            public MinerReservedPortsFile()
                : base(Folders.Config, "MinerReservedPorts.json", "MinerReservedPorts_old.json")
            { }
        }

        // {miner path : {envName : envValue} }
        private class MinerSystemVariablesFile : ConfigFile<Dictionary<string, Dictionary<string, string>>>
        {
            public MinerSystemVariablesFile() : base(Folders.Config, "MinerSystemVariables.json", "MinerSystemVariables_old.json")
            { }
        }

        private static Dictionary<MinerBaseType,
            Dictionary<string,
                Dictionary<AlgorithmType,
                    List<int>>>> _minerReservedPorts =
            new Dictionary<MinerBaseType, Dictionary<string, Dictionary<AlgorithmType, List<int>>>>();

        public static List<int> AllReservedPorts = new List<int>();

        public static Dictionary<string, Dictionary<string, string>> MinerSystemVariables =
            new Dictionary<string, Dictionary<string, string>>();

        public static void Init()
        {
            ExtraLaunchParameters.InitializePackages();
            MinerPaths.InitializePackages();
            InitMinerReservedPortsFile();
            InitMinerSystemVariablesFile();
        }

        public static List<int> GetPortsListFor(MinerBaseType minerBaseType, string path, AlgorithmType algorithmType)
        {
            if (_minerReservedPorts != null && _minerReservedPorts.ContainsKey(minerBaseType) &&
                (_minerReservedPorts[minerBaseType] != null && _minerReservedPorts[minerBaseType].ContainsKey(path)) &&
                (_minerReservedPorts[minerBaseType][path] != null &&
                 _minerReservedPorts[minerBaseType][path].ContainsKey(algorithmType) &&
                 _minerReservedPorts[minerBaseType][path][algorithmType] != null))
            {
                return _minerReservedPorts[minerBaseType][path][algorithmType];
            }

            return new List<int>();
        }

        public static void InitMinerReservedPortsFile()
        {
            var file = new MinerReservedPortsFile();
            _minerReservedPorts = new Dictionary<MinerBaseType, Dictionary<string, Dictionary<AlgorithmType, List<int>>>>();
            if (file.IsFileExists())
            {
                var read = file.ReadFile();
                if (read != null)
                {
                    _minerReservedPorts = read;
                }
            }
            try
            {
                for (var type = (MinerBaseType.NONE + 1); type < MinerBaseType.END; ++type)
                {
                    if (_minerReservedPorts.ContainsKey(type) == false)
                    {
                        _minerReservedPorts[type] = new Dictionary<string, Dictionary<AlgorithmType, List<int>>>();
                    }
                }
                for (var devGroupType = (DeviceGroupType.NONE + 1); devGroupType < DeviceGroupType.LAST; ++devGroupType)
                {
                    var minerAlgosForGroup = GroupAlgorithms.CreateDefaultsForGroup(devGroupType);
                    if (minerAlgosForGroup != null)
                    {
                        foreach (var mbaseKvp in minerAlgosForGroup)
                        {
                            var minerBaseType = mbaseKvp.Key;
                            if (_minerReservedPorts.ContainsKey(minerBaseType))
                            {
                                var algos = mbaseKvp.Value;
                                foreach (var algo in algos)
                                {
                                    var algoType = algo.NiceHashID;
                                    var path = MinerPaths.GetPathFor(minerBaseType, algoType, devGroupType);
                                    var isPathValid = path != MinerPaths.Data.None;
                                    if (isPathValid && _minerReservedPorts[minerBaseType].ContainsKey(path) == false)
                                    {
                                        _minerReservedPorts[minerBaseType][path] = new Dictionary<AlgorithmType, List<int>>();
                                    }
                                    if (isPathValid && _minerReservedPorts[minerBaseType][path] != null &&
                                        _minerReservedPorts[minerBaseType][path].ContainsKey(algoType) == false)
                                    {
                                        _minerReservedPorts[minerBaseType][path][algoType] = new List<int>();
                                    }
                                }
                            }
                        }
                    }
                }
                file.Commit(_minerReservedPorts);
                // set all reserved
                foreach (var paths in _minerReservedPorts.Values)
                {
                    foreach (var algos in paths.Values)
                    {
                        foreach (var ports in algos.Values)
                        {
                            foreach (var port in ports)
                            {
                                AllReservedPorts.Add(port);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static void InitMinerSystemVariablesFile()
        {
            var file = new MinerSystemVariablesFile();
            MinerSystemVariables = new Dictionary<string, Dictionary<string, string>>();
            var isFileInit = false;
            if (file.IsFileExists())
            {
                var read = file.ReadFile();
                if (read != null)
                {
                    isFileInit = true;
                    MinerSystemVariables = read;
                }
            }
            if (!isFileInit)
            {
                // general AMD defaults scope
                {
                    var minerPaths = new List<string>()
                    {
                        MinerPaths.Data.Sgminer560General,
                        MinerPaths.Data.SgminerGm,
                        MinerPaths.Data.ClaymoreCryptoNightMiner,
                        MinerPaths.Data.ClaymoreZcashMiner,
                        MinerPaths.Data.OptiminerZcashMiner
                    };
                    foreach (var minerPath in minerPaths)
                    {
                        MinerSystemVariables[minerPath] = new Dictionary<string, string>()
                        {
                            {"GPU_MAX_ALLOC_PERCENT", "100"},
                            {"GPU_USE_SYNC_OBJECTS", "1"},
                            {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                            {"GPU_MAX_HEAP_SIZE", "100"},
                            {"GPU_FORCE_64BIT_PTR", "1"}
                        };
                    }
                }
                // ClaymoreDual scope
                {
                    MinerSystemVariables[MinerPaths.Data.ClaymoreDual] = new Dictionary<string, string>()
                    {
                        {"GPU_MAX_ALLOC_PERCENT", "100"},
                        {"GPU_USE_SYNC_OBJECTS", "1"},
                        {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                        {"GPU_MAX_HEAP_SIZE", "100"},
                        {"GPU_FORCE_64BIT_PTR", "0"}
                    };
                }
                // save defaults
                file.Commit(MinerSystemVariables);
            }
        }
    }
}

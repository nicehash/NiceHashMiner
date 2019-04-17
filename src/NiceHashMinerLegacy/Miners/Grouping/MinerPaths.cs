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
    /// <summary>
    /// MinerPaths, used just to store miners paths strings. Only one instance needed
    /// </summary>
    public static class MinerPaths
    {
        public static class Data
        {
            public const string None = "";

            // root binary folder
            public const string ClaymoreDual = @"miner_plugins\ClaymoreDual\bins\EthDcrMiner64.exe";
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
            switch (minerBaseType)
            {
                case MinerBaseType.Claymore:
                    return Data.ClaymoreDual;
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
    }
}

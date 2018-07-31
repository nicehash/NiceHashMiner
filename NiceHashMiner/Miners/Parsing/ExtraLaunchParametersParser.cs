using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Parsing
{
    public static class ExtraLaunchParametersParser
    {
        private const string Tag = "ExtraLaunchParametersParser";
        private const string MinerOptionTypeNone = "MinerOptionType_NONE";


        private static bool _showLog = true;

        private static void LogParser(string msg)
        {
            if (_showLog)
            {
                Helpers.ConsolePrint(Tag, msg);
            }
        }

        // exception...
        public static int GetEqmCudaThreadCount(MiningPair pair)
        {
            if (pair.CurrentExtraLaunchParameters.Contains("-ct"))
            {
                var eqmCudaOptions = new List<MinerOption>
                {
                    new MinerOption("CUDA_Solver_Thread", "-ct", "-ct", "1", MinerOptionFlagType.MultiParam, " "),
                };
                var parsedStr = Parse(new List<MiningPair>
                {
                    pair
                }, eqmCudaOptions);
                try
                {
                    var threads = int.Parse(parsedStr.Trim().Replace("-ct", "").Trim());
                    return threads;
                }
                catch { }
            }
            return 1; // default 
        }

        private static bool _prevHasIgnoreParam = false;
        private static int _logCount = 0;

        private static void IgnorePrintLogInit()
        {
            _prevHasIgnoreParam = false;
            _logCount = 0;
        }

        private static void IgnorePrintLog(string param, string ignoreParam, List<MinerOption> ignoreLogOpions = null)
        {
            // AMD temp controll is separated and logs stuff that is ignored
            var printIgnore = true;
            if (ignoreLogOpions != null)
            {
                if (ignoreLogOpions.Any(ignoreOption => param.Equals(ignoreOption.ShortName) || param.Equals(ignoreOption.LongName)))
                {
                    printIgnore = false;
                    _prevHasIgnoreParam = true;
                    _logCount = 0;
                }
            }
            if (printIgnore && !_prevHasIgnoreParam)
            {
                LogParser(string.Format(ignoreParam, param));
            }
            if (_logCount == 1)
            {
                _prevHasIgnoreParam = false;
                _logCount = 0;
            }
            ++_logCount;
        }

        private static string Parse(List<MiningPair> miningPairs, List<MinerOption> options, bool useIfDefaults = true,
            List<MinerOption> ignoreLogOpions = null, bool ignoreDcri = false)
        {
            const string ignoreParam = "Cannot parse \"{0}\", not supported, set to ignore, or wrong extra launch parameter settings";
            var optionsOrder = new List<string>();
            var cdevOptions = new Dictionary<string, Dictionary<string, string>>();
            var isOptionDefaults = new Dictionary<string, bool>();
            var isOptionExist = new Dictionary<string, bool>();
            // init devs options, and defaults
            foreach (var pair in miningPairs)
            {
                var defaults = new Dictionary<string, string>();
                foreach (var option in options)
                {
                    defaults[option.Type] = option.Default;
                }
                cdevOptions[pair.Device.Uuid] = defaults;
            }
            // init order and params flags, and params list
            foreach (var option in options)
            {
                var optionType = option.Type;
                optionsOrder.Add(optionType);

                isOptionDefaults[optionType] = true;
                isOptionExist[optionType] = false;
            }
            // parse
            foreach (var pair in miningPairs)
            {
                LogParser($"ExtraLaunch params \"{pair.CurrentExtraLaunchParameters}\" for device UUID {pair.Device.Uuid}");
                var parameters = pair.CurrentExtraLaunchParameters.Replace("=", "= ").Split(' ');

                IgnorePrintLogInit();


                var currentFlag = MinerOptionTypeNone;
                var ignoringNextOption = false;
                foreach (var param in parameters)
                {
                    if (param.Equals("")) continue;

                    if (currentFlag == MinerOptionTypeNone)
                    {
                        var isIngored = true;
                        foreach (var option in options)
                        {
                            if (param.Equals(option.ShortName) || param.Equals(option.LongName))
                            {
                                isIngored = false;
                                if (ignoreDcri && option.Type.Equals("ClaymoreDual_dcri")) 
                                {
                                    Helpers.ConsolePrint("CDTUNING", "Disabling dcri extra launch param");
                                    ignoringNextOption = true;
                                } 
                                else 
                                {
                                    if (option.FlagType == MinerOptionFlagType.Uni) {
                                        isOptionExist[option.Type] = true;
                                        cdevOptions[pair.Device.Uuid][option.Type] = "notNull"; // if Uni param is null it is not present
                                    } 
                                    else 
                                    { 
                                        // Sinlge and Multi param
                                        currentFlag = option.Type;
                                    }
                                }
                            }
                        }
                        if (isIngored) 
                        {
                            if (ignoringNextOption) 
                            {
                                // This is a paramater for an ignored option, silently ignore it
                                ignoringNextOption = false;
                            } else 
                            {
                                IgnorePrintLog(param, ignoreParam, ignoreLogOpions);
                            }
                        }
                    } 
                    else if (currentFlag != MinerOptionTypeNone) {
                        isOptionExist[currentFlag] = true;
                        cdevOptions[pair.Device.Uuid][currentFlag] = param;
                        currentFlag = MinerOptionTypeNone;
                    }
                    else
                    {
                        // problem
                        IgnorePrintLog(param, ignoreParam, ignoreLogOpions);
                    }
                }
            }

            var retVal = "";

            // check if is all defaults
            var isAllDefault = true;
            foreach (var pair in miningPairs)
            {
                foreach (var option in options)
                {
                    if (option.Default != cdevOptions[pair.Device.Uuid][option.Type])
                    {
                        isAllDefault = false;
                        isOptionDefaults[option.Type] = false;
                    }
                }
            }

            if (!isAllDefault || useIfDefaults)
            {
                foreach (var option in options)
                {
                    if (isOptionDefaults[option.Type] && !isOptionExist[option.Type] && !useIfDefaults) continue;
                    // if options all default ignore
                    switch (option.FlagType)
                    {
                        case MinerOptionFlagType.Uni:
                            // uni params if one exist use or all must exist?
                            var isOptionInUse = miningPairs.Any(pair => cdevOptions[pair.Device.Uuid][option.Type] != null);
                            if (isOptionInUse)
                            {
                                retVal += $" {option.LongName}";
                            }
                            break;
                        case MinerOptionFlagType.MultiParam:
                        {
                            var values = miningPairs.Select(pair => cdevOptions[pair.Device.Uuid][option.Type]).ToList();
                            var mask = " {0} {1}";
                            if (option.LongName.Contains("="))
                            {
                                mask = " {0}{1}";
                            }
                            retVal += string.Format(mask, option.LongName, string.Join(option.Separator, values));
                            break;
                        }
                        case MinerOptionFlagType.SingleParam:
                        {
                            var values = new HashSet<string>();
                            foreach (var pair in miningPairs)
                            {
                                values.Add(cdevOptions[pair.Device.Uuid][option.Type]);
                            }
                            var setValue = option.Default;
                            if (values.Count >= 1)
                            {
                                // Always take first
                                setValue = values.First();
                            }
                            var mask = " {0} {1}";
                            if (option.LongName.Contains("="))
                            {
                                mask = " {0}{1}";
                            }
                            retVal += string.Format(mask, option.LongName, setValue);
                            break;
                        }
                        case MinerOptionFlagType.DuplicateMultiParam:
                        {
                            const string mask = " {0} {1}";
                            var values = miningPairs.Select(pair =>
                                string.Format(mask, option.LongName, cdevOptions[pair.Device.Uuid][option.Type])).ToList();
                            retVal += " " + string.Join(" ", values);
                            break;
                        }
                    }
                }
            }

            LogParser($"Final extra launch params parse \"{retVal}\"");

            return retVal;
        }

        public static string ParseForMiningSetup(MiningSetup miningSetup, DeviceType deviceType, bool showLog = true)
        {
            return ParseForMiningPairs(
                miningSetup.MiningPairs,
                deviceType, showLog);
        }

        public static string ParseForMiningPair(MiningPair miningPair, AlgorithmType algorithmType, DeviceType deviceType,
            bool showLog = true)
        {
            return ParseForMiningPairs(
                new List<MiningPair>
                {
                    miningPair
                },
                deviceType, showLog);
        }

        private static MinerType GetMinerType(DeviceType deviceType, MinerBaseType minerBaseType, AlgorithmType algorithmType)
        {
            //if (MinerBaseType.cpuminer == minerBaseType) {
            //    return MinerType.cpuminer_opt;
            //}
            switch (minerBaseType)
            {
                case MinerBaseType.OptiminerAMD:
                    return MinerType.OptiminerZcash;
                case MinerBaseType.sgminer:
                    return MinerType.sgminer;
                case MinerBaseType.ccminer:
                case MinerBaseType.ccminer_alexis:
                case MinerBaseType.experimental:
                    if (AlgorithmType.CryptoNight == algorithmType)
                    {
                        return MinerType.ccminer_CryptoNight;
                    }
                    return MinerType.ccminer;
                case MinerBaseType.Claymore:
                    switch (algorithmType)
                    {
                        case AlgorithmType.CryptoNight:
                            return MinerType.ClaymoreCryptoNight;
                        case AlgorithmType.Equihash:
                            return MinerType.ClaymoreZcash;
                        case AlgorithmType.DaggerHashimoto:
                            return MinerType.ClaymoreDual;
                    }
                    break;
                case MinerBaseType.Claymore_old:
                    if (AlgorithmType.CryptoNight == algorithmType)
                    {
                        return MinerType.ClaymoreCryptoNight;
                    }
                    break;
                case MinerBaseType.ethminer:
                    if (DeviceType.AMD == deviceType)
                    {
                        return MinerType.ethminer_OCL;
                    }
                    if (DeviceType.NVIDIA == deviceType)
                    {
                        return MinerType.ethminer_CUDA;
                    }
                    break;
                case MinerBaseType.nheqminer:
                    switch (deviceType)
                    {
                        case DeviceType.CPU:
                            return MinerType.nheqminer_CPU;
                        case DeviceType.AMD:
                            return MinerType.nheqminer_AMD;
                        case DeviceType.NVIDIA:
                            return MinerType.nheqminer_CUDA;
                    }
                    break;
                case MinerBaseType.eqm:
                    if (DeviceType.CPU == deviceType)
                    {
                        return MinerType.eqm_CPU;
                    }
                    if (DeviceType.NVIDIA == deviceType)
                    {
                        return MinerType.eqm_CUDA;
                    }
                    break;
                //case MinerBaseType.excavator:
                //    return MinerType.excavator;
                case MinerBaseType.EWBF:
                    return MinerType.EWBF;
                case MinerBaseType.Xmrig:
                    return MinerType.Xmrig;
                case MinerBaseType.dtsm:
                    return MinerType.dtsm;
                case MinerBaseType.cpuminer:
                    return MinerType.cpuminer_opt;
            }

            return MinerType.NONE;
        }

        public static string ParseForMiningPairs(List<MiningPair> miningPairs, DeviceType deviceType, bool showLog = true)
        {
            _showLog = showLog;

            var minerBaseType = MinerBaseType.NONE;
            var algorithmType = AlgorithmType.NONE;
            var ignoreDcri = false;
            if (miningPairs.Count > 0)
            {
                var algo = miningPairs[0].Algorithm;
                if (algo != null)
                {
                    algorithmType = algo.NiceHashID;
                    minerBaseType = algo.MinerBaseType;
                    if (algo is DualAlgorithm dualAlgo && dualAlgo.TuningEnabled) ignoreDcri = true;
                }
            }

            var minerType = GetMinerType(deviceType, minerBaseType, algorithmType);

            var minerOptionPackage = ExtraLaunchParameters.GetMinerOptionPackageForMinerType(minerType);

            var setMiningPairs = miningPairs.ConvertAll((mp) => mp);
            // handle exceptions and package parsing
            // CPU exception
            if (deviceType == DeviceType.CPU && minerType != MinerType.Xmrig)
            {
                CheckAndSetCpuPairs(setMiningPairs);
            }
            // ethminer exception
            if (MinerType.ethminer_OCL == minerType || MinerType.ethminer_CUDA == minerType)
            {
                // use if missing compute device for correct mapping
                // init fakes workaround
                var cDevsMappings = new List<MiningPair>();
                {
                    var id = -1;
                    var fakeAlgo = new Algorithm(MinerBaseType.ethminer, AlgorithmType.DaggerHashimoto, "daggerhashimoto");
                    foreach (var pair in setMiningPairs)
                    {
                        while (++id != pair.Device.ID)
                        {
                            var fakeCdev = new ComputeDevice(id);
                            cDevsMappings.Add(new MiningPair(fakeCdev, fakeAlgo));
                        }
                        cDevsMappings.Add(pair);
                    }
                }
                // reset setMiningPairs
                setMiningPairs = cDevsMappings;
            }
            // sgminer exception handle intensity types
            if (MinerType.sgminer == minerType)
            {
                // rawIntensity overrides xintensity, xintensity overrides intensity
                var sgminerIntensities = new List<MinerOption>
                {
                    new MinerOption("Intensity", "-I", "--intensity", "d", MinerOptionFlagType.MultiParam,
                        ","), // default is "d" check if -1 works
                    new MinerOption("Xintensity", "-X", "--xintensity", "-1", MinerOptionFlagType.MultiParam, ","), // default none
                    new MinerOption("Rawintensity", "", "--rawintensity", "-1", MinerOptionFlagType.MultiParam, ","), // default none
                };
                var containsIntensity = new Dictionary<string, bool>
                {
                    {"Intensity", false},
                    {"Xintensity", false},
                    {"Rawintensity", false},
                };
                // check intensity and xintensity, the latter overrides so change accordingly
                foreach (var cDev in setMiningPairs)
                {
                    foreach (var intensityOption in sgminerIntensities)
                    {
                        if (!string.IsNullOrEmpty(intensityOption.ShortName) &&
                            cDev.CurrentExtraLaunchParameters.Contains(intensityOption.ShortName))
                        {
                            cDev.CurrentExtraLaunchParameters =
                                cDev.CurrentExtraLaunchParameters.Replace(intensityOption.ShortName, intensityOption.LongName);
                            containsIntensity[intensityOption.Type] = true;
                        }
                        if (cDev.CurrentExtraLaunchParameters.Contains(intensityOption.LongName))
                        {
                            containsIntensity[intensityOption.Type] = true;
                        }
                    }
                }
                // replace
                if (containsIntensity["Intensity"] && containsIntensity["Xintensity"])
                {
                    LogParser("Sgminer replacing --intensity with --xintensity");
                    foreach (var cDev in setMiningPairs)
                    {
                        cDev.CurrentExtraLaunchParameters = cDev.CurrentExtraLaunchParameters.Replace("--intensity", "--xintensity");
                    }
                }
                if (containsIntensity["Xintensity"] && containsIntensity["Rawintensity"])
                {
                    LogParser("Sgminer replacing --xintensity with --rawintensity");
                    foreach (var cDev in setMiningPairs)
                    {
                        cDev.CurrentExtraLaunchParameters = cDev.CurrentExtraLaunchParameters.Replace("--xintensity", "--rawintensity");
                    }
                }
            }

            string ret;
            var general = Parse(setMiningPairs, minerOptionPackage.GeneralOptions, false, minerOptionPackage.TemperatureOptions, ignoreDcri);
            // temp control and parse
            if (ConfigManager.GeneralConfig.DisableAMDTempControl)
            {
                LogParser("DisableAMDTempControl is TRUE, temp control parameters will be ignored");
                ret = general;
            }
            else
            {
                LogParser("AMD parsing temperature control parameters");
                // temp = Parse(setMiningPairs, minerOptionPackage.TemperatureOptions, true, minerOptionPackage.GeneralOptions);            
                var temp = Parse(setMiningPairs, minerOptionPackage.TemperatureOptions, false, minerOptionPackage.GeneralOptions, ignoreDcri);

                ret = general + "  " + temp;
            }

            return ret;
        }

        private static void CheckAndSetCpuPairs(List<MiningPair> miningPairs)
        {
            foreach (var pair in miningPairs)
            {
                var cDev = pair.Device;
                // extra thread check
                if (pair.CurrentExtraLaunchParameters.Contains("--threads=") || pair.CurrentExtraLaunchParameters.Contains("-t"))
                {
                    // nothing
                }
                else
                {
                    // add threads params mandatory
                    pair.CurrentExtraLaunchParameters += " -t " + GetThreads(cDev.Threads, pair.Algorithm.LessThreads);
                }
            }
        }

        public static int GetThreadsNumber(MiningPair cpuPair)
        {
            var cDev = cpuPair.Device;
            var algo = cpuPair.Algorithm;
            // extra thread check
            if (algo.ExtraLaunchParameters.Contains("--threads=") || algo.ExtraLaunchParameters.Contains("-t"))
            {
                var strings = algo.ExtraLaunchParameters.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                var i = -1;
                for (var curI = 0; curI < strings.Length; ++curI)
                {
                    if (strings[curI] == "--threads=" || strings[curI] == "-t")
                    {
                        i = curI + 1;
                        break;
                    }
                }
                if (i > -1 && strings.Length > i)
                {
                    var numTr = cDev.Threads;
                    if (int.TryParse(strings[i], out numTr))
                    {
                        if (numTr <= cDev.Threads) return numTr;
                    }
                }
            }
            return GetThreads(cDev.Threads, cpuPair.Algorithm.LessThreads);
        }

        private static int GetThreads(int threads, int lessThreads)
        {
            if (threads > lessThreads)
            {
                return threads - lessThreads;
            }
            return threads;
        }

        public static bool GetNoPrefetch(MiningPair cpuPair)
        {
            var algo = cpuPair.Algorithm;
            return algo.ExtraLaunchParameters.Contains("--no_prefetch");
        }

        public static List<int> GetIntensityStak(MiningPair pair)
        {
            var algo = pair.Algorithm;
            var intensities = new List<int>();
            if (algo.ExtraLaunchParameters.Contains("--intensity"))
            {
                var strings = algo.ExtraLaunchParameters.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var i = strings.FindIndex(a => a == "--intensity") + 1;
                if (i > -1 && strings.Count > i)
                {
                    var intStrings = strings[i].Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var intString in intStrings)
                    {
                        if (int.TryParse(intString, out var intensity))
                        {
                            intensities.Add(intensity);
                        }
                    }
                }
            }
            return intensities;
        }
    }
}

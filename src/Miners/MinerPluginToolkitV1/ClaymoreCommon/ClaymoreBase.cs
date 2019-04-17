using MinerPlugin;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.ClaymoreCommon
{

    public abstract class ClaymoreBase : MinerBase
    {
        public abstract override Tuple<string, string> GetBinAndCwdPaths();
        public abstract override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard);

        protected int _apiPort;
        protected readonly string _uuid;

        // TODO rename first and second
        // this is second algorithm - if this is null only dagger is being mined
        public AlgorithmType _algorithmSingleType;
        public AlgorithmType _algorithmDualType;

        public string _devices;
        public string _extraLaunchParameters = "";

        // command line parts
        public string _platform;

        public ClaymoreBase(string uuid)
        {
            _uuid = uuid;
        }

        private static int GetPlatformIDForType(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.AMD:
                    return 1;
                case DeviceType.NVIDIA:
                    return 2;
                default:
                    return 3;
            }
        }

        protected virtual string SingleAlgoName
        {
            get
            {
                switch (_algorithmSingleType)
                {
                    case AlgorithmType.DaggerHashimoto:
                        return "eth";
                    default:
                        return "";
                }
            }
        }

        protected virtual string DualAlgoName
        {
            get
            {
                switch (_algorithmDualType)
                {
                    case AlgorithmType.Decred:
                        return "dcr";
                    case AlgorithmType.Blake2s:
                        return "b2s";
                    case AlgorithmType.Keccak:
                        return "kc";
                    default:
                        return "";
                }
            }
        }

        public double DevFee
        {
            get
            {
                return 1.0;
            }
        }

        public double DualDevFee
        {
            get
            {
                return 0.0;
            }
        }

        public bool IsDual()
        {
            return (_algorithmDualType != AlgorithmType.NONE);
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmSingleType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");

            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmDualType = dualType.Item1;
            ok = dualType.Item2;
            if (!ok) _algorithmDualType = AlgorithmType.NONE;
            // all good continue on

            // Order pairs and parse ELP
            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            _devices = string.Join("", orderedMiningPairs.Select(p => p.Device.ID));
            _platform = $"{GetPlatformIDForType(orderedMiningPairs.First().Device.DeviceType)}";

            if (MinerOptionsPackage != null)
            {
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public string CreateCommandLine(string username)
        {
            var urlFirst = StratumServiceHelpers.GetLocationUrl(_algorithmSingleType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = "";
            if (_algorithmDualType == AlgorithmType.NONE) //noDual
            {
                cmd = $"-di {_devices} -platform {_platform} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dbg 1 {_extraLaunchParameters} -wd 0";
            }
            else
            {
                var urlSecond = StratumServiceHelpers.GetLocationUrl(_algorithmDualType, _miningLocation, NhmConectionType.STRATUM_TCP);
                cmd = $"-di {_devices} -platform {_platform} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dcoin {DualAlgoName} -dpool {urlSecond} -dwal {username} -dpsw x -dbg 1 {_extraLaunchParameters} -wd 0";
            }
            return cmd;
        }

        protected override string MiningCreateCommandLine()
        {
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange();
            return CreateCommandLine(_username) + $" -mport 127.0.0.1:-{_apiPort}";
        }

        public static readonly MinerOptionsPackage DefaultMinerOptionsPackage = new MinerOptionsPackage
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
     }
}

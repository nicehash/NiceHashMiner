using MinerPlugin;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.ClaymoreCommon
{

    public abstract class ClaymoreBase : MinerBase
    {
        public abstract override Tuple<string, string> GetBinAndCwdPaths();
        public abstract override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard);


        //protected List<MiningPair> _orderedMiningPairs = new List<MiningPair>();
        protected int _apiPort;
        
        public AlgorithmType _algorithmFirstType = AlgorithmType.NONE;
        public AlgorithmType _algorithmSecondType = AlgorithmType.NONE;

        public string _devices;
        public string _extraLaunchParameters = "";

        // command line parts
        protected Dictionary<string, int> _mappedIDs;

        public ClaymoreBase(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedIDs = mappedIDs;
        }

        protected virtual string SingleAlgoName
        {
            get
            {
                switch (_algorithmFirstType)
                {
                    case AlgorithmType.DaggerHashimoto:
                        return "eth";
                    default:
                        return "";
                }
            }
        }

#pragma warning disable 0618
        protected virtual string DualAlgoName
        {
            get
            {
                switch (_algorithmSecondType)
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
#pragma warning restore 0618

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
            return (_algorithmSecondType != AlgorithmType.NONE);
        }

        // override this because of the device ordering in the logs '_logGroup'
        public override void InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            // sort _miningPairs by _mappedIDs
            var orderedMiningPairs = miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => _mappedIDs[a.Device.UUID].CompareTo(_mappedIDs[b.Device.UUID]));
            _miningPairs = orderedMiningPairs;
            // update log group
            try
            {
                var devs = _miningPairs.Select(pair => $"{pair.Device.DeviceType}:{pair.Device.ID}");
                var devsTag = $"devs({string.Join(",", devs)})";
                var algo = _miningPairs.First().Algorithm.AlgorithmName;
                var algoTag = $"algo({algo})";
                _logGroup = $"{_baseTag}-{algoTag}-{devsTag}";
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error while setting _logGroup: {e.Message}");
            }
            Init();
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmFirstType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok)
            {
                Logger.Info(_logGroup, "Initialization of miner failed. Algorithm not found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }

            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmSecondType = dualType.Item1;
            ok = dualType.Item2;
            if (!ok) _algorithmSecondType = AlgorithmType.NONE;
            // all good continue on

            // mining pairs are ordered in InitMiningPairs
            _devices = string.Join("", _miningPairs.Select(p => ClaymoreHelpers.GetClaymoreDeviceID(_mappedIDs[p.Device.UUID])));

            if (MinerOptionsPackage != null)
            {
                var ignoreDefaults = MinerOptionsPackage.IgnoreDefaultValueOptions;
                var generalParams = ExtraLaunchParametersParser.Parse(_miningPairs.ToList(), MinerOptionsPackage.GeneralOptions, ignoreDefaults);
                var temperatureParams = ExtraLaunchParametersParser.Parse(_miningPairs.ToList(), MinerOptionsPackage.TemperatureOptions, ignoreDefaults);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public string CreateCommandLine(string username)
        {
            var urlFirst = StratumServiceHelpers.GetLocationUrl(_algorithmFirstType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = "";
            if (_algorithmSecondType == AlgorithmType.NONE) //noDual
            {
                cmd = $"-di {_devices} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 {_extraLaunchParameters} -wd 0";
            }
            else
            {
                var urlSecond = StratumServiceHelpers.GetLocationUrl(_algorithmSecondType, _miningLocation, NhmConectionType.STRATUM_TCP);
                cmd = $"-di {_devices} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dcoin {DualAlgoName} -dpool {urlSecond} -dwal {username} -dpsw x {_extraLaunchParameters} -wd 0";
            }
            return cmd;
        }

        protected override string MiningCreateCommandLine()
        {
            _apiPort = GetAvaliablePort();
            return CreateCommandLine(_username) + $" -mport 127.0.0.1:-{_apiPort}";
        }

     }
}

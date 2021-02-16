using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1.ClaymoreCommon
{

    public abstract class ClaymoreBase : MinerBase
    {
        protected int _apiPort;
        public AlgorithmType _algorithmSecondType = AlgorithmType.NONE;

        public string _devices;

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
                switch (_algorithmType)
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
                    default:
                        return "";
                }
            }
        }
#pragma warning restore 0618

        public virtual double DevFee
        {
            get
            {
                return 1.0;
            }
        }

        public virtual double DualDevFee
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

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by mapped ids
            pairsList.Sort((a, b) => _mappedIDs[a.Device.UUID].CompareTo(_mappedIDs[b.Device.UUID]));
            return pairsList;
        }

        protected override void Init()
        {
            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmSecondType = dualType.Item1;
            var ok = dualType.Item2;
            if (!ok) _algorithmSecondType = AlgorithmType.NONE;
            // all good continue on

            // mining pairs are ordered in InitMiningPairs
            var mappedDeviceIDs = _miningPairs.Select(p => ClaymoreHelpers.GetClaymoreDeviceID(_mappedIDs[p.Device.UUID]));
            _devices = string.Join("", mappedDeviceIDs);
        }

        public virtual string CreateCommandLine(string username)
        {
            var urlFirst = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
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

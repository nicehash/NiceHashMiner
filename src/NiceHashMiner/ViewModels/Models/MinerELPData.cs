using NHM.Common;
using NHM.MinerPluginToolkitV1.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static NHM.MinerPluginToolkitV1.CommandLine.MinerConfigManager;

namespace NiceHashMiner.ViewModels.Models
{
    public class MinerELPData : NotifyChangedBase
    {
        public string Name { get; set; }
        public string UUID { get; set; }
        private List<string> _singleParams { get; set; } = new List<string>();
        public List<string> SingleParams
        {
            get { return _singleParams; }
            set
            {
                _singleParams = value;
                OnPropertyChanged(nameof(SingleParams));
            }
        }
        public void UpdateSingleParams(string singleTxt)
        {
            SingleParams = Regex.Replace(singleTxt, @"\s+", " ").Trim().Split(" ").ToList();
        }
        private List<(string name, string value)> _doubleParams { get; set; } = new List<(string name, string value)>();
        public List<(string name, string value)> DoubleParams
        {
            get { return _doubleParams; }
            set
            {
                _doubleParams = value;
                OnPropertyChanged(nameof(DoubleParams));
            }
        }
        public bool UpdateDoubleParams(string doubleTxt)
        {
            var doubles = Regex.Replace(doubleTxt, @"\s+", " ").Trim().Split(" ").ToList();
            var doubleParams = new List<(string name, string value)>();
            if (doubles.Count % 2 != 0)
            {
                DoubleParams = new List<(string name, string value)>();
                return false;
            }
            for (int i = 0; i < doubles.Count; i++)
            {
                if (i % 2 == 0 || i == 0) continue;
                doubleParams.Add((doubles[i - 1], doubles[i]));
            }
            DoubleParams = doubleParams;
            return true;
        }

        private IEnumerable<AlgoELPData> _algos;
        public IEnumerable<AlgoELPData> Algos
        {
            get => _algos;
            set
            {
                _algos = value;
                OnPropertyChanged(nameof(Algos));
            }
        }
        public string SingleParamString
        {
            get
            {
                return String.Join(' ', SingleParams) ?? "";
            }
        }
        public string DoubleParamString
        {
            get
            {
                return String.Join(' ', DoubleParams.Select(t => $"{t.name} {t.value}")) ?? "";
            }
        }



        public void IterateSubModelsAndConstructELPs()
        {
            List<List<string>> minerParams = new();
            foreach (var single in SingleParams)
            {
                minerParams.Add(new List<string>() { single });
            }
            foreach (var dbl in DoubleParams)
            {
                minerParams.Add(new List<string>() { dbl.name, dbl.value });
            }
            foreach (var algo in Algos)
            {
                List<List<string>> algoParams = new();
                if (algo.SingleParams == null) algo.SingleParams = new();
                foreach (var single in algo.SingleParams)
                {
                    algoParams.Add(new List<string>() { single });
                }
                if(algo.DoubleParams == null) algo.DoubleParams = new();
                foreach (var dbl in algo.DoubleParams)
                {
                    algoParams.Add(new List<string>() { dbl.name, dbl.value });
                }
                var header = algo.Devices.FirstOrDefault();
                if (header == null || !header.IsDeviceDataHeader) continue;
                List<List<List<string>>> devParams = new();
                if (algo.Devices == null) algo.Devices = new();
                foreach (var dev in algo.Devices)
                {
                    if (dev.IsDeviceDataHeader) continue;
                    if (header.ELPs == null || dev.ELPs == null) continue;
                    if (header.ELPs.Count != dev.ELPs.Count) continue;
                    List<List<string>> oneDevParams = new();
                    for (int i = 0; i < dev.ELPs.Count; i++)
                    {
                        if (header.ELPs[i].ELP == null) continue;
                        var flagAndDelim = header.ELPs[i].ELP.Trim().Split(' ');
                        if (flagAndDelim.Length != 2) continue;
                        oneDevParams.Add(new List<string> { flagAndDelim[0], dev.ELPs[i].ELP, flagAndDelim[1] });
                    }
                    devParams.Add(oneDevParams);
                }
                algo.ParsedString = MinerExtraParameters.Parse(minerParams, algoParams, devParams);


                Dictionary<HashSet<int>, List<List<List<string>>>> deviceParamsGroups = new Dictionary<HashSet<int>, List<List<List<string>>>>();
                for (int first = 0; first < devParams.Count; first++)
                {
                    var isPartOfGroup = deviceParamsGroups.Keys.Any(keys => keys.Contains(first));
                    if (isPartOfGroup) continue;
                    var group = new HashSet<int> { first };
                    for (int second = first+1; second < devParams.Count; second++)
                    {
                        if (MinerExtraParameters.CheckIfCanGroup(new List<List<List<string>>> { devParams[first], devParams[second] })) group.Add(second);
                    }
                    deviceParamsGroups.Add(group, devParams.Where((_, index) => group.Contains(index)).ToList());
                }


                //var canGroup = MinerExtraParameters.CheckIfCanGroup(devParams);

                //var test = MinerExtraParameters.GetAllInstanceCommands(minerParams, algoParams, devParams);
            }
        }
        public void ClearSingleParams()
        {
            SingleParams.Clear();
        }
        public void ClearDoubleParams()
        {
            DoubleParams.Clear();
        }
        public void UpdateMinerELPConfig()
        {
            var config = ConstructConfig();
            MinerConfigManager.WriteConfig(config);
        }
        private MinerConfig ConstructConfig()
        {
            var minerConfig = new MinerConfig();
            minerConfig.MinerName = Name;
            minerConfig.MinerUUID = UUID;
            foreach (var single in SingleParams)
            {
                minerConfig.MinerCommands.Add(new List<string>() { single });
            }
            foreach (var dbl in DoubleParams)
            {
                minerConfig.MinerCommands.Add(new List<string>() { dbl.name, dbl.value });
            }
            foreach (var algo in Algos)
            {
                var tempAlgo = new Algo();
                tempAlgo.AlgorithmName = algo.Name;
                foreach (var single in algo.SingleParams)
                {
                    tempAlgo.AlgoCommands.Add(new List<string>() { single });
                }
                foreach (var dbl in algo.DoubleParams)
                {
                    tempAlgo.AlgoCommands.Add(new List<string>() { dbl.name, dbl.value });
                }
                var header = algo.Devices.FirstOrDefault();
                if (header == null || !header.IsDeviceDataHeader) continue;
                foreach (var dev in algo.Devices)
                {
                    var deviceParams = new List<List<string>>();
                    if (dev.IsDeviceDataHeader) continue;
                    for (int i = 0; i < dev.ELPs.Count; i++)
                    {
                        if (header.ELPs[i].ELP == null) continue;
                        var flagAndDelim = header.ELPs[i].ELP.Trim().Split(' ');
                        if (flagAndDelim.Length != 2) continue;
                        deviceParams.Add(new List<string> { flagAndDelim[0], dev.ELPs[i].ELP, flagAndDelim[1] });
                    }
                    tempAlgo.Devices.Add(dev.UUID, new Device() { DeviceName = dev.DeviceName, Commands = deviceParams });
                }
                minerConfig.Algorithms.Add(tempAlgo);
            }
            return minerConfig;
        }
    }
}

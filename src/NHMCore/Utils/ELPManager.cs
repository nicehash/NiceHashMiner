using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.CommandLine;
using NHMCore.Configs.ELPDataModels;
using NHMCore.Mining;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NHM.MinerPluginToolkitV1.CommandLine.MinerConfigManager;

namespace NHMCore.Utils
{
    public delegate void NotifyELPChangeEventHandler(object sender, EventArgs e);
    public class ELPManager
    {
        private ELPManager() { }
        public static ELPManager Instance { get; } = new ELPManager();
        public event NotifyELPChangeEventHandler ELPReiteration;

        private IEnumerable<MinerELPData> _minerELPs;
        public IEnumerable<MinerELPData> GetMinerELPs()
        {
            return _minerELPs;
        }
        public void SetMinerELPs(IEnumerable<MinerELPData> newValues)
        {
            _minerELPs = newValues;
        }

        protected virtual void OnChanged(EventArgs e)
        {
            if (ELPReiteration != null) ELPReiteration(this, e);
        }
        public void NotifyELPReiteration()
        {
            OnChanged(EventArgs.Empty);
        }
        public MinerConfig CreateDefaultConfig(string pluginName, string pluginUUID,
            Dictionary<string, List<string>> supportedDevicesAlgorithms,
            List<(string FullName, string Uuid, DeviceType deviceType)> devices)
        {
            MinerConfig defCfg = new();
            defCfg.MinerName = pluginName;
            defCfg.MinerUUID = pluginUUID;
            if (pluginName.ToLower().Contains("xmr")) defCfg.MinerCommands.Add(new List<string>() { "--cpu-priority",  "0" });
            Dictionary<string, List<(string uuid, string name)>> algorithmDevicePairs = new(); 
            foreach (var devAlgoPair in supportedDevicesAlgorithms)
            {
                foreach (var algo in devAlgoPair.Value)
                {
                    if (!algorithmDevicePairs.ContainsKey(algo)) algorithmDevicePairs.Add(algo, new List<(string, string)>());
                    var devs = devices.Where(dev => dev.deviceType.ToString().Contains(devAlgoPair.Key))?
                        .Select(dev => new { P = (dev.Uuid, dev.FullName) })?
                        .Select(p => p.P);
                    if(devs != null) algorithmDevicePairs[algo].AddRange(devs);
                }
            }
            foreach (var algoPairs in algorithmDevicePairs)
            {
                var devicesDict = new Dictionary<string, Device>();
                algoPairs.Value.ForEach(dev => devicesDict.TryAdd(dev.uuid, new Device() { DeviceName = dev.name, Commands = new List<List<string>>() }));
                defCfg.Algorithms.Add(new Algo() { AlgorithmName = algoPairs.Key, Devices = devicesDict });
            }
            return defCfg;
        }

        private MinerConfig CompareConfigWithDefaultAndFixIfNeeded(MinerConfig data, MinerConfig def)
        {
            if (data.MinerUUID != def.MinerUUID) data.MinerUUID = def.MinerUUID;
            if (data.MinerName != def.MinerName) data.MinerName = def.MinerName;
            def.MinerCommands = data.MinerCommands;
            foreach (var algorithm in def?.Algorithms)
            {
                var containedAlgo = data.Algorithms?.Where(a => a.AlgorithmName == algorithm.AlgorithmName)?.FirstOrDefault();
                if (containedAlgo == null) continue;
                algorithm.AlgoCommands = containedAlgo?.AlgoCommands;
                foreach (var dev in algorithm?.Devices)
                {
                    var containedDev = containedAlgo.Devices?.Where(a => a.Key == dev.Key).FirstOrDefault();
                    if (containedDev == null || containedDev?.Value == null) continue;
                    algorithm.Devices[dev.Key] = containedDev?.Value;
                }
            }
            return def;
        }
        public MinerConfig FixConfigIntegrityIfNeeded(MinerConfig data, string pluginName, string pluginUUID, 
            Dictionary<string, List<string>> supportedDevicesAlgorithms,
            List<(string FullName, string Uuid, DeviceType deviceType)> devices)
        {
            var def = CreateDefaultConfig(pluginName, pluginUUID, supportedDevicesAlgorithms, devices);
            try
            {
                def = CompareConfigWithDefaultAndFixIfNeeded(data, def);
            }
            catch (Exception ex)
            {
                Logger.Error("MainVM", $"IsConfigIntegrityOK {ex.Message}");
            }
            return def;
        }

        public void UpdateMinerELPConfig()
        {
            foreach(var miner in _minerELPs)
            {
                var config = ConstructConfigFromMinerELPData(miner);
                WriteConfig(config);
            }
        }
        public void IterateSubModelsAndConstructELPs()
        {
            foreach (var miner in _minerELPs)
            {
                IterateSubModelsAndConstructELPsForPlugin(miner);
            }
        }

        public DeviceELPData FindDeviceNode(AlgorithmContainer ac, string uuid)
        {
            if (ac == null) return null;
            return _minerELPs
                .Where(miner => miner.UUID == ac.MinerUUID)?
                .FirstOrDefault()?
                .Algos.Where(algo => algo.Name == ac.AlgorithmName)?
                .FirstOrDefault()?
                .Devices.Where(dev => dev.UUID == uuid)?
                .FirstOrDefault();
        }
        public string FindAppropriateCommandForAlgoContainer(AlgorithmContainer ac)
        {
            if(ac == null) return string.Empty;
            return _minerELPs
                .Where(miner => miner.UUID == ac.MinerUUID)?
                .FirstOrDefault()?
                .Algos.Where(algo => algo.Name == ac.AlgorithmName)?
                .FirstOrDefault()?
                .AllCMDStrings.Where(x => x.uuid == ac.ComputeDevice.Uuid)?
                .Select(x => x.command)?
                .FirstOrDefault() ?? String.Empty;
        }
        public MinerELPData ConstructMinerELPDataFromConfig(MinerConfig cfg)
        {
            var minerELP = new MinerELPData();
            minerELP.Name = cfg.MinerName;
            minerELP.UUID = cfg.MinerUUID;
            foreach (var minerCMD in cfg.MinerCommands)
            {
                if (minerCMD.Count == 1) minerELP.SingleParams.Add(minerCMD.First());
                if (minerCMD.Count == 2) minerELP.DoubleParams.Add((minerCMD.First(), minerCMD.Last()));
            }
            var algoELPList = new List<AlgoELPData>();
            foreach (var algo in cfg.Algorithms)
            {
                var tempAlgo = new AlgoELPData();
                var uniqueFlags = algo.Devices.Values?
                    .Select(v => v.Commands.Where(c => c.Count == 3)?.Select(a => $"{a[0]} {a[2]}"))?
                    .SelectMany(v => v)?
                    .Distinct()?
                    .ToList();
                uniqueFlags?.ForEach(f => tempAlgo.Devices[0].AddELP(f));
                if (!uniqueFlags.Any()) tempAlgo.Devices[0].ELPs.Add(new DeviceELPElement(false) { ELP = String.Empty });
                tempAlgo.Name = algo.AlgorithmName;
                foreach (var dev in algo.Devices)
                {
                    var tempELPElts = new DeviceELPElement[uniqueFlags?.Count + 1 ?? 1];
                    tempELPElts[tempELPElts.Length - 1] = new DeviceELPElement() { ELP = String.Empty };
                    foreach (var arg in dev.Value.Commands)
                    {
                        if (arg.Count != 3) continue;
                        var index = uniqueFlags.IndexOf($"{arg[0]} {arg[2]}");
                        if (index < 0) continue;
                        tempELPElts[index] = new DeviceELPElement() { ELP = arg[1] };
                    }
                    tempELPElts.Where(elt => elt == null)?.ToList()?.ForEach(elt => elt = new DeviceELPElement() { ELP = string.Empty });
                    tempAlgo.Devices.Add(new DeviceELPData()
                    {
                        UUID = dev.Key,
                        DeviceName = dev.Value.DeviceName,
                        ELPs = new ObservableCollection<DeviceELPElement>(tempELPElts)
                    });
                }
                foreach (var algoCMD in algo.AlgoCommands)
                {
                    if (algoCMD.Count == 1) tempAlgo.SingleParams.Add(algoCMD.First());
                    if (algoCMD.Count == 2) tempAlgo.DoubleParams.Add((algoCMD.First(), algoCMD.Last()));
                }
                algoELPList.Add(tempAlgo);
            }
            minerELP.Algos = algoELPList.ToArray();
            return minerELP;
        }
        private MinerConfig ConstructConfigFromMinerELPData(MinerELPData miner)
        {
            var minerConfig = new MinerConfig();
            minerConfig.MinerName = miner.Name;
            minerConfig.MinerUUID = miner.UUID;
            miner.SingleParams.ForEach(single => minerConfig.MinerCommands.Add(new List<string>() { single }));
            miner.DoubleParams.ForEach(dbl => minerConfig.MinerCommands.Add(new List<string>() { dbl.name, dbl.value }));
            foreach (var algo in miner.Algos)
            {
                var tempAlgo = new Algo();
                tempAlgo.AlgorithmName = algo.Name;
                if (algo.SingleParams == null) algo.SingleParams = new();
                algo.SingleParams.ForEach(single => tempAlgo.AlgoCommands.Add(new List<string>() { single }));
                if (algo.DoubleParams == null) algo.DoubleParams = new();
                algo.DoubleParams.ForEach(dbl => tempAlgo.AlgoCommands.Add(new List<string>() { dbl.name, dbl.value }));
                var header = algo.Devices.FirstOrDefault();
                if (header == null || !header.IsDeviceDataHeader) continue;
                foreach (var dev in algo.Devices)
                {
                    var deviceParams = new List<List<string>>();
                    if (dev.IsDeviceDataHeader) continue;
                    for (int i = 0; i < dev.ELPs.Count; i++)
                    {
                        if (header.ELPs[i] == null || header.ELPs[i].ELP == null) continue;
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
        private void IterateSubModelsAndConstructELPsForPlugin(MinerELPData miner)
        {
            List<List<string>> minerParams = new();
            miner.SingleParams.ForEach(single => minerParams.Add(new List<string>() { single }));
            miner.DoubleParams.ForEach(dbl => minerParams.Add(new List<string>() { dbl.name, dbl.value }));
            foreach (var algo in miner.Algos)
            {
                var shouldAddnewColumn = false;
                List<List<string>> algoParams = new();
                if (algo.SingleParams == null) algo.SingleParams = new();
                algo.SingleParams.ForEach(single => algoParams.Add(new List<string>() { single }));
                if (algo.DoubleParams == null) algo.DoubleParams = new();
                algo.DoubleParams.ForEach(dbl => algoParams.Add(new List<string>() { dbl.name, dbl.value }));
                var header = algo.Devices.FirstOrDefault();
                if (header == null || !header.IsDeviceDataHeader) continue;
                var columnToDelete = header.ELPs
                    .Select((elp, index) => new { elp, index })
                    .Where(item => string.IsNullOrEmpty(item.elp.ELP))
                    .FirstOrDefault();
                bool shouldDelete = false;
                if(columnToDelete != null) shouldDelete = columnToDelete.index < header.ELPs.Count - 1;
                List<(string devUUID, List<List<string>> paramList)> devParams = new();
                if (algo.Devices == null) algo.Devices = new();
                foreach (var dev in algo.Devices)
                {
                    if (dev.IsDeviceDataHeader && !string.IsNullOrEmpty(dev.ELPs?.LastOrDefault()?.ELP)) shouldAddnewColumn = true;
                    if (shouldAddnewColumn) dev.ELPs.Add(new DeviceELPElement(!dev.IsDeviceDataHeader) { ELP = String.Empty });
                    if (header.ELPs == null || dev.ELPs == null) continue;
                    if (columnToDelete != null && shouldDelete) dev.ELPs.RemoveAt(columnToDelete.index);
                    if (header.ELPs.Count != dev.ELPs.Count) continue;
                    List<List<string>> oneDevParams = new();
                    for (int i = 0; i < dev.ELPs.Count; i++)
                    {
                        var flagAndDelim = header.ELPs[i].ELP.Trim().Split(' ');
                        if (flagAndDelim.Length != 2) continue;
                        oneDevParams.Add(new List<string> { flagAndDelim[0], dev.ELPs[i].ELP, flagAndDelim[1] });
                    }
                    devParams.Add((dev.UUID, oneDevParams));
                    dev.ConstructedELPs = oneDevParams;
                }
                Dictionary<HashSet<int>, List<(string devUUID, List<List<string>> paramList)>> deviceParamsGroups = new();
                for (int first = 1; first < devParams.Count; first++)
                {
                    var isPartOfGroup = deviceParamsGroups.Keys.Any(keys => keys.Contains(first));
                    if (isPartOfGroup) continue;
                    var group = new HashSet<int> { first };
                    for (int second = first + 1; second < devParams.Count; second++)
                    {
                        if (MinerExtraParameters.CheckIfCanGroup(new List<List<List<string>>> { devParams[first].paramList, devParams[second].paramList })) group.Add(second);
                    }
                    var selectionGroup = devParams.Where((_, index) => group.Contains(index)).ToList();
                    deviceParamsGroups.Add(group,  selectionGroup);
                }
                var parsedCommandsPerGroup = new List<(string uuid, string command)>();
                foreach (var dev in deviceParamsGroups)
                {
                    var uuidList = dev.Value.Select(k => k.devUUID).ToList();
                    var command = MinerExtraParameters.Parse(minerParams, algoParams, dev.Value.Select(v => v.paramList).ToList());
                    foreach(var uuid in uuidList)
                    {
                        parsedCommandsPerGroup.Add((uuid, command));
                    }
                }
                algo.AllCMDStrings = new ObservableCollection<(string uuid, string command)>(parsedCommandsPerGroup);
            }
        }
    }
}

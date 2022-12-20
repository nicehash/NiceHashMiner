using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.CommandLine;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Configs.ELPDataModels;
using NHMCore.Mining;
using NHMCore.Nhmws;
using NHMCore.Nhmws.V4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static NHM.MinerPluginToolkitV1.CommandLine.MinerConfigManager;

namespace NHMCore.Configs.Managers
{
    public delegate void NotifyELPChangeEventHandler(object sender, EventArgs e);
    public class ELPManager
    {
        private ELPManager() { }
        public static ELPManager Instance { get; } = new ELPManager();
        private readonly string _TAG = "ELPManager";
        public event NotifyELPChangeEventHandler ELPReiteration;
        const int HEADER = 0;
        const int FLAG = 0;
        const int VALUE = 1;
        const int DELIMITER = 2;

        private IEnumerable<MinerELPData> _minerELPs = Enumerable.Empty<MinerELPData>();
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
        public MinerConfig CreateDefaultConfig(PluginConfiguration pconf)
        {
            MinerConfig defCfg = new();
            defCfg.MinerName = pconf.PluginName;
            defCfg.MinerUUID = pconf.PluginUUID;
            defCfg.MinerCommands.AddRange(pconf.MinerSpecificCommands);
            Dictionary<string, List<(string uuid, string name)>> algorithmDevicePairs = new();
            foreach (var devAlgoPair in pconf.SupportedDevicesAlgorithms)
            {
                foreach (var algo in devAlgoPair.Value)
                {
                    if (!algorithmDevicePairs.ContainsKey(algo)) algorithmDevicePairs.Add(algo, new List<(string, string)>());
                    var devs = pconf.Devices.Where(dev => dev.deviceType.ToString().Contains(devAlgoPair.Key))?
                        .Select(dev => new { P = (dev.Uuid, dev.FullName) })?
                        .Select(p => p.P);
                    if (devs != null) algorithmDevicePairs[algo].AddRange(devs);
                }
            }
            foreach (var algoPairs in algorithmDevicePairs)
            {
                var devicesDict = new Dictionary<string, MinerConfigManager.Device>();
                algoPairs.Value.ForEach(dev => devicesDict.TryAdd(dev.uuid, new MinerConfigManager.Device() { DeviceName = dev.name, Commands = new List<List<string>>() }));
                defCfg.Algorithms.Add(new MinerConfigManager.Algo() { AlgorithmName = algoPairs.Key, Devices = devicesDict });
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
        public MinerConfig FixConfigIntegrityIfNeeded(MinerConfig data, PluginConfiguration pconf)
        {
            var def = CreateDefaultConfig(pconf);
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
            foreach (var miner in _minerELPs)
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
                miner.UpdateProperties();
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
            if (ac == null) return string.Empty;
            return _minerELPs
                .Where(miner => miner.UUID == ac.MinerUUID)?
                .FirstOrDefault()?
                .Algos.Where(algo => algo.Name == ac.AlgorithmName)?
                .FirstOrDefault()?
                .AllCMDStrings.Where(x => x.uuid == ac.ComputeDevice.Uuid)?
                .Select(x => x.command)?
                .FirstOrDefault() ?? string.Empty;
        }
        public void SetAlgoCMDString(AlgorithmContainer ac, string newCMD)
        {
            if (ac == null) return;
            var target = _minerELPs
                .Where(miner => miner.UUID == ac.MinerUUID)?
                .FirstOrDefault()?
                .Algos.Where(algo => algo.Name == ac.AlgorithmName)?
                .FirstOrDefault();
            if(target == null) return;


            var index = target.AllCMDStrings.ToList().FindIndex(i => i.uuid == ac.ComputeDevice.Uuid);
            if (index == -1) return;
            target.AllCMDStrings.RemoveAt(index);
            target.AllCMDStrings.Add((ac.ComputeDevice.Uuid, newCMD));
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
                    .Select(v => v.Commands.Where(c => c.Count == 3)?.Select(a => $"{a[FLAG]} {a[DELIMITER]}"))?
                    .SelectMany(v => v)?
                    .Distinct()?
                    .ToList();
                uniqueFlags?.ForEach(f => tempAlgo.Devices[HEADER].AddELP(f));
                if (!uniqueFlags.Any()) tempAlgo.Devices[HEADER].ELPs.Add(new DeviceELPElement(false) { ELP = string.Empty });
                tempAlgo.Name = algo.AlgorithmName;
                foreach (var dev in algo.Devices)
                {
                    var tempELPElts = new DeviceELPElement[uniqueFlags?.Count + 1 ?? 1];
                    tempELPElts[tempELPElts.Length - 1] = new DeviceELPElement() { ELP = string.Empty };
                    foreach (var arg in dev.Value.Commands)
                    {
                        if (arg.Count != 3) continue;
                        var index = uniqueFlags.IndexOf($"{arg[FLAG]} {arg[DELIMITER]}");
                        if (index < 0) continue;
                        tempELPElts[index] = new DeviceELPElement() { ELP = arg[VALUE] };
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
                var tempAlgo = new MinerConfigManager.Algo();
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
                    for (var i = 0; i < dev.ELPs.Count; i++)
                    {
                        if (header.ELPs[i] == null || header.ELPs[i].ELP == null) continue;
                        var flagAndDelim = header.ELPs[i].ELP.Trim().Split(' ');
                        if (flagAndDelim.Length != 2) continue;
                        deviceParams.Add(new List<string> { flagAndDelim[0], dev.ELPs[i].ELP, flagAndDelim[1] });
                    }
                    tempAlgo.Devices.Add(dev.UUID, new MinerConfigManager.Device() { DeviceName = dev.DeviceName, Commands = deviceParams });
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
                algo.CombinedParams = MinerExtraParameters.ParseAlgoPreview(minerParams, algoParams);
                var header = algo.Devices.FirstOrDefault();
                if (header == null || !header.IsDeviceDataHeader) continue;
                var columnToDelete = header.ELPs
                    .Select((elp, index) => new { elp, index })
                    .Where(item => string.IsNullOrEmpty(item.elp.ELP.Trim()))
                    .FirstOrDefault();
                var shouldDelete = false;
                if (columnToDelete != null) shouldDelete = columnToDelete.index < header.ELPs.Count - 1;
                List<(string devUUID, List<List<string>> paramList)> devParams = new();
                if (algo.Devices == null) algo.Devices = new();
                foreach (var dev in algo.Devices)
                {
                    if (dev.IsDeviceDataHeader && !string.IsNullOrEmpty(dev.ELPs?.LastOrDefault()?.ELP)) shouldAddnewColumn = true;
                    if (shouldAddnewColumn) dev.ELPs.Add(new DeviceELPElement(!dev.IsDeviceDataHeader) { ELP = string.Empty });
                    if (header.ELPs == null || dev.ELPs == null) continue;
                    if (columnToDelete != null && shouldDelete) dev.ELPs.RemoveAt(columnToDelete.index);
                    if (header.ELPs.Count != dev.ELPs.Count) continue;
                    List<List<string>> oneDevParams = new();
                    for (var i = 0; i < dev.ELPs.Count; i++)
                    {
                        var flagAndDelim = header.ELPs[i].ELP.Trim().Split(' ');
                        if (flagAndDelim.Length != 2) continue;
                        if (flagAndDelim[1] == "$ws$") flagAndDelim[1] = " ";
                        if (dev.ELPs[i].ELP == String.Empty) continue;
                        oneDevParams.Add(new List<string> { flagAndDelim[0], dev.ELPs[i].ELP, flagAndDelim[1] });
                    }
                    devParams.Add((dev.UUID, oneDevParams));
                    dev.ConstructedELPs = oneDevParams;
                }
                Dictionary<HashSet<int>, List<(string devUUID, List<List<string>> paramList)>> deviceParamsGroups = new();
                for (var first = 1; first < devParams.Count; first++)
                {
                    var isPartOfGroup = deviceParamsGroups.Keys.Any(keys => keys.Contains(first));
                    if (isPartOfGroup) continue;
                    var group = new HashSet<int> { first };
                    for (var second = first + 1; second < devParams.Count; second++)
                    {
                        if (MinerExtraParameters.CheckIfCanGroup(new List<List<List<string>>> { devParams[first].paramList, devParams[second].paramList })) group.Add(second);
                    }
                    var selectionGroup = devParams.Where((_, index) => group.Contains(index)).ToList();
                    deviceParamsGroups.Add(group, selectionGroup);
                }
                var parsedCommandsPerGroup = new List<(string uuid, string command)>();
                foreach (var dev in deviceParamsGroups)
                {
                    var uuidList = dev.Value.Select(k => k.devUUID).ToList();
                    var command = MinerExtraParameters.Parse(minerParams, algoParams, dev.Value.Select(v => v.paramList).ToList());
                    foreach (var uuid in uuidList)
                    {
                        parsedCommandsPerGroup.Add((uuid, command));
                    }
                }
                algo.AllCMDStrings = new ObservableCollection<(string uuid, string command)>(parsedCommandsPerGroup);
            }
        }
#if NHMWS4
        public Task<(ErrorCode err, string msg)> ExecuteTest(string uuid, ElpBundle bundle)
        {
            if (!MiningState.Instance.AnyDeviceRunning) return Task.FromResult((ErrorCode.ErrNoDeviceRunning, "No devices mining"));
            var allContainers = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .Where(d => d.State == DeviceState.Mining || d.State == DeviceState.Testing)?
                .SelectMany(d => d.AlgorithmSettings);
            if (allContainers == null || !allContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "No targets found"));

            List<AlgorithmContainer> specificContainers = allContainers.ToList();
            if (bundle.AlgoId != null && bundle.MinerId != null) specificContainers = allContainers.Where(d =>
                                                                                        bundle.AlgoId.Contains(d.AlgorithmName.ToLower()) &&
                                                                                        bundle.MinerId.Contains(d.PluginName.ToLower()))?.ToList();
            else if (bundle.AlgoId != null) specificContainers = allContainers.Where(d => bundle.AlgoId.Contains(d.AlgorithmName.ToLower()))?.ToList();
            else if (bundle.MinerId != null) specificContainers = allContainers.Where(d => bundle.MinerId.Contains(d.PluginName.ToLower()))?.ToList();
            if (specificContainers == null || !specificContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Action target mismatch, containers null"));
            var target = specificContainers.Where(c => c.IsCurrentlyMining)?.FirstOrDefault();
            if (target == null)
            {
                target = specificContainers.FirstOrDefault();
                if (target == null) return Task.FromResult((ErrorCode.TargetContainerNotFound, "Failed to switch to target algorithm container"));
            }
            //AvailableDevices.Devices //if we want switching for loose options we can set true to specific containers in the future
            //    .Where(d => d.B64Uuid == uuid)?
            //    .SelectMany(d => d.AlgorithmSettings)?
            //    .ToList()?
            //    .ForEach(c => c.IsTesting = false);

            target.SetTargetElpProfile(bundle, true);
            MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
        public Task<(ErrorCode err, string msg)> StopTest(string uuid, bool triggerSwitch)
        {
            var targetDeviceContainer = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .SelectMany(d => d.AlgorithmSettings)?
                .Where(a => a.IsTesting || a.ActiveELPTestProfile != null)?
                .FirstOrDefault();
            if (targetDeviceContainer == null)
            {
                Logger.Error(_TAG, "Device not found for stop ELP test");
                return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Device not found"));
            }
            targetDeviceContainer.SetTargetElpProfile(null, true);
            if(triggerSwitch) MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
        public Task<(ErrorCode err, string msg)> ApplyELPBundle(List<ElpBundle> bundles)
        {
            if (bundles == null) return Task.FromResult((ErrorCode.NoError, "ELPBundles == null"));
            List<AlgorithmContainer> processed = new();
            var sorted = new List<(int, ElpBundle)>();
            foreach (var bundle in bundles)
            {
                if (bundle.MinerId != null && bundle.AlgoId != null) sorted.Add((0, bundle));
                else if (bundle.MinerId == null && bundle.AlgoId != null) sorted.Add((1, bundle));
                else if (bundle.MinerId != null && bundle.AlgoId == null) sorted.Add((2, bundle));
                else sorted.Add((3, bundle));
            }
            sorted = sorted.OrderBy(item => item.Item1).ToList();
            foreach (var (type, bundle) in sorted)
            {
                var current = new List<AlgorithmContainer>();
                if (type == 0) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.AlgoId.Contains(c.AlgorithmName.ToLower()))?
                        .Where(c => bundle.MinerId.Contains(c.PluginName.ToLower()))?
                        .ToList();
                else if (type == 1) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.AlgoId.Contains(c.AlgorithmName.ToLower()))?
                        .ToList();
                else if (type == 2) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.MinerId.Contains(c.PluginName.ToLower()))?
                        .ToList();
                else current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .ToList();
                if (current == null) continue;
                current = current.Where(c => !processed.Contains(c)).ToList();
                processed.AddRange(current);
                foreach (var container in current)
                {
                    Logger.Warn(_TAG, $"\t{container.ComputeDevice.ID}-{container.ComputeDevice.Name}/{container.AlgorithmName}/{container.PluginName}");
                    container.SetTargetElpProfile(bundle, false);
                }
            }
            MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }

        public Task ResetELPBundle(bool triggerSwitch = true)
        {
            var containers = AvailableDevices.Devices.SelectMany(d => d.AlgorithmSettings);
            foreach (var container in containers)
            {
                container.SetTargetElpProfile(null, false);
            }
            if(triggerSwitch) MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
#endif
    }
}

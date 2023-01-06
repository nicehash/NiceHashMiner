using HidSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.DeviceMonitoring;
using NHM.DeviceMonitoring.Core_clock;
using NHM.DeviceMonitoring.Memory_clock;
using NHM.DeviceMonitoring.TDP;
using NHMCore.Configs;
using NHMCore.Configs.Managers;
using NHMCore.Mining;
using NHMCore.Mining.MiningStats;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Nhmws.V4
{
    static class MessageParserV4
    {
        internal static IMethod ParseMessage(string jsonData)
        {
            var method = MessageParser.ParseMessageData(jsonData);
            return method switch
            {
                // non rpc
                "sma" => JsonConvert.DeserializeObject<SmaMessage>(jsonData),
                "markets" => new ObsoleteMessage { Method = method },
                "balance" => JsonConvert.DeserializeObject<BalanceMessage>(jsonData),
                "versions" => JsonConvert.DeserializeObject<VersionsMessage>(jsonData),
                "burn" => JsonConvert.DeserializeObject<BurnMessage>(jsonData),
                "exchange_rates" => JsonConvert.DeserializeObject<ExchangeRatesMessage>(jsonData),
                // rpc
                "mining.set.username" => JsonConvert.DeserializeObject<MiningSetUsername>(jsonData),
                "mining.set.worker" => JsonConvert.DeserializeObject<MiningSetWorker>(jsonData),
                "mining.set.group" => JsonConvert.DeserializeObject<MiningSetGroup>(jsonData),
                "mining.enable" => JsonConvert.DeserializeObject<MiningEnable>(jsonData),
                "mining.disable" => JsonConvert.DeserializeObject<MiningDisable>(jsonData),
                "mining.start" => JsonConvert.DeserializeObject<MiningStart>(jsonData),
                "mining.stop" => JsonConvert.DeserializeObject<MiningStop>(jsonData),
                "mining.set.power_mode" => JsonConvert.DeserializeObject<MiningSetPowerMode>(jsonData),
                "miner.reset" => JsonConvert.DeserializeObject<MinerReset>(jsonData),
                "miner.call.action" => JsonConvert.DeserializeObject<MinerCallAction>(jsonData),
                "miner.set.mutable" => JsonConvert.DeserializeObject<MinerSetMutable>(jsonData),
                // non supported
                _ => throw new Exception($"Unable to deserialize '{jsonData}' got method '{method}'."),
            };
        }
        internal static int NHMDeviceTypeToNHMWSDeviceType(DeviceType dt)
        {
            //rig manager enum
            //const deviceClasses = ['UNKNOWN','CPU','NVIDIA','AMD','ASIC',5,6,7,8,9,'ASIC']
            return dt switch
            {
                DeviceType.CPU => 1,
                DeviceType.NVIDIA => 2,
                DeviceType.AMD => 3,
                _ => 0
            };
        }

        internal static IOrderedEnumerable<ComputeDevice> SortedDevices(this IEnumerable<ComputeDevice> devices)
        {
            return devices.OrderBy(d => d.DeviceType)
                .ThenBy(d => d.BaseDevice is IGpuDevice gpu ? gpu.PCIeBusID : int.MinValue);
        }

        private static string GetDevicePlugin(string UUID)
        {
            var data = MiningDataStats.GetDevicesMiningStats();
            var devData = data.FirstOrDefault(dev => dev.DeviceUUID == UUID);
            if (devData == null)
            {
                var fallback = AvailableDevices.Devices
                    .Where(d => d.Uuid == UUID)?
                    .Where(d => d.State == DeviceState.Mining
                        || d.State == DeviceState.Benchmarking
                        || d.State == DeviceState.Testing)?
                    .SelectMany(d => d.AlgorithmSettings)?
                    .Where(a => a.IsCurrentlyMining)?
                    .FirstOrDefault();
                return fallback == null ? string.Empty : fallback.PluginName;
            }
            return devData.MinerName;
        }

        private static (List<(string name, string? unit)> properties, JArray values) GetDeviceOptionalDynamic(ComputeDevice d, bool isStateChange = false, bool isLogin = false)
        {
            string getValue<T>(T o) => (typeof(T).Name, o) switch
            {
                (nameof(ILoad), ILoad g) => $"{(int)g.Load}",
                (nameof(IMemControllerLoad), IMemControllerLoad g) => $"{g.MemoryControllerLoad}",
                (nameof(ITemp), ITemp g) => $"{g.Temp}",
                (nameof(IGetFanSpeedPercentage), IGetFanSpeedPercentage g) => $"{g.GetFanSpeedPercentage().percentage}",
                (nameof(IPowerUsage), IPowerUsage g) => $"{g.PowerUsage}",
                (nameof(IVramTemp), IVramTemp g) => $"{g.VramTemp}",
                (nameof(IHotspotTemp), IHotspotTemp g) => $"{g.HotspotTemp}",
                (_, _) => null,
            };

            string getValueForName(string name) => name switch
            {
                "Miner" => $"{GetDevicePlugin(d.Uuid)}",
                "OC profile" => $"{d.OCProfile}",
                "OC profile ID" => $"{d.OCProfileID}",
                "Fan profile" => $"{d.FanProfile}",
                "Fan profile ID" => $"{d.FanProfileID}",
                "ELP profile" => $"{d.ELPProfile}",
                "ELP profile ID" => $"{d.ELPProfileID}",
                _ => null,
            };

            (DeviceDynamicProperties type, string name, string unit, string value)? pairOrNull<T>(DeviceDynamicProperties type, string name, string unit)
            {
                if (d.DeviceMonitor is T sensor) return (type, name, unit, getValue<T>(sensor));
                if (typeof(T) == typeof(string)) return (type, name, unit, getValueForName(name));
                return null;
            }

            // here sort manually by type 
            var dynamicPropertiesWithValues = new List<(DeviceDynamicProperties type, string name, string unit, string value)?>
            {
                pairOrNull<ITemp>(DeviceDynamicProperties.Temperature ,"Temperature","°C"),
                pairOrNull<IVramTemp>(DeviceDynamicProperties.VramTemp,"VRAM Temperature","°C"),
                pairOrNull<ILoad>(DeviceDynamicProperties.Load,"Load","%"),
                pairOrNull<IMemControllerLoad>(DeviceDynamicProperties.MemoryControllerLoad, "MemCtrl Load","%"),
                pairOrNull<IGetFanSpeedPercentage>(DeviceDynamicProperties.FanSpeedPercentage, "Fan speed","%"),
                pairOrNull<IPowerUsage>(DeviceDynamicProperties.PowerUsage, "Power usage","W"),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "Miner", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "OC profile", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "OC profile ID", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "Fan profile", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "Fan profile ID", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "ELP profile", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "ELP profile ID", null),
            };
            var deviceOptionalDynamic = dynamicPropertiesWithValues
                .Where(p => p.HasValue)
                .Where(p => p.Value.value != null)
                .Select(p => p.Value)
                .ToList();

            if (isStateChange)
            {
                bool shouldRemoveDynamicVal((DeviceDynamicProperties type, string name, string unit, string value) dynamicVal)
                {
                    if (dynamicVal.unit == String.Empty) return false;
                    var ok = Int32.TryParse(dynamicVal.value, out var res);
                    if (ok && res < 0) return true;
                    return false;
                };
                deviceOptionalDynamic.RemoveAll(dynamVal => shouldRemoveDynamicVal(dynamVal));
                //deviceOptionalDynamic.ForEach(dynamVal => d.SupportedDynamicProperties.Add(dynamVal.type));
                if (isLogin) deviceOptionalDynamic.ForEach(dynamVal => d.SupportedDynamicProperties.Add(dynamVal.type));
            }
            foreach (DeviceDynamicProperties i in Enum.GetValues(typeof(DeviceDynamicProperties)))
            {
                if (!d.SupportedDynamicProperties.Contains(i)) deviceOptionalDynamic.RemoveAll(prop => prop.type == i);
            }
            List<(string name, string? unit)> optionalDynamicProperties = deviceOptionalDynamic.Select(p => (p.name, p.unit)).ToList();
            var values_odv = new JArray(deviceOptionalDynamic.Select(p => p.value));
            return (optionalDynamicProperties, values_odv);
        }

        // we cache device properties so we persevere  property IDs
        private static readonly Dictionary<ComputeDevice, List<OptionalMutableProperty>> _cachedDevicesOptionalMutable = new Dictionary<ComputeDevice, List<OptionalMutableProperty>>();
        private static (List<OptionalMutableProperty> properties, JArray values) GetDeviceOptionalMutable(ComputeDevice d, bool isStateChange, bool isLogin)
        {
            OptionalMutableProperty valueOrNull<T>(OptionalMutableProperty v) => d.DeviceMonitor is T ? v : null;
            List<OptionalMutableProperty> getOptionalMutableProperties(ComputeDevice d)
            {
                var optionalProperties = new List<OptionalMutableProperty>();
                // TODO sort by type
                optionalProperties.Add(new OptionalMutablePropertyString
                {
                    PropertyID = OptionalMutableProperty.NextPropertyId(),
                    DisplayGroup = 0,
                    DisplayName = "Miners settings",
                    DefaultValue = "",
                    Range = (2048, ""),
                    ExecuteTask = async (object p) =>
                    {
                        if (p is not string prop) return null;
                        var newState = JsonConvert.DeserializeObject<MinerAlgoState>(prop);
                        return d.ApplyNewAlgoStates(newState);
                    },
                    GetValue = () =>
                    {
                        string ret = null;
                        if (isStateChange)
                        {
                            ret = string.Empty;
                            ret += GetMinersForDeviceDynamic(d);
                        }
                        return ret;
                    }
                });
                if (isLogin) optionalProperties.ForEach(i => ActionMutableMap.MutableList.Add(i));
                return optionalProperties
                    .Where(p => p != null)
                    .ToList();
            }

            List<OptionalMutableProperty> getOptionalMutablePropertiesCached(ComputeDevice d)
            {
                if (_cachedDevicesOptionalMutable.TryGetValue(d, out var cachedProps)) return cachedProps;
                return getOptionalMutableProperties(d);
            }

            var props = getOptionalMutablePropertiesCached(d);
            var selectedValues = props
                .Where(p => p.GetValue() != null)?
                .Select(p => p.GetValue());
            JArray values_omv = null;
            if (selectedValues.Any())
            {
                values_omv = new JArray(selectedValues);
            }
            return (props, values_omv);
        }



        private static LoginMessage _loginMessage = null;
        public static List<List<string>> DeviceOptionalDynamicToList(List<(string name, string? unit)> properties)
        {
            List<List<string>> result = new List<List<string>>();
            foreach (var property in properties)
            {
                if (property.unit == null)
                {
                    result.Add(new List<string> { property.name });
                    continue;
                }
                result.Add(new List<string> { property.name, property.unit });
            }
            return result;
        }
        public static LoginMessage CreateLoginMessage(string btc, string worker, string rigID, IOrderedEnumerable<ComputeDevice> devices)
        {
            var sorted = SortedDevices(devices);
            if (_loginMessage != null) return _loginMessage;
            Device mapComputeDevice(ComputeDevice d)
            {
                return new Device
                {
                    StaticProperties = new Dictionary<string, object>
                    {
                        { "device_id", d.B64Uuid },
                        { "class", $"{NHMDeviceTypeToNHMWSDeviceType(d.DeviceType)}" },
                        { "name", d.Name },
                        { "optional", GetStaticPropertiesOptionalValues(d) },
                    },
                    Actions = CreateDefaultDeviceActions(d.B64Uuid),
                    OptionalDynamicProperties = DeviceOptionalDynamicToList(GetDeviceOptionalDynamic(d, true, true).properties),
                    OptionalMutableProperties = GetDeviceOptionalMutable(d, true, true).properties,
                };
            }

            _loginMessage = new LoginMessage
            {
                Btc = btc,
                Worker = worker,
                RigID = rigID,
                Version = new List<string> { $"NHM/{NHMApplication.ProductVersion}", Environment.OSVersion.ToString() },
                OptionalMutableProperties = GetRigOptionalMutableValuesLogin(btc, worker),
                OptionalDynamicProperties = GetRigOptionalDynamicValuesLogin(),
                Actions = CreateDefaultRigActions(),
                Devices = devices.Select(mapComputeDevice).ToList(),
                MinerState = GetMinerStateValues(worker, devices, true),
            };
            return _loginMessage;
        }
        private static List<OptionalMutableProperty> GetRigOptionalMutableValuesLogin(string btc, string worker)
        {
            return new List<OptionalMutableProperty>
                {
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "User name",
                        DefaultValue = btc,
                        Range = (64, String.Empty),
                    },
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Worker name",
                        DefaultValue = worker,
                        Range = (64, String.Empty),
                    },
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Miners settings",
                        DefaultValue = "",
                        Range = (65536, String.Empty),
                    },
                    //new OptionalMutablePropertyString
                    //{
                    //    PropertyID = OptionalMutableProperty.NextPropertyId(),
                    //    DisplayGroup = 0,
                    //    DisplayName = "Scheduler settings",
                    //    DefaultValue = "",
                    //    Range = (4096, String.Empty)
                    //}
                };
        }
        private static List<List<string>> GetRigOptionalDynamicValuesLogin()
        {
            return new List<List<string>>
                {
                    new List<string>
                    {
                        "Uptime",
                        "s"
                    },
                    new List<string>
                    {
                        "IP address"
                    },
                    new List<string>
                    {
                        "Profiles bundle id"
                    },
                    new List<string>
                    {
                        "Profiles bundle name"
                    }
                };
        }

        private static JObject GetMinerStateValues(string workerName, IOrderedEnumerable<ComputeDevice> devices, bool isLogin)
        {
            var json = JObject.FromObject(GetMinerState(workerName, devices, isLogin));
            var delProp = json.Property("method");
            delProp.Remove();
            return json;
        }
        private static List<string> GetRigOptionalDynamicValues()
        {
            var list = new List<string>
            {
                Helpers.GetElapsedSecondsSinceStart().ToString(),
                Helpers.GetLocalIP().ToString(),
                BundleManager.GetBundleInfo().BundleID,
                BundleManager.GetBundleInfo().BundleName,
            };
            return list;
        }
        private static List<string> GetRigOptionalMutableValues()
        {
            var list = new List<string>
            {
                CredentialsSettings.Instance.BitcoinAddress,
                CredentialsSettings.Instance.WorkerName,
                "",//TODO rig-wise algo settings
                //"",//TODO scheduler
            };
            return list;
        }

        internal static MinerState GetMinerState(string workerName, IOrderedEnumerable<ComputeDevice> devices, bool isStateChange = false)
        {
            var rig = ApplicationStateManager.CalcRigStatus();

            int rigStateToInt(RigStatus s) => s switch
            {
                RigStatus.Stopped => 1, // READY/IDLE/STOPPED
                RigStatus.Mining => 2, // MINING/WORKING
                RigStatus.Benchmarking => 3, // BENCHMARKING
                RigStatus.Error => 5, // ERROR
                RigStatus.Pending => 0, // NOT DEFINED
                RigStatus.Disabled => 4, // DISABLED
                _ => 0, // UNKNOWN
            };


            MinerState.DeviceState toDeviceState(ComputeDevice d)
            {
                int deviceStateToInt(DeviceState s) => s switch
                {
                    DeviceState.Stopped => 1, // READY/IDLE/STOPPED
                    DeviceState.Mining => 2, // MINING/WORKING
                    DeviceState.Benchmarking => 3, // BENCHMARKING
                    DeviceState.Error => 5, // ERROR
                    DeviceState.Pending => 0, // NOT DEFINED
                    DeviceState.Disabled => 4, // DISABLED
#if NHMWS4
                    DeviceState.Gaming => 6, //GAMING
                    DeviceState.Testing => 7, //TESTING
#endif
                    _ => 0, // UNKNOWN
                };

                JArray mdv(ComputeDevice d)
                {
                    var state = deviceStateToInt(d.State);
                    var speeds = MiningDataStats.GetSpeedForDevice(d.Uuid);
                    return new JArray(state, new JArray(speeds.Select(kvp => new JArray((int)kvp.type, kvp.speed))));
                }
                JArray mmv(ComputeDevice d)
                {
                    return new JArray(deviceStateToInt(d.State));
                }


                return new MinerState.DeviceState
                {
                    MandatoryDynamicValues = mdv(d),
                    OptionalDynamicValues = GetDeviceOptionalDynamic(d, isStateChange).values, // odv
                    MandatoryMutableValues = mmv(d),
                    OptionalMutableValues = GetDeviceOptionalMutable(d, isStateChange, false).values, // omv
                };
            }

            return new MinerState
            {
                MutableDynamicValues = new JArray(rigStateToInt(rig)),
                OptionalDynamicValues = new JArray(GetRigOptionalDynamicValues()),
                MandatoryMutableValues = new JArray(rigStateToInt(rig), workerName),
                OptionalMutableValues = new JArray(GetRigOptionalMutableValues()),
                Devices = devices.Select(toDeviceState).ToList(),
            };
        }


        private static List<NhmwsAction> CreateDefaultDeviceActions(string uuid)
        {
            return new List<NhmwsAction>
            {
                NhmwsAction.ActionDeviceEnable(uuid),
                NhmwsAction.ActionDeviceDisable(uuid),
                NhmwsAction.ActionOcProfileTest(uuid),
                NhmwsAction.ActionOcProfileTestStop(uuid),
                NhmwsAction.ActionFanProfileTest(uuid),
                NhmwsAction.ActionFanProfileTestStop(uuid),
                NhmwsAction.ActionElpProfileTest(uuid),
                NhmwsAction.ActionElpProfileTestStop(uuid),
            };
        }
        private static List<NhmwsAction> CreateDefaultRigActions()
        {
            return new List<NhmwsAction>
            {
                NhmwsAction.ActionStartMining(),
                NhmwsAction.ActionStopMining(),
                NhmwsAction.ActionRebenchmark(),
                NhmwsAction.ActionProfilesBundleSet(),
                NhmwsAction.ActionProfilesBundleReset(),
            };
        }
        private static List<JArray> GetStaticPropertiesOptionalValues(ComputeDevice d)
        {
            return d.BaseDevice switch
            {
                IGpuDevice gpu => new List<JArray>
                    {
                        new JArray("bus_id", $"{gpu.PCIeBusID}"),
                        new JArray("vram", $"{gpu.GpuRam}"),
                        new JArray("miners", FormatForOptionalValues("miners", GetMinersForDeviceStatic(d))),
                        new JArray("limits", GetLimitsForDevice(d)),
                    },
                _ => new List<JArray>
                    {
                        new JArray("miners", FormatForOptionalValues("miners", GetMinersForDeviceStatic(d))),
                        new JArray("limits", GetLimitsForDevice(d)),
                    },
            };
        }

        private static string FormatForOptionalValues(string name, string content)
        {
            return "{\"" + name + "\":" + content + "}";
        }

        private static string GetMinersForDeviceDynamic(ComputeDevice d)//todo  if include enabled return array of strings else return array of structs
        {
            var minersObject = new MinerAlgoState();
            var containers = d.AlgorithmSettings;
            if (containers == null) return String.Empty;
            var grouped = containers.GroupBy(c => c.PluginName).ToList();
            if (grouped == null) return String.Empty;
            foreach (var group in grouped)
            {
                var containerEnabled = group.Any(c => c.Enabled);
                var miner = new MinerDynamic() { Id = group.Key, Enabled = containerEnabled };
                var algos = new List<Algo>();
                foreach (var algo in group)
                {
                    var tempAlgo = new Algo() { Id = algo.AlgorithmName, Enabled = algo.Enabled };
                    algos.Add(tempAlgo);
                }
                miner.Algos = algos;
                minersObject.Miners.Add(miner);
            }
            var json = JsonConvert.SerializeObject(minersObject);
            return json;
        }
        private static string GetMinersForDeviceStatic(ComputeDevice d)
        {
            List<MinerStatic> miners = new List<MinerStatic>();
            var uniquePlugins = d.AlgorithmSettings?.Select(item => item.PluginName)?.Distinct()?.Where(item => !string.IsNullOrEmpty(item));
            if (uniquePlugins == null) return String.Empty;
            foreach (var plugin in uniquePlugins)
            {
                var uniqueAlgos = d.AlgorithmSettings?.Where(item => item.PluginName == plugin)?.Select(item => item.AlgorithmName)?.Distinct();
                if (uniqueAlgos == null) uniqueAlgos = new List<string>();
                miners.Add(new MinerStatic() { Id = plugin, AlgoList = uniqueAlgos.ToList() });
            }
            var json = JsonConvert.SerializeObject(miners);
            return json;
        }
        private static string GetLimitsForDevice(ComputeDevice d)
        {
            ComplexLimit limit = new ComplexLimit();
            if (d.DeviceMonitor is ITDP && d.DeviceMonitor is ITDPLimits tdpLim && d.CanSetPowerMode)
            {
                var lims = tdpLim.GetTDPLimits();
                if (lims.ok)
                {
                    limit.limits.Add(new Limit { Name = "Power mode", Unit = "W", Def = (int)lims.def, Range = ((int)lims.min, (int)lims.max) });
                }
            }
            if (d.DeviceMonitor is ICoreClockSet && d.DeviceMonitor is ICoreClockRange ccLim)
            {
                var lims = ccLim.CoreClockRange;
                if (lims.ok)
                {
                    limit.limits.Add(new Limit { Name = "Core clock", Unit = "MHz", Def = (int)lims.def, Range = ((int)lims.min, (int)lims.max) });
                }
            }
            if (d.DeviceMonitor is IMemoryClockSet && d.DeviceMonitor is IMemoryClockRange mcLim)
            {
                var lims = mcLim.MemoryClockRange;
                if (lims.ok)
                {
                    limit.limits.Add(new Limit { Name = "Memory clock", Unit = "MHz", Def = (int)lims.def, Range = ((int)lims.min, (int)lims.max) });
                }
            }
            var deviceType = d.DeviceType switch
            {
                DeviceType.CPU => 1,
                DeviceType.NVIDIA => 2,
                DeviceType.AMD => 2,
                DeviceType.INTEL => 2,
                _ => 0
            };
            var deviceVendor = d.Vendor switch
            {
                DeviceType.INTEL => 1,
                DeviceType.AMD => 2,
                DeviceType.NVIDIA => 3,
                _ => 0,
            };
            limit.Vendor = deviceVendor;
            limit.Type = deviceType;
            var json = JsonConvert.SerializeObject(limit);
            return json;
        }
    }
}

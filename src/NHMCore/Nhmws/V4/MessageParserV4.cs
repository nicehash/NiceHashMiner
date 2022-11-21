using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.DeviceMonitoring;
using NHM.DeviceMonitoring.Core_clock;
using NHM.DeviceMonitoring.Memory_clock;
using NHM.DeviceMonitoring.TDP;
using NHMCore.Configs;
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

        internal static IOrderedEnumerable<ComputeDevice> SortedDevices(this IEnumerable<ComputeDevice> devices)
        {
            return devices.OrderBy(d => d.DeviceType)
                .ThenBy(d => d.BaseDevice is IGpuDevice gpu ? gpu.PCIeBusID : int.MinValue);
        }

        private static string GetDevicePlugin(string UUID)
        {
            var data = MiningDataStats.GetDevicesMiningStats();
            var devData = data.FirstOrDefault(dev => dev.DeviceUUID == UUID);
            if (devData == null) return "";

            return devData.MinerName;
        }

        private static (List<(string name, string? unit)> properties, JArray values) GetDeviceOptionalDynamic(ComputeDevice d, bool isLogin = false)
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
                "OC profile ID" => $"",
                "Fan profile" => $"", //TODO
                "Fan profile ID" => $"",
                "ELP profile" => $"", //TODO
                "ELP profile ID" => $"",
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
                pairOrNull<ITemp>(DeviceDynamicProperties.Temperature ,"Temperature","C"),
                pairOrNull<IVramTemp>(DeviceDynamicProperties.VramTemp,"VRAM Temperature","C"),
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

            if (isLogin)
            {
                bool shouldRemoveDynamicVal((DeviceDynamicProperties type, string name, string unit, string value) dynamicVal)
                {
                    if (dynamicVal.unit == String.Empty) return false;
                    var ok = Int32.TryParse(dynamicVal.value, out var res);
                    if (ok && res < 0) return true;
                    return false;
                };
                deviceOptionalDynamic.RemoveAll(dynamVal => shouldRemoveDynamicVal(dynamVal));
                deviceOptionalDynamic.ForEach(dynamVal => d.SupportedDynamicProperties.Add(dynamVal.type));
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
        private static (List<OptionalMutableProperty> properties, JArray values) GetDeviceOptionalMutable(ComputeDevice d, bool isLogin)
        {
            OptionalMutableProperty valueOrNull<T>(OptionalMutableProperty v) => d.DeviceMonitor is T ? v : null;
            List<OptionalMutableProperty> getOptionalMutableProperties(ComputeDevice d)
            {
                if (isLogin)
                {
                    // TODO sort by type
                    var optionalProperties = new List<OptionalMutableProperty>();
                    optionalProperties.Add(new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Miners settings",
                        DefaultValue = "",
                        Range = (2048, ""),
                        //ExecuteTask = async (object p) =>
                        //{
                        //    //todo
                        //    return null;
                        //},
                        GetValue = () =>
                        {
                            //todo?
                            return string.Empty;
                        }
                    });
                    #region OMVMaybe
                    //if (d.DeviceMonitor is ITDP tdp && d.DeviceMonitor is ITDPLimits tdpLim)
                    //{
                    //    var limits = tdpLim.GetTDPLimits();
                    //    if (limits.ok)
                    //    {
                    //        optionalProperties.Add(valueOrNull<ITDP>(new OptionalMutablePropertyInt
                    //        {
                    //            PropertyID = OptionalMutableProperty.NextPropertyId(),
                    //            DisplayName = "Power mode",
                    //            DisplayUnit = "%",
                    //            DefaultValue = (int)limits.def,
                    //            Range = ((int)limits.min, (int)limits.max),
                    //            //ExecuteTask = async (object p) =>
                    //            //{
                    //            //    // #1 validate JSON input
                    //            //    if (p is string pstr && pstr is not null) return Task.FromResult<object>(null);
                    //            //    // TODO do something
                    //            //    return Task.FromResult<object>(null);
                    //            //},
                    //            GetValue = () =>
                    //            {
                    //                return tdp.TDPPercentage;
                    //            }
                    //        }));
                    //    }

                    //}
                    //if (d.DeviceMonitor is ICoreClockSet && d.DeviceMonitor is ICoreClockRange rangeCore)
                    //{
                    //    var ret = rangeCore.CoreClockRange;
                    //    if (ret.ok)
                    //    {
                    //        optionalProperties.Add(valueOrNull<ICoreClockSet>(new OptionalMutablePropertyInt
                    //        {
                    //            PropertyID = OptionalMutableProperty.NextPropertyId(),
                    //            DisplayName = "Core clock",
                    //            DisplayUnit = "MHz",
                    //            DefaultValue = ret.def,
                    //            Range = (ret.min, ret.max),
                    //            //ExecuteTask = async (object p) =>
                    //            //{
                    //            //todo
                    //            //}
                    //            GetValue = () =>
                    //            {
                    //                return d.CoreClock;
                    //            }
                    //        }));
                    //    }
                    //}
                    //if (d.DeviceMonitor is IMemoryClockSet && d.DeviceMonitor is IMemoryClockRange rangeMem)
                    //{
                    //    var ret = rangeMem.MemoryClockRange;
                    //    if (ret.ok)
                    //    {
                    //        optionalProperties.Add(valueOrNull<ICoreClockSet>(new OptionalMutablePropertyInt
                    //        {
                    //            PropertyID = OptionalMutableProperty.NextPropertyId(),
                    //            DisplayName = "Memory clock",
                    //            DisplayUnit = "MHz",
                    //            DefaultValue = ret.def,
                    //            Range = (ret.min, ret.max),
                    //            //ExecuteTask = async (object p) =>
                    //            //{
                    //            //todo
                    //            //}
                    //            GetValue = () =>
                    //            {
                    //                return d.MemoryClock;
                    //            }
                    //        }));
                    //    }
                    //}
                    #endregion
                    return optionalProperties
                        .Where(p => p != null)
                        .ToList();
                }
                return null;
            }

            List<OptionalMutableProperty> getOptionalMutablePropertiesCached(ComputeDevice d)
            {
                if (_cachedDevicesOptionalMutable.TryGetValue(d, out var cachedProps)) return cachedProps;
                return getOptionalMutableProperties(d);
            }

            var props = getOptionalMutablePropertiesCached(d);
            var values_omv = new JArray(props.Select(p => p.GetValue()));

            return (props, values_omv);
        }

        private static LoginMessage _loginMessage = null;
        public static List<List<string>> DeviceOptionalDynamicToList(List<(string name, string? unit)> properties)
        {
            List<List<string>> result = new List<List<string>>();
            foreach (var property in properties)
            {
                if(property.unit == null)
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
                        { "class", $"{(int)d.DeviceType}" },
                        { "name", d.Name },
                        { "optional", GetStaticPropertiesOptionalValues(d) },
                    },
                    Actions = CreateDefaultDeviceActions(d.B64Uuid),
                    OptionalDynamicProperties = DeviceOptionalDynamicToList(GetDeviceOptionalDynamic(d, true).properties),
                    OptionalMutableProperties = GetDeviceOptionalMutable(d, true).properties,
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
                    //new OptionalMutablePropertyString
                    //{
                    //    PropertyID = OptionalMutableProperty.NextPropertyId(),
                    //    DisplayGroup = 0,
                    //    DisplayName = "Worker name",
                    //    DefaultValue = worker,
                    //    Range = (64, String.Empty),
                    //},
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
                String.Empty, //todo
                String.Empty
            };
            return list;
        }
        private static List<string> GetRigOptionalMutableValues()
        {
            var list = new List<string>
            {
                CredentialsSettings.Instance.BitcoinAddress,
                //CredentialsSettings.Instance.WorkerName
            };
            return list;
        }

        internal static MinerState GetMinerState(string workerName, IOrderedEnumerable<ComputeDevice> devices, bool isLogin = false)
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
                    OptionalDynamicValues = GetDeviceOptionalDynamic(d, isLogin).values, // odv
                    MandatoryMutableValues = mmv(d),
                    OptionalMutableValues = GetDeviceOptionalMutable(d, isLogin).values, // omv
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
                        new JArray("miners", FormatForOptionalValues("miners", GetMinersForDevice(d, false))),
                        new JArray("limits", FormatForOptionalValues("limits", GetLimitsForDevice(d))),
                    },
                _ => new List<JArray> 
                    {
                        new JArray("miners", FormatForOptionalValues("miners", GetMinersForDevice(d, false))),
                        new JArray("limits", FormatForOptionalValues("limits", GetLimitsForDevice(d))),
                    },
            };
        }

        private static string FormatForOptionalValues(string name, string content)
        {
            return "{\""+ name +"\":" + content + "}";
        }

        private static string GetMinersForDeviceDynamic(ComputeDevice d, bool includeEnabled)//todo  if include enabled return array of strings else return array of structs
        {
            List<Miner> miners = new List<Miner>();
            var containers = d.AlgorithmSettings;
            if (containers == null) return String.Empty;
            var grouped =  containers.GroupBy(c => c.PluginName);
            if(grouped == null) return String.Empty;
            foreach (var group in grouped)
            {
                var container = group.First();
                var miner = new Miner() { Id = group.Key };
                if (includeEnabled) miner.Enabled = container.Enabled;
                var algos = new List<Algo>();
                foreach (var algo in group)
                {
                    var tempAlgo = new Algo() { Id = algo.AlgorithmName };
                    if (includeEnabled) tempAlgo.Enabled = algo.Enabled;
                    algos.Add(tempAlgo);
                }
                miner.Algos = algos;
                miners.Add(miner);
            }
            var json = JsonConvert.SerializeObject(miners);
            return json;
        }
        private static string GetMinersForDeviceStatic(ComputeDevice d)
        {
            List<Miner> miners = new List<Miner>();
            var uniquePlugins = d.AlgorithmSettings?.Select(item => item.PluginName)?.Distinct()?.Where(item => !string.IsNullOrEmpty(item));
            if (uniquePlugins == null) return String.Empty;
            foreach (var plugin in uniquePlugins)
            {
                var uniqueAlgos = d.AlgorithmSettings?.Where(item => item.PluginName == plugin)?.Select(item => item.AlgorithmName)?.Distinct();
                if (uniqueAlgos == null) uniqueAlgos = new List<string>();
                miners.Add(new Miner() { Id = plugin, AlgoList = uniqueAlgos.ToList() });
            }
            var json = JsonConvert.SerializeObject(miners);
            return json;
        }
        private static string GetLimitsForDevice(ComputeDevice d)
        {
            List<Limit> limits = new List<Limit>();
            if (d.DeviceMonitor is ITDP && d.DeviceMonitor is ITDPLimits tdpLim && d.CanSetPowerMode)
            {
                var lims = tdpLim.GetTDPLimits();
                if (lims.ok)
                {
                    limits.Add(new Limit { Name = "Power mode", Unit = "%", Def = (int)lims.def, Range = ((int)lims.min, (int)lims.max) });
                }
            }
            if (d.DeviceMonitor is ICoreClockSet && d.DeviceMonitor is ICoreClockRange ccLim)
            {
                var lims = ccLim.CoreClockRange;
                if (lims.ok)
                {
                    limits.Add(new Limit { Name = "Core clock", Unit = "MHz", Def = (int)lims.def, Range = ((int)lims.min, (int)lims.max) });
                }
            }
            if (d.DeviceMonitor is IMemoryClockSet && d.DeviceMonitor is IMemoryClockRange mcLim)
            {
                var lims = mcLim.MemoryClockRange;
                if (lims.ok)
                { 
                    limits.Add(new Limit { Name = "Memory clock", Unit = "MHz", Def = (int)lims.def, Range = ((int)lims.min, (int)lims.max) });
                }
            }
            var json = JsonConvert.SerializeObject(limits);
            return json;
        }
    }
}

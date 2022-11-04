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
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

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

        private static (List<(string name, string unit)> properties, JArray values) GetDeviceOptionalDynamic(ComputeDevice d, bool isLogin)
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
                "OC profile" => $"", //TODO
                "Fan profile" => $"", //TODO
                "ELP profile" => $"", //TODO
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
                pairOrNull<IGetFanSpeedPercentage>(DeviceDynamicProperties.FanSpeedPercentage, "Fan","%"),
                pairOrNull<IPowerUsage>(DeviceDynamicProperties.PowerUsage, "Power","W"),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "Miner", ""),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "OC profile", ""),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "Fan profile", ""),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "ELP profile", "")
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
            var optionalDynamicProperties = deviceOptionalDynamic.Select(p => (p.name, p.unit)).ToList();
            var values_odv = new JArray(deviceOptionalDynamic.Select(p => p.value));
            return (optionalDynamicProperties, values_odv);
        }

        // we cache device properties so we persevere  property IDs
        private static readonly Dictionary<ComputeDevice, List<OptionalMutableProperty>> _cachedDevicesOptionalMutable = new Dictionary<ComputeDevice, List<OptionalMutableProperty>>();
        private static (List<OptionalMutableProperty> properties, JArray values) GetDeviceOptionalMutable(ComputeDevice d)
        {
            OptionalMutableProperty valueOrNull<T>(OptionalMutableProperty v) => d.DeviceMonitor is T ? v : null;
            List<OptionalMutableProperty> getOptionalMutableProperties(ComputeDevice d)
            {
                // TODO sort by type
                var optionalProperties = new List<OptionalMutableProperty>();
                optionalProperties.Add(new OptionalMutablePropertyString
                {
                    PropertyID = OptionalMutableProperty.NextPropertyId(),
                    DisplayGroup = 0,
                    DisplayName = "Miners settings",
                    DefaultValue = "",
                    Range = (2048, "")
                });
                if (d.DeviceMonitor is ITDP tdp)
                {
                    optionalProperties.Add(valueOrNull<ITDP>(new OptionalMutablePropertyEnum //TODO is always included?
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(), // TODO this will eat up the ID
                        DisplayName = "TDP Simple",
                        DefaultValue = "Medium",
                        Range = new List<string> { "Low", "Medium", "High" },
                        // TODO action/setter to execute
                        ExecuteTask = async (object p) =>
                        {
                            // #1 validate JSON input
                            if (p is string pstr && pstr is not null) return Task.FromResult<object>(null);
                            // TODO do something
                            return Task.FromResult<object>(null);
                        },
                        GetValue = () =>
                        {
                            var ret = d.TDPSimple switch
                            {
                                TDPSimpleType.LOW => "Low",
                                TDPSimpleType.MEDIUM => "Medium",
                                TDPSimpleType.HIGH => "High",
                                _ => "ERROR",
                            };
                            return ret;
                        }
                    }));
                }
                if (d.DeviceMonitor is ICoreClockSet && d.DeviceMonitor is ICoreClockRange rangeCore)
                {
                    var ret = rangeCore.CoreClockRange;
                    if (ret.ok)
                    {
                        optionalProperties.Add(valueOrNull<ICoreClockSet>(new OptionalMutablePropertyInt
                        {
                            PropertyID = OptionalMutableProperty.NextPropertyId(),
                            DisplayName = "Core clock",
                            DefaultValue = ret.def,
                            Range = (ret.min, ret.max)
                        }));
                    }
                }
                if (d.DeviceMonitor is IMemoryClockSet && d.DeviceMonitor is IMemoryClockRange rangeMem)
                {
                    var ret = rangeMem.MemoryClockRange;
                    if (ret.ok)
                    {
                        optionalProperties.Add(valueOrNull<ICoreClockSet>(new OptionalMutablePropertyInt
                        {
                            PropertyID = OptionalMutableProperty.NextPropertyId(),
                            DisplayName = "Memory clock",
                            DefaultValue = ret.def,
                            Range = (ret.min, ret.max)
                        }));
                    }
                }
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
            var values_omv = new JArray(props.Select(p => p.GetValue()));

            return (props, values_omv);
        }

        private static LoginMessage _loginMessage = null;
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
                    Actions = CreateDefaultDeviceActions(),
                    OptionalDynamicProperties = GetDeviceOptionalDynamic(d, true).properties,
                    OptionalMutableProperties = GetDeviceOptionalMutable(d).properties,
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
                MinerState = GetMinerStateValues(worker, devices),
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

        private static JObject GetMinerStateValues(string workerName, IOrderedEnumerable<ComputeDevice> devices)
        {
            var json = JObject.FromObject(GetMinerState(workerName, devices));
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
                CredentialsSettings.Instance.WorkerName
            };
            return list;
        }

        internal static MinerState GetMinerState(string workerName, IOrderedEnumerable<ComputeDevice> devices)
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
                    OptionalDynamicValues = GetDeviceOptionalDynamic(d, false).values, // odv
                    MandatoryMutableValues = mmv(d),
                    OptionalMutableValues = GetDeviceOptionalMutable(d).values, // omv
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
        private static List<NhmwsAction> CreateDefaultDeviceActions()
        {
            return new List<NhmwsAction>
            {
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "Device enable",
                    DisplayGroup = 0,
                },
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "Device disable",
                    DisplayGroup = 0,
                },
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "OC profile test",
                    DisplayGroup = 1,
                    Parameters = new List<Parameter>()
                    {
                        new ParameterString()
                        {
                            DisplayName = "OC profile",
                            DefaultValue = "",
                            Range = (1024, "")
                        }
                    }
                },
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "Fan profile test",
                    DisplayGroup = 1,
                    Parameters = new List<Parameter>()
                    {
                        new ParameterString()
                        {
                            DisplayName = "Fan profile",
                            DefaultValue = "",
                            Range = (1024, "")
                        }
                    }
                },
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "ELP profile test",
                    DisplayGroup = 1,
                    Parameters = new List<Parameter>()
                    {
                        new ParameterString()
                        {
                            DisplayName = "ELP profile",
                            DefaultValue = "",
                            Range = (1024, "")
                        }
                    }
                }
            };
        } 
        private static List<NhmwsAction> CreateDefaultRigActions()
        {
            return new List<NhmwsAction>
            {
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "Mining start",
                    DisplayGroup = 1,
                },
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "Mining stop",
                    DisplayGroup = 1,
                },
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "Profiles bundle set",
                    DisplayGroup = 1,
                    Parameters = new List<Parameter>()
                    {
                        new ParameterString()
                        {
                            DisplayName = "Bundle profiles",
                            DefaultValue = "",
                            Range = (4096, "")
                        }
                    }
                },
                new NhmwsAction
                {
                    ActionID = NhmwsAction.NextActionId(),
                    DisplayName = "Profiles bundle reset",
                    DisplayGroup = 1,
                },
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
                            new JArray("miners", FormatForOptionalValues("miners", GetMinersForDevice(d))),//todo make function
                            //new JArray("limits", $"{GetLimitsForDevice(d)}"),
                        },
                _ => new List<JArray> { },
            };
        }

        private static string FormatForOptionalValues(string name, string content)
        {
            return "{\""+ name +"\":" + content + "}";
        }

        private static string GetMinersForDevice(ComputeDevice d)
        {
            List<Miner> miners = new List<Miner>();
            var uniquePlugins = d.AlgorithmSettings?.Select(item => item.PluginName)?.Distinct()?.Where(item => !string.IsNullOrEmpty(item));
            if (uniquePlugins == null) return String.Empty;
            foreach(var plugin in uniquePlugins)
            {
                var uniqueAlgos = d.AlgorithmSettings?.Where(item => item.PluginName == plugin)?.Select(item => item.AlgorithmName)?.Distinct();
                if(uniqueAlgos == null) uniqueAlgos = new List<string>();
                miners.Add(new Miner() { Id = plugin, Algos = uniqueAlgos.ToList() });
            }
            var json = JsonConvert.SerializeObject(miners);
            return json;
        }
        //private static string GetLimitsForDevice(ComputeDevice d)
        //{
        //    List<Limit> limits = new List<Limit>();
        //    if(d.DeviceMonitor is ITDP)
        //    {
        //        //todo need monitor here
        //        //if(d.DeviceMonitor.)
        //        //limits.Add(new Limit() { Name = "Power mode", Unit = "%" });
        //    }
        //    if(d.DeviceMonitor is ICoreClockSet)
        //    {

        //    }
        //    if(d.DeviceMonitor is IMemoryClockSet)
        //    {

        //    }
        //}
    }
}

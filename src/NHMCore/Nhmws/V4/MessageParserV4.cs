using Newtonsoft.Json;
using NHM.Common.Device;
using NHM.DeviceMonitoring;
using NHM.DeviceMonitoring.TDP;
using NHMCore.Mining;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
                // non supported
                _ => throw new Exception($"Unable to deserialize '{jsonData}' got method '{method}'."),
            };
        }



        private static LoginMessage _loginMessage = null;
        public static LoginMessage CreateLoginMessage(string btc, string worker, string rigID, IEnumerable<ComputeDevice> devices)
        {
            if (_loginMessage != null)
            {
                _loginMessage.Btc = btc;
                _loginMessage.Worker = worker;
                return _loginMessage;
            }

            List<NhnwsAction> createDefaultActions() =>
                new List<NhnwsAction>
                {
                    new NhnwsAction
                    {
                        ActionID = NhnwsAction.NextActionId(),
                        DisplayName = "Mining start",
                        DisplayGroup = 1,
                    },
                    new NhnwsAction
                    {
                        ActionID = NhnwsAction.NextActionId(),
                        DisplayName = "Mining stop",
                        DisplayGroup = 1,
                    },
                    new NhnwsAction
                    {
                        ActionID = NhnwsAction.NextActionId(),
                        DisplayName = "Mining enable",
                        DisplayGroup = 1,
                    },
                    new NhnwsAction
                    {
                        ActionID = NhnwsAction.NextActionId(),
                        DisplayName = "Mining disable",
                        DisplayGroup = 1,
                    },
                };

            Device mapComputeDevice(ComputeDevice d)
            {
                Dictionary<string, object> getStaticPropertiesOptionalValues(ComputeDevice d)
                {
                    return d.BaseDevice switch
                    {
                        IGpuDevice gpu => new Dictionary<string, object>
                        {
                            { "bus_id", $"{gpu.PCIeBusID}" },
                            { "vram", $"{gpu.GpuRam}" },
                        },
                        _ => new Dictionary<string, object> { },
                    };
                }
                
                List<(string name, string unit)> getOptionalDynamicProperties(ComputeDevice d)
                {
                    // TODO sort by type
                    (string name, string unit)? pairOrNull<T>(string name, string unit) => d.DeviceMonitor is T ? (name, unit) : null;
                    var dynamicProperties = new List<(string name, string unit)?>
                    {
                        pairOrNull<ITemp>("Temperature","C"),
                        pairOrNull<IPowerUsage>("Power Usage","W"),
                        pairOrNull<ILoad>("Load","%"),
                        pairOrNull<IGetFanSpeedPercentage>("Fan speed percentage","%"),
                    };
                    return dynamicProperties
                        .Where(p => p.HasValue)
                        .Select(p => p.Value)
                        .ToList();
                }
                List<OptionalMutableProperty> getOptionalMutableProperties(ComputeDevice d)
                {
                    // TODO sort by type
                    OptionalMutableProperty valueOrNull<T>(OptionalMutableProperty v) => d.DeviceMonitor is T ? v : null;
                    var optionalProperties = new List<OptionalMutableProperty>
                    {
                        valueOrNull<ITDP>(new OptionalMutablePropertyEnum
                        {
                            PropertyID = OptionalMutableProperty.NextPropertyId(), // TODO this will eat up the ID
                            DisplayName = "TDP Simple",
                            DefaultValue = "Medium",
                            Range = new List<string>{ "Low", "Medium", "High" },
                            // TODO action/setter to execute
                            ExecuteTask = async (object p) =>
                            {
                                // #1 validate JSON input
                                if (p is string pstr && pstr is not null) return Task.FromResult<object>(null);
                                // TODO do something
                                return Task.FromResult<object>(null);
                            },
                        }),
                    };
                    return optionalProperties
                        .Where(p => p != null)
                        .ToList();
                }
                return new Device
                {
                    StaticProperties = new Dictionary<string, object>
                    {
                        { "device_id", d.B64Uuid },
                        { "class", $"{(int)d.DeviceType}" },
                        { "name", d.Name },
                        { "optional", getStaticPropertiesOptionalValues(d) },
                    },
                    Actions = createDefaultActions(),
                    OptionalDynamicProperties = getOptionalDynamicProperties(d),
                    OptionalMutableProperties = getOptionalMutableProperties(d),
                };
            } 

            _loginMessage = new LoginMessage
            {
                Btc = btc,
                Worker = worker,
                RigID = rigID,
                Version = new List<string> { $"NHM/{Assembly.GetEntryAssembly().GetName().Version.ToString()}", "NA/NA" },
                OptionalMutableProperties = new List<OptionalMutableProperty>
                {
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "User name",
                        DefaultValue = btc,
                        Range = (64, null),
                    },
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Worker name",
                        DefaultValue = worker,
                        Range = (64, null),
                    },
                },
                Actions = createDefaultActions(),
                Devices = devices.Select(mapComputeDevice).ToList(),
            };
            return _loginMessage;
        }

    }
}

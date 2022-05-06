using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Mining;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NHMCore.Nhmws.V4
{
    static class MessageParser
    {
        private static string ParseMessageData(string jsonData)
        {
            try
            {
                dynamic message = JsonConvert.DeserializeObject(jsonData);
                return message.method.Value as string;
            }
            catch {}
            return null;
        }

        internal static IMethod ParseMessage(string jsonData)
        {
            var method = ParseMessageData(jsonData);
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

        public static double? ParseBalanceMessage(this BalanceMessage msg)
        {
            return double.TryParse(msg.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var btcBalance) ? btcBalance : null;
        }

        public static (Dictionary<AlgorithmType, double> payingDict, IEnumerable<AlgorithmType> stables) ParseSmaMessageData(this SmaMessage msg)
        {
            try
            {
                // TODO split this as only one might fail??
                var stables = JsonConvert.DeserializeObject<int[]>(msg.Stable).Select(id => (AlgorithmType)id);
                var payingPairs = msg.Data
                    .Where(pair => pair.Count >= 2)
                    .Select(pair => (id: pair[0] as int?, paying: pair[1] as double?))
                    .Where(pair => pair.id.HasValue && pair.paying.HasValue)
                    .Select(pair => (id: (AlgorithmType)pair.id.Value, paying: pair.paying.Value))
                    .ToArray();
                var payingDict = new Dictionary<AlgorithmType, double>();
                foreach (var (id, paying) in payingPairs) payingDict[id] = paying;
                return (payingDict, stables);
            }
            catch (Exception e)
            {
                Logger.Error("MessageParser", $"ParseSmaMessageData - error: {e.Message}");
            }
            return (null, null);
        }

        public static (double usdBtcRate, Dictionary<string, double> exchangesFiat) ParseExchangeRatesMessageData(this ExchangeRatesMessage er)
        {
            double? getBTC_To_USD_exchangeRate(Dictionary<string, string> exchangePair)
            {
                var isCoinBTC = exchangePair.TryGetValue("coin", out var coin) && coin == "BTC";
                double? usdExchangeRate = exchangePair.TryGetValue("USD", out var usd) && double.TryParse(usd, NumberStyles.Float, CultureInfo.InvariantCulture, out var usdD) ? usdD : null;
                return usdExchangeRate;
            }
            try
            {
                var exchange = JsonConvert.DeserializeObject<ExchangeRatesMessage.ExchangeRatesData>(er.Data);
                if (exchange?.ExchangesFiat == null || exchange?.Exchanges == null)
                {
                    throw new Exception("Parsed empty data");
                }
                double usdBtcRate = exchange.Exchanges
                    .Select(getBTC_To_USD_exchangeRate)
                    .FirstOrDefault(rate => rate.HasValue)
                    ?? -1.0;
                return (usdBtcRate, exchange.ExchangesFiat);
            }
            catch (Exception e)
            {
                Logger.Error("MessageParser", $"ParseExchangeRatesMessageData - error: {e.Message}");
            }
            return (1, null);
        }

        public static (string btc, string worker, string group) ParseCredentialsMessage(ISetCredentialsMessage msg)
        {
            return msg switch
            {
                MiningSetUsername m => (m.Btc, null, null),
                MiningSetWorker m => (null, m.Worker, null),
                MiningSetGroup m => (null, null, m.Group),
                _ => (null, null, null),
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
                return new Device
                {
                    StaticProperties = new Dictionary<string, object>
                    {
                        { "device_id", d.B64Uuid },
                        { "class", $"{(int)d.DeviceType}" },
                        { "name", d.Name },
                        { "optional", new List<string>() },
                    },
                    Actions = createDefaultActions(),
                    OptionalDynamicProperties = new List<(string name, string unit)> { },
                    OptionalMutableProperties = new List<OptionalMutableProperty> { },
                };
            } 

            _loginMessage = new LoginMessage
            {
                Btc = btc,
                Worker = worker,
                RigID = rigID,
                Version = new List<string> { $"NHM/{Application.ProductVersion}", "NA/NA" },
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

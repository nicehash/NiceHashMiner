using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHMCore.Nhmws.V4;
using System.Collections.Generic;

namespace NHMCore.Nhmws
{
    internal interface IMethod
    {
        string Method { get; }
    }

    internal interface IReceiveMessage : IMethod { }

    // RPC message is named as Action
    internal interface IReceiveRpcMessage : IReceiveMessage
    {
        int Id { get; }
    }

    internal interface ISetCredentialsMessage : IReceiveRpcMessage { }

    internal interface ISetDeviceMessage : IReceiveRpcMessage
    {
        string Device { get; }
    }

    internal interface ISendMessage : IMethod { }

    internal class ObsoleteMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method { get; set; }
    }

    internal class UnknownMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method { get; set; }
    }

    internal class SmaMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method => "sma";
        [JsonProperty("data")]
        public List<JArray> Data { get; set; }
        [JsonProperty("stable")]
        public string Stable { get; set; }
    }

    internal class BalanceMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method => "balance";
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    internal class VersionsMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method => "versions";
        // ignore v2
        // ignore legacy
        [JsonProperty("v3")]
        public string V3 { get; set; }
    }

    internal class BurnMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method => "burn";
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    internal class ExchangeRatesMessage : IReceiveMessage
    {
        public class ExchangeRatesData
        {
            [JsonProperty("exchanges")]
            public List<Dictionary<string, string>> Exchanges { get; set; }
            [JsonProperty("exchanges_fiat")]
            public Dictionary<string, double> ExchangesFiat { get; set; }
        }

        [JsonProperty("method")]
        public string Method => "exchange_rates";
        [JsonProperty("data")]
        public string Data { get; set; }
    }

    // RPC

    internal class MiningSetUsername : ISetCredentialsMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.set.username";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("username")]
        public string Btc { get; set; }
    }

    internal class MiningSetWorker : ISetCredentialsMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.set.worker";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("worker")]
        public string Worker { get; set; }
    }

    internal class MiningSetGroup : ISetCredentialsMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.set.group";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("group")]
        public string Group { get; set; }
    }

    internal class MiningEnable : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.enable";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
    }

    internal class MiningDisable : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.disable";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
    }

    internal class MiningStart : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.start";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
    }

    internal class MiningStop : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.stop";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
    }

    internal class MiningSetPowerMode : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.set.power_mode";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
        [JsonProperty("power_mode")]
        public int PowerMode { get; set; }
    }

    internal class MinerReset : IReceiveRpcMessage
    {
        [JsonProperty("method")]
        public string Method => "miner.reset";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("level")]
        public string Level { get; set; }
    }

    internal class MinerCallAction : IReceiveRpcMessage //todo in progress
    {
        [JsonProperty("method")]
        public string Method => "miner.call.action";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonProperty("action_id")]
        public int ActionId { get; set; }
        [JsonProperty("parameters")]
        public List<string> Parameters { get; set; }
    }

    internal class MinerSetMutable : IReceiveRpcMessage //todo in progress
    {
        [JsonProperty("method")]
        public string Method => "miner.set.mutable";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("properties")]
        List<Property> Properties { get; set; }
        [JsonProperty("devices")]
        List<Device> Devices { get; set; }

    }

    // RPC response
    internal class ExecutedCall : ISendMessage
    {
        [JsonProperty("method")]
        public string Method => "executed";
        [JsonProperty("params")]
        public List<object> Params = new List<object>();

        public ExecutedCall(int messageID, int errorCode, string message = null)
        {
            Params.Add(messageID);
            Params.Add(errorCode);
            if (message != null) Params.Add(message);
        }
    }
}

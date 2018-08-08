using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMiner.Switching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Stats.Models;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;
using WebSocketSharp;

namespace NiceHashMiner.Stats
{
    public class SocketEventArgs : EventArgs
    {
        public readonly string Message;

        public SocketEventArgs(string message)
        {
            Message = message;
        }
    }

    public static class NiceHashStats
    {
        #region JSON Models
#pragma warning disable 649, IDE1006
        private class NicehashCredentials
        {
            public string method = "credentials.set";
            public string btc;
            public string worker;
        }

        private class NicehashDeviceStatus
        {
            public string method = "miner.status";
            [JsonProperty("params")]
            public List<JToken> param;
        }
        public class ExchangeRateJson
        {
            public List<Dictionary<string, string>> exchanges { get; set; }
            public Dictionary<string, double> exchanges_fiat { get; set; }
        }
#pragma warning restore 649, IDE1006
        #endregion

        private const int DeviceUpdateLaunchDelay = 20 * 1000;
        private const int DeviceUpdateInterval = 60 * 1000;
        
        public static double Balance { get; private set; }
        public static string Version { get; private set; }
        public static string VersionLink { get; private set; }
        public static bool IsAlive => _socket?.IsAlive ?? false;

        // Event handlers for socket
        public static event EventHandler OnBalanceUpdate;

        public static event EventHandler OnSmaUpdate;
        public static event EventHandler OnVersionUpdate;
        public static event EventHandler OnConnectionLost;
        public static event EventHandler OnConnectionEstablished;
        public static event EventHandler<SocketEventArgs> OnVersionBurn;
        public static event EventHandler OnExchangeUpdate;
        public static event EventHandler<DeviceUpdateEventArgs> OnDeviceUpdate;

        private static NiceHashSocket _socket;
        
        private static System.Threading.Timer _deviceUpdateTimer;

        private static IGlobalRatesUpdate _mainForm;
        private static IRatesComunication _ratesComunication;

        public static void StartConnection(string address, IGlobalRatesUpdate mainForm, IRatesComunication ratesComunication)
        {
            _mainForm = mainForm;
            _ratesComunication = ratesComunication;

            if (_socket == null)
            {
                _socket = new NiceHashSocket(address);
                _socket.OnConnectionEstablished += SocketOnOnConnectionEstablished;
                _socket.OnDataReceived += SocketOnOnDataReceived;
                _socket.OnConnectionLost += SocketOnOnConnectionLost;
            }
            _socket.StartConnection(ConfigManager.GeneralConfig.BitcoinAddress, ConfigManager.GeneralConfig.WorkerName, ConfigManager.GeneralConfig.RigGroup);
            _deviceUpdateTimer = new System.Threading.Timer(MinerStatus_Tick, null, DeviceUpdateInterval, DeviceUpdateInterval);
        }

        #region Socket Callbacks

        private static void SocketOnOnConnectionLost(object sender, EventArgs eventArgs)
        {
            OnConnectionLost?.Invoke(sender, eventArgs);
        }

        private static void SocketOnOnDataReceived(object sender, MessageEventArgs e)
        {
            ExecutedInfo info = null;
            var executed = false;
            var id = -1;
            try
            {
                if (e.IsText)
                {
                    info = ProcessData(e.Data, out executed, out id);
                }

                if (executed)
                {
                    SendExecuted(info, id);
                }
            }
            catch (RpcException rEr)
            {
                Helpers.ConsolePrint("SOCKET", rEr.ToString());
                if (!executed) return;
                Helpers.ConsolePrint("SOCKET", $"Sending executed response with code {rEr.Code}");
                SendExecuted(info, id, rEr.Code, rEr.Message);
            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("SOCKET", er.ToString());
            }
        }

        internal static ExecutedInfo ProcessData(string data, out bool executed, out int id)
        {
            Helpers.ConsolePrint("SOCKET", "Received: " + data);
            dynamic message = JsonConvert.DeserializeObject(data);
            executed = false;

            id = (int?) message?.id?.Value ?? -1;
            switch (message.method.Value)
            {
                case "sma":
                {
                    // Try in case stable is not sent, we still get updated paying rates
                    try
                    {
                        var stable = JsonConvert.DeserializeObject(message.stable.Value);
                        SetStableAlgorithms(stable);
                    }
                    catch
                    { }
                    SetAlgorithmRates(message.data);
                    return null;
                }

                case "balance":
                    SetBalance(message.value.Value);
                    return null;
                case "burn":
                    OnVersionBurn?.Invoke(null, new SocketEventArgs(message.message.Value));
                    return null;
                case "exchange_rates":
                    SetExchangeRates(message.data.Value);
                    return null;
                case "essentials":
                    var ess = JsonConvert.DeserializeObject<EssentialsCall>(data);
                    ProcessEssentials(ess);

                    return null;
                case "mining.set.username":
                    executed = true;
                    var user = (string) message.username;

                    if (!BitcoinAddress.ValidateBitcoinAddress(user))
                        throw new RpcException("Bitcoin address invalid", 1);

                    ConfigManager.GeneralConfig.BitcoinAddress = user;
                    return new ExecutedInfo {NewBtc = user};
                case "mining.set.worker":
                    executed = true;
                    var worker = (string) message.worker;

                    if (!BitcoinAddress.ValidateWorkerName(worker))
                        throw new RpcException("Worker name invalid", 1);

                    ConfigManager.GeneralConfig.WorkerName = worker;
                    return new ExecutedInfo {NewWorker = worker};
                case "mining.set.group":
                    executed = true;
                    var group = (string) message.group;
                    ConfigManager.GeneralConfig.RigGroup = group;

                    return new ExecutedInfo {NewRig = group};
                case "mining.enable":
                    executed = true;
                    SetDevicesEnabled((string) message.device, true);
                    return null;
                case "mining.disable":
                    executed = true;
                    SetDevicesEnabled((string) message.device, false);
                    return null;
                case "mining.start":
                    executed = true;
                    // TODO
                    StartMining((string) message.device);
                    return null;
                case "mining.stop":
                    executed = true;
                    // TODO
                    StopMining((string) message.device);
                    return null;
            }
            
            throw new RpcException("Operation not supported", 2);
        }

        private static void SocketOnOnConnectionEstablished(object sender, EventArgs e)
        {
            MinerStatus_Tick(null); // Send device to populate rig stats

            OnConnectionEstablished?.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region Incoming socket calls

        private static void ProcessEssentials(EssentialsCall ess)
        {
            if (ess?.Params?.First()[2] is string ver && ess.Params.First()[3] is string link)
            {
                SetVersion(ver, link);
            }

            if (ess?.Params?[2] != null)
            {
                foreach (var map in ess.Params[2])
                {
                    // Hacky way temporary

                    if (!(map is JArray m && m.Count > 1)) continue;
                    var name = m.Last().Value<string>();
                    var i = m.First().Value<int>();

                    var filterName = name.AfterFirstOccurence("GTX ");
                    if (string.IsNullOrWhiteSpace(filterName))
                        filterName = name.AfterFirstOccurence("NVIDIA ");
                    if (string.IsNullOrWhiteSpace(filterName))
                        continue;

                    foreach (var dev in ComputeDeviceManager.Available.Devices)
                    {
                        if (dev.Name.Contains(filterName))
                            dev.TypeID = i;
                    }
                }
            }
        }

        private static void SetAlgorithmRates(JArray data)
        {
            try
            {
                var payingDict = new Dictionary<AlgorithmType, double>();
                if (data != null)
                {
                    foreach (var algo in data)
                    {
                        var algoKey = (AlgorithmType) algo[0].Value<int>();
                        payingDict[algoKey] = algo[1].Value<double>();
                    }
                }

                NHSmaData.UpdateSmaPaying(payingDict);
                
                OnSmaUpdate?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetStableAlgorithms(JArray stable)
        {
            var stables = stable.Select(algo => (AlgorithmType) algo.Value<int>());
            NHSmaData.UpdateStableAlgorithms(stables);
        }

        private static void SetBalance(string balance)
        {
            try
            {
                if (double.TryParse(balance, NumberStyles.Float, CultureInfo.InvariantCulture, out var bal))
                {
                    Balance = bal;
                    OnBalanceUpdate?.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetVersion(string version, string link)
        {
            Version = version;
            VersionLink = link;
            OnVersionUpdate?.Invoke(null, EventArgs.Empty);
        }

        private static void SetExchangeRates(string data)
        {
            try
            {
                var exchange = JsonConvert.DeserializeObject<ExchangeRateJson>(data);
                if (exchange?.exchanges_fiat == null || exchange.exchanges == null) return;
                foreach (var exchangePair in exchange.exchanges)
                {
                    if (!exchangePair.TryGetValue("coin", out var coin) || coin != "BTC" ||
                        !exchangePair.TryGetValue("USD", out var usd) || 
                        !double.TryParse(usd, NumberStyles.Float, CultureInfo.InvariantCulture, out var usdD))
                        continue;

                    ExchangeRateApi.UsdBtcRate = usdD;
                    break;
                }

                ExchangeRateApi.UpdateExchangesFiat(exchange.exchanges_fiat);

                OnExchangeUpdate?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetDevicesEnabled(string devs, bool enabled)
        {
            var found = false;
            if (!ComputeDeviceManager.Available.Devices.Any())
                throw new RpcException("No devices to set", 1);

            foreach (var dev in ComputeDeviceManager.Available.Devices)
            {
                if (devs != "*" && dev.B64Uuid != devs) continue;
                found = true;
                dev.Enabled = enabled;
            }

            if (!found)
                throw new RpcException("Device not found", 1);

            OnDeviceUpdate?.Invoke(null, new DeviceUpdateEventArgs(ComputeDeviceManager.Available.Devices));
        }

        private static void StartMining(string devs)
        {
            if (devs != "*")
            {
                SetDevicesEnabled(devs, true);
                MinersManager.UpdateUsedDevices(ComputeDeviceManager.Available.Devices);
            }
            else
            {
                if (MinersManager.IsMiningEnabled())
                    throw new RpcException("Mining already enabled", 40);
            }

            if (!ConfigManager.GeneralConfig.HasValidUserWorker())
                throw new RpcException("No valid worker and/or address set", 41);

            var loc = Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation];

            if (!MinersManager.StartInitialize(_ratesComunication, loc, ConfigManager.GeneralConfig.WorkerName,
                ConfigManager.GeneralConfig.BitcoinAddress))
                throw new RpcException("Mining could not start", 42);

            _mainForm?.StartMiningGui();
        }

        private static void StopMining(string devs)
        {
            if (devs != "*")
            {
                SetDevicesEnabled(devs, false);
                MinersManager.UpdateUsedDevices(ComputeDeviceManager.Available.Devices);
            }

            if (!MinersManager.IsMiningEnabled())
                throw new RpcException("Mining already stopped", 50);

            MinersManager.StopAllMiners();
            _mainForm?.StopMiningGui();
        }

        #endregion

        #region Outgoing socket calls

        public static void SetCredentials(string btc, string worker)
        {
            if (BitcoinAddress.ValidateBitcoinAddress(btc) && BitcoinAddress.ValidateWorkerName(worker))
            {
                // Send as task since SetCredentials is called from UI threads
                Task.Factory.StartNew(() =>
                {
                    MinerStatus_Tick(null);
                    _socket?.StartConnection(btc, worker);
                });
            }
        }

        private static void MinerStatus_Tick(object state)
        {
            var devices = ComputeDeviceManager.Available.Devices;
            var paramList = new List<JToken>
            {
                "STOPPED"  // TODO
            };
            var activeIDs = MinersManager.GetActiveMinersIndexes();
            var benchIDs = new List<int>();  // TODO

            var deviceList = new JArray();

            foreach (var device in devices)
            {
                try
                {
                    var array = new JArray
                    {
                        device.TypeID,
                        device.B64Uuid  // TODO
                    };

                    // Status (dev type and mining/benching/disabled
                    var status = ((int) device.DeviceType + 1) << 2;

                    if (activeIDs.Contains(device.Index))
                        status += 2;
                    else if (benchIDs.Contains(device.Index))
                        status += 3;
                    else if (device.Enabled)
                        status += 1;

                    array.Add(status);

                    array.Add((int)Math.Round(device.Load));

                    // TODO algo speeds
                    array.Add(new JArray());

                    // Hardware monitoring
                    array.Add((int) Math.Round(device.Temp));
                    array.Add(device.FanSpeed);
                    array.Add((int) Math.Round(device.PowerUsage));

                    // Power/intensity mode
                    array.Add(0);
                    array.Add(0);

                    deviceList.Add(array);
                }
                catch (Exception e) { Helpers.ConsolePrint("SOCKET", e.ToString()); }
            }

            paramList.Add(deviceList);

            var data = new NicehashDeviceStatus
            {
                param = paramList
            };
            var sendData = JsonConvert.SerializeObject(data);

            // This function is run every minute and sends data every run which has two auxiliary effects
            // Keeps connection alive and attempts reconnection if internet was dropped
            _socket?.SendData(sendData);
        }

        private static void SendExecuted(ExecutedInfo info, int id, int code = 0, string message = null)
        {
            // First set status
            MinerStatus_Tick(null);
            // Then executed
            var data = new ExecutedCall(id, code, message).Serialize();
            _socket?.SendData(data);
            // Login if we have to
            if (info?.LoginNeeded ?? false)
            {
                _socket?.StartConnection(info.NewBtc, info.NewWorker, info.NewRig);
            }
        }

        #endregion

        public static string GetNiceHashApiData(string url, string worker)
        {
            var responseFromServer = "";
            try
            {
                var activeMinersGroup = MinersManager.GetActiveMinersGroup();

                var wr = (HttpWebRequest) WebRequest.Create(url);
                wr.UserAgent = "NiceHashMiner/" + Application.ProductVersion;
                if (worker.Length > 64) worker = worker.Substring(0, 64);
                wr.Headers.Add("NiceHash-Worker-ID", worker);
                wr.Headers.Add("NHM-Active-Miners-Group", activeMinersGroup);
                wr.Timeout = 30 * 1000;
                var response = wr.GetResponse();
                var ss = response.GetResponseStream();
                if (ss != null)
                {
                    ss.ReadTimeout = 20 * 1000;
                    var reader = new StreamReader(ss);
                    responseFromServer = reader.ReadToEnd();
                    if (responseFromServer.Length == 0 || responseFromServer[0] != '{')
                        throw new Exception("Not JSON!");
                    reader.Close();
                }
                response.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", ex.Message);
                return null;
            }

            return responseFromServer;
        }
    }
}

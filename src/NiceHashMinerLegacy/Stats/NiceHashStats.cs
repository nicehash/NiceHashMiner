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
using NiceHashMiner.Benchmarking;
using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Stats.Models;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;
using WebSocketSharp;
using NiceHashMiner.Configs;

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

        //public static double Balance { get; private set; }
        public static string Version { get; private set; }
        public static string VersionLink { get; private set; }
        public static bool IsAlive => _socket?.IsAlive ?? false;

        // Event handlers for socket
        public static event EventHandler OnSmaUpdate;
        public static event EventHandler OnConnectionLost;
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
            int? id = null;
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

        internal static ExecutedInfo ProcessData(string data, out bool executed, out int? id)
        {
            Helpers.ConsolePrint("SOCKET", "Received: " + data);
            dynamic message = JsonConvert.DeserializeObject(data);
            executed = false;

            if (message == null)
                throw new RpcException("No message found", 34);

            id = (int?) message.id;
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
                    var btc = (string)message.username;
                    var userSetResult = ApplicationStateManager.SetBTCIfValidOrDifferent(btc, true);
                    switch (userSetResult)
                    {
                        case ApplicationStateManager.SetResult.INVALID:
                            throw new RpcException("Bitcoin address invalid", -4);
                        case ApplicationStateManager.SetResult.CHANGED:
                            // we return executed
                            break;
                        case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                            throw new RpcException($"Nothing to change btc \"{btc}\" already set", -5);
                    }
                    return new ExecutedInfo { NewBtc = btc };
                case "mining.set.worker":
                    executed = true;
                    var worker = (string)message.worker;
                    var workerSetResult = ApplicationStateManager.SetWorkerIfValidOrDifferent(worker, true);
                    switch (workerSetResult)
                    {
                        case ApplicationStateManager.SetResult.INVALID:
                            throw new RpcException("Worker name invalid", -5);
                        case ApplicationStateManager.SetResult.CHANGED:
                            // we return executed
                            break;
                        case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                            throw new RpcException($"Nothing to change worker name \"{worker}\" already set", -5);
                    }
                    return new ExecutedInfo { NewWorker = worker };
                case "mining.set.group":
                    executed = true;
                    var group = (string) message.group;
                    var groupSetResult = ApplicationStateManager.SetGroupIfValidOrDifferent(group, true);
                    switch (groupSetResult)
                    {
                        case ApplicationStateManager.SetResult.INVALID:
                            // TODO error code not correct
                            throw new RpcException("Group name invalid", -1000);
                        case ApplicationStateManager.SetResult.CHANGED:
                            // we return executed
                            break;
                        case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                            throw new RpcException($"Nothing to change group \"{group}\" already set", -5);
                    }
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
                    StartMining((string) message.device);
                    return null;
                case "mining.stop":
                    executed = true;
                    StopMining((string) message.device);
                    return null;
                case "mining.set.power_mode":
                    executed = true;
                    SetPowerMode((string) message.device, (PowerLevel) message.power_mode);
                    return null;
            }
            
            throw new RpcException("Operation not supported", 2);
        }

        private static void SocketOnOnConnectionEstablished(object sender, EventArgs e)
        {
            // Send device to populate rig stats, and send device names
            SendMinerStatus(true);
        }

        #endregion

        #region Incoming socket calls

        private static void ProcessEssentials(EssentialsCall ess)
        {
            if (ess?.Versions?.Count > 1 && ess.Versions[1].Count == 2)
            {
                SetVersion(ess.Versions[1][0], ess.Versions[1][1]);
            }

            if (ess?.Devices != null)
            {
                foreach (var map in ess.Devices)
                {
                    // Hacky way temporary

                    if (!(map is JArray m && m.Count > 1)) continue;
                    var name = m.Last().Value<string>();
                    var i = m.First().Value<int>();

                    foreach (var dev in ComputeDeviceManager.Available.Devices)
                    {
                        if (dev.Name.Contains(name))
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
                    ApplicationStateManager.OnBalanceUpdate(bal);
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
            ApplicationStateManager.OnVersionUpdate(version);
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

        private static bool SetDevicesEnabled(string devs, bool enabled)
        {
            var found = false;
            if (!ComputeDeviceManager.Available.Devices.Any())
                throw new RpcException("No devices to set", 1);

            var anyStillRunning = false;

            foreach (var dev in ComputeDeviceManager.Available.Devices)
            {
                if (devs == "*" || dev.B64Uuid == devs)
                {
                    found = true;
                    dev.Enabled = enabled;
                }

                anyStillRunning = anyStillRunning || dev.Enabled;
            }

            if (!found)
                throw new RpcException("Device not found", 1);

            OnDeviceUpdate?.Invoke(null, new DeviceUpdateEventArgs(ComputeDeviceManager.Available.Devices));

            return anyStillRunning;
        }

        private static void StartMining(string devs)
        {
            if (BenchmarkManager.InBenchmark)
                throw new RpcException("In benchmark", 43);
            if (!ConfigManager.GeneralConfig.HasValidUserWorker())
                throw new RpcException("No valid worker and/or address set", 41);

            if (MinersManager.IsMiningEnabled())
            {
                if (devs == "*")
                    throw new RpcException("Mining already enabled", 40);

                SetDevicesEnabled(devs, true);
                MinersManager.UpdateUsedDevices(ComputeDeviceManager.Available.Devices);
            }
            else
            {
                if (devs != "*")
                {
                    // Only mine with the one selected
                    foreach (var dev in ComputeDeviceManager.Available.Devices)
                    {
                        dev.Enabled = false;
                    }
                    SetDevicesEnabled(devs, true);
                }
                // TODO this will all go out
                var loc = "eu";//Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation];

                if (!MinersManager.StartInitialize(_ratesComunication, loc, Globals.GetWorkerName(), Globals.GetBitcoinUser()))
                    throw new RpcException("Mining could not start", 42);

                _mainForm?.StartMiningGui();
            }
        }

        private static void StopMining(string devs)
        {
            if (!MinersManager.IsMiningEnabled())
                throw new RpcException("Mining already stopped", 50);

            if (devs != "*")
            {
                if (SetDevicesEnabled(devs, false))
                {
                    MinersManager.UpdateUsedDevices(ComputeDeviceManager.Available.Devices);
                }
                else
                {
                    // No devices are left enabled, stop all mining
                    MinersManager.StopAllMiners(true);
                    _mainForm?.StopMiningGui();
                }
            }
            else
            {
                MinersManager.StopAllMiners(true);
                _mainForm?.StopMiningGui();
            }
        }

        private static void SetPowerMode(string device, PowerLevel level)
        {
            var devs = device == "*" ? 
                ComputeDeviceManager.Available.Devices : 
                ComputeDeviceManager.Available.Devices.Where(d => d.B64Uuid == device);

            var found = false;

            foreach (var dev in devs)
            {
                if (!(dev is CudaComputeDevice cuda)) continue;
                cuda.SetPowerTarget(level);
                found = true;
            }

            if (!found)
            {
                throw new RpcException("No devices settable devices found", 101);
            }
        }

        #endregion

        #region Outgoing socket calls

        public static void SetCredentials(string btc, string worker, string group)
        {
            if (BitcoinAddress.ValidateBitcoinAddress(btc) && BitcoinAddress.ValidateWorkerName(worker))
            {
                // Send as task since SetCredentials is called from UI threads
                Task.Factory.StartNew(() =>
                {
                    SendMinerStatus(false);
                    _socket?.StartConnection(btc, worker, group);
                });
            }
        }

        private static void SendMinerStatus(bool sendDeviceNames)
        {
            var devices = ComputeDeviceManager.Available.Devices;

            var stat = "STOPPED";
            if (BenchmarkManager.InBenchmark) stat = "BENCHMARKING";
            else if (MinersManager.IsMiningEnabled()) stat = "MINING";

            var paramList = new List<JToken>
            {
                stat
            };
            var activeIDs = MinersManager.GetActiveMinersIndexes();
            var benchIDs = BenchmarkManager.GetBenchmarkingDevices().ToList();

            var deviceList = new JArray();

            foreach (var device in devices)
            {
                try
                {
                    var array = new JArray
                    {
                        sendDeviceNames ? device.Name : "",
                        device.B64Uuid  // TODO
                    };

                    // Status (dev type and mining/benching/disabled
                    var status = ((int) device.DeviceType + 1) << 3;

                    if (activeIDs.Contains(device.Index))
                        status += 2;
                    else if (benchIDs.Contains(device.Index))
                        status += 3;
                    else //if (device.Enabled)
                        status += 1;

                    array.Add(status);

                    array.Add((int)Math.Round(device.Load));

                    // TODO algo speeds
                    array.Add(new JArray());

                    // Hardware monitoring
                    array.Add((int) Math.Round(device.Temp));
                    array.Add(device.FanSpeed);
                    array.Add((int) Math.Round(device.PowerUsage));

                    // Power mode
                    if (device is CudaComputeDevice cuda)
                    {
                        array.Add((int) cuda.PowerLevel);
                    }
                    else
                    {
                        array.Add(0);
                    }

                    // Intensity mode
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

        private static void MinerStatus_Tick(object state)
        {
            SendMinerStatus(false);
        }

        private static void SendExecuted(ExecutedInfo info, int? id, int code = 0, string message = null)
        {
            // First set status
            SendMinerStatus(false);
            // Then executed
            var data = new ExecutedCall(id ?? -1, code, message).Serialize();
            _socket?.SendData(data);
            // Login if we have to
            if (info?.LoginNeeded ?? false)
            {
                _socket?.StartConnection(info.NewBtc, info.NewWorker, info.NewRig);
            }
        }

        #endregion

        public static void StateChanged()
        {
            SendMinerStatus(false);
        }

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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Devices;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NiceHashMiner.Switching;
using WebSocketSharp;


namespace NiceHashMiner
{
    public class SocketEventArgs : EventArgs
    {
        public string Message;

        public SocketEventArgs(string message)
        {
            Message = message;
        }
    }

    internal class NiceHashStats
    {
#pragma warning disable 649, IDE1006

        #region JSON Models

        private class nicehash_login
        {
            public string method = "login";
            public string version;
            public int protocol = 1;
        }

        private class nicehash_credentials
        {
            public string method = "credentials.set";
            public string btc;
            public string worker;
        }

        private class nicehash_device_status
        {
            public string method = "devices.status";
            public List<JArray> devices;
        }

        #endregion

#pragma warning restore 649, IDE1006

        private const int DeviceUpdateLaunchDelay = 20 * 1000;
        private const int DeviceUpdateInterval = 60 * 1000;
        
        public static double Balance { get; private set; }
        public static string Version { get; private set; }
        public static bool IsAlive => NiceHashConnection.IsAlive;

        // Event handlers for socket
        public static event EventHandler OnBalanceUpdate = delegate { };

        public static event EventHandler OnSmaUpdate = delegate { };
        public static event EventHandler OnVersionUpdate = delegate { };
        public static event EventHandler OnConnectionLost = delegate { };
        public static event EventHandler OnConnectionEstablished = delegate { };
        public static event EventHandler<SocketEventArgs> OnVersionBurn = delegate { };

        private static readonly Random Random = new Random();

        private static System.Threading.Timer _deviceUpdateTimer;

        #region Socket

        private class NiceHashConnection
        {
            private static WebSocket _webSocket;
            public static bool IsAlive => _webSocket.ReadyState == WebSocketState.Open;
            private static bool _attemptingReconnect;
            private static bool _connectionAttempted;
            private static bool _connectionEstablished;

            public static void StartConnection(string address)
            {
                _connectionAttempted = true;
                try
                {
                    if (_webSocket == null)
                    {
                        _webSocket = new WebSocket(address, true);
                    }
                    else
                    {
                        _webSocket.Close();
                    }
                    _webSocket.OnOpen += ConnectCallback;
                    _webSocket.OnMessage += ReceiveCallback;
                    _webSocket.OnError += ErrorCallback;
                    _webSocket.OnClose += CloseCallback;
                    _webSocket.Log.Level = LogLevel.Debug;
                    _webSocket.Log.Output = (data, s) => Helpers.ConsolePrint("SOCKET", data.ToString());
                    _webSocket.EnableRedirection = true;
                    _webSocket.Connect();
                    _connectionEstablished = true;
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("SOCKET", e.ToString());
                }
            }

            private static void ConnectCallback(object sender, EventArgs e)
            {
                try
                {
                    NHSmaData.InitializeIfNeeded();
                    //send login
                    var version = "NHML/" + Application.ProductVersion;
                    var login = new nicehash_login
                    {
                        version = version
                    };
                    var loginJson = JsonConvert.SerializeObject(login);
                    SendData(loginJson);

                    DeviceStatus_Tick(null); // Send device to populate rig stats

                    OnConnectionEstablished.Emit(null, EventArgs.Empty);
                }
                catch (Exception er)
                {
                    Helpers.ConsolePrint("SOCKET", er.ToString());
                }
            }

            private static void ReceiveCallback(object sender, MessageEventArgs e)
            {
                try
                {
                    if (e.IsText)
                    {
                        Helpers.ConsolePrint("SOCKET", "Received: " + e.Data);
                        dynamic message = JsonConvert.DeserializeObject(e.Data);
                        if (message.method == "sma")
                        {
                            // Try in case stable is not sent, we still get updated paying rates
                            try
                            {
                                var stable = JsonConvert.DeserializeObject(message.stable.Value);
                                SetStableAlgorithms(stable);
                            }
                            catch
                            {
                                SetAlgorithmRates(message.data);
                            }
                        }
                        else if (message.method == "balance")
                        {
                            SetBalance(message.value.Value);
                        }
                        else if (message.method == "versions")
                        {
                            SetVersion(message.legacy.Value);
                        }
                        else if (message.method == "burn")
                        {
                            OnVersionBurn.Emit(null, new SocketEventArgs(message.message.Value));
                        }
                    }
                }
                catch (Exception er)
                {
                    Helpers.ConsolePrint("SOCKET", er.ToString());
                }
            }

            private static void ErrorCallback(object sender, WebSocketSharp.ErrorEventArgs e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }

            private static void CloseCallback(object sender, CloseEventArgs e)
            {
                Helpers.ConsolePrint("SOCKET", $"Connection closed code {e.Code}: {e.Reason}");
                AttemptReconnect();
            }

            // Don't call SendData on UI threads, since it will block the thread for a bit if a reconnect is needed
            public static bool SendData(string data, bool recurs = false)
            {
                try
                {
                    if (_webSocket != null && IsAlive)
                    {
                        // Make sure connection is open
                        // Verify valid JSON and method
                        dynamic dataJson = JsonConvert.DeserializeObject(data);
                        if (dataJson.method == "credentials.set" || dataJson.method == "devices.status" || dataJson.method == "login")
                        {
                            Helpers.ConsolePrint("SOCKET", "Sending data: " + data);
                            _webSocket.Send(data);
                            return true;
                        }
                    }
                    else if (_webSocket != null)
                    {
                        if (AttemptReconnect() && !recurs)
                        {
                            // Reconnect was successful, send data again (safety to prevent recursion overload)
                            SendData(data, true);
                        }
                        else
                        {
                            Helpers.ConsolePrint("SOCKET", "Socket connection unsuccessfull, will try again on next device update (1min)");
                        }
                    }
                    else
                    {
                        if (!_connectionAttempted)
                        {
                            Helpers.ConsolePrint("SOCKET", "Data sending attempted before socket initialization");
                        }
                        else
                        {
                            Helpers.ConsolePrint("SOCKET", "webSocket not created, retrying");
                            StartConnection(Links.NhmSocketAddress);
                        }
                    }
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("SOCKET", e.ToString());
                }
                return false;
            }

            private static bool AttemptReconnect()
            {
                if (_attemptingReconnect)
                {
                    return false;
                }
                if (IsAlive)
                {
                    // no reconnect needed
                    return true;
                }
                _attemptingReconnect = true;
                var sleep = _connectionEstablished ? 10 + Random.Next(0, 20) : 0;
                Helpers.ConsolePrint("SOCKET", "Attempting reconnect in " + sleep + " seconds");
                // More retries on first attempt
                var retries = _connectionEstablished ? 5 : 25;
                if (_connectionEstablished)
                {
                    // Don't wait if no connection yet
                    Thread.Sleep(sleep * 1000);
                }
                else
                {
                    // Don't not wait again
                    _connectionEstablished = true;
                }
                for (var i = 0; i < retries; i++)
                {
                    _webSocket.Connect();
                    Thread.Sleep(100);
                    if (IsAlive)
                    {
                        _attemptingReconnect = false;
                        return true;
                    }
                    Thread.Sleep(1000);
                }
                _attemptingReconnect = false;
                OnConnectionLost.Emit(null, EventArgs.Empty);
                return false;
            }
        }

        public static void StartConnection(string address)
        {
            NiceHashConnection.StartConnection(address);
            _deviceUpdateTimer = new System.Threading.Timer(DeviceStatus_Tick, null, DeviceUpdateInterval, DeviceUpdateInterval);
        }

        #endregion

        #region Incoming socket calls

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
                
                OnSmaUpdate.Emit(null, EventArgs.Empty);
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
                double.TryParse(balance, NumberStyles.Number, CultureInfo.InvariantCulture, out var bal);
                Balance = bal;
                OnBalanceUpdate.Emit(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetVersion(string version)
        {
            Version = version;
            OnVersionUpdate.Emit(null, EventArgs.Empty);
        }

        #endregion

        #region Outgoing socket calls

        public static void SetCredentials(string btc, string worker)
        {
            var data = new nicehash_credentials
            {
                btc = btc,
                worker = worker
            };
            if (BitcoinAddress.ValidateBitcoinAddress(data.btc) && BitcoinAddress.ValidateWorkerName(worker))
            {
                var sendData = JsonConvert.SerializeObject(data);

                // Send as task since SetCredentials is called from UI threads
                Task.Factory.StartNew(() => NiceHashConnection.SendData(sendData));
            }
        }

        public static void DeviceStatus_Tick(object state)
        {
            var devices = ComputeDeviceManager.Avaliable.AllAvaliableDevices;
            var deviceList = new List<JArray>();
            var activeIDs = MinersManager.GetActiveMinersIndexes();
            foreach (var device in devices)
            {
                try
                {
                    var array = new JArray
                    {
                        device.Index,
                        device.Name
                    };
                    var status = Convert.ToInt32(activeIDs.Contains(device.Index)) + ((int) device.DeviceType + 1) * 2;
                    array.Add(status);
                    array.Add((uint) device.Load);
                    array.Add((uint) device.Temp);
                    array.Add(device.FanSpeed);

                    deviceList.Add(array);
                }
                catch (Exception e) { Helpers.ConsolePrint("SOCKET", e.ToString()); }
            }
            var data = new nicehash_device_status
            {
                devices = deviceList
            };
            var sendData = JsonConvert.SerializeObject(data);
            // This function is run every minute and sends data every run which has two auxiliary effects
            // Keeps connection alive and attempts reconnection if internet was dropped
            NiceHashConnection.SendData(sendData);
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

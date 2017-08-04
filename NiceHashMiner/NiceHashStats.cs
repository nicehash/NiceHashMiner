using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners;
using NiceHashMiner.Devices;
using Newtonsoft.Json.Linq;
using WebSocketSharp;



namespace NiceHashMiner
{ 
    class NiceHashStats {
#pragma warning disable 649
        #region JSON Models

        class nicehash_login {
            public string method = "login";
            public string version;
            public int protocol = 1;
        }

        class nicehash_credentials {
            public string method = "credentials.set";
            public string btc;
            public string worker;
        }

        class nicehash_device_status
        {
            public string method = "devices.status";
            public List<JArray> devices;
        }

        #endregion
#pragma warning restore 649

        public static Dictionary<AlgorithmType, NiceHashSMA> AlgorithmRates { get; private set; }
        public static double Balance { get; private set; }
        public static string Version { get; private set; }
        public static bool IsAlive { get { return NiceHashConnection.IsAlive; } }
        // Event handlers for socket
        public static event EventHandler OnBalanceUpdate = delegate { };
        public static event EventHandler OnSMAUpdate = delegate { };
        public static event EventHandler OnVersionUpdate = delegate { };
        public static event EventHandler OnConnectionLost = delegate { };
        public static event EventHandler OnConnectionEstablished = delegate { };

        #region Socket
        private class NiceHashConnection
        {
            static WebSocket webSocket;
            public static bool IsAlive { get { return webSocket.IsAlive; } }
            static bool attemptingReconnect = false;

            public static void StartConnection(string address) {
                try {
                    if (webSocket == null) {
                        webSocket = new WebSocket(address);
                    } else {
                        webSocket.Close();
                    }
                    webSocket.OnOpen += ConnectCallback;
                    webSocket.OnMessage += ReceiveCallback;
                    webSocket.OnError += ErrorCallback;
                    webSocket.OnClose += CloseCallback;
                    webSocket.EmitOnPing = true;
                    webSocket.Log.Level = LogLevel.Debug;
                    webSocket.Connect();
                } catch (Exception e) {
                    Helpers.ConsolePrint("SOCKET", e.ToString());
                }
            }

            private static void ConnectCallback(object sender, EventArgs e) {
                try {
                    if (AlgorithmRates == null) {
                        // Populate SMA first (for port etc)
                        AlgorithmRates = BaseNiceHashSMA.BaseNiceHashSMADict;
                    }
                    //send login
                    var version = "NHML/" + Application.ProductVersion;
                    var login = new nicehash_login();
                    login.version = version;
                    var loginJson = JsonConvert.SerializeObject(login);
                    SendData(loginJson);

                    OnConnectionEstablished.Emit(null, EventArgs.Empty);
                } catch (Exception er) {
                    Helpers.ConsolePrint("SOCKET", er.ToString());
                }
            }

            private static void ReceiveCallback(object sender, MessageEventArgs e) {
                try {
                    if (e.IsText) {
                        Helpers.ConsolePrint("SOCKET", "Received: " + e.Data);
                        dynamic message = JsonConvert.DeserializeObject(e.Data);
                        if (message.method == "sma") {
                            SetAlgorithmRates(message.data);
                        }else if (message.method == "balance") {
                            SetBalance(message.value.Value);
                        } else if (message.method == "versions") {
                            SetVersion(message.legacy.Value);
                        }
                    }
                } catch (Exception er) {
                    Helpers.ConsolePrint("SOCKET", er.ToString());
                }
            }

            private static void ErrorCallback(object sender, WebSocketSharp.ErrorEventArgs e) {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }

            private static void CloseCallback(object sender, CloseEventArgs e) {
                Helpers.ConsolePrint("SOCKET", "Connection closed: " + e.Reason);
                AttemptReconnect();
            }

            // Don't call SendData on UI threads, since it will block the thread for a bit if a reconnect is needed
            public static bool SendData(string data, bool recurs = false) {
                try { 
                    if (webSocket != null && webSocket.IsAlive) {  // Make sure connection is open
                        // Verify valid JSON and method
                        dynamic dataJson = JsonConvert.DeserializeObject(data);
                        if (dataJson.method == "credentials.set" || dataJson.method == "devices.status" || dataJson.method == "login") {
                            Helpers.ConsolePrint("SOCKET", "Sending data: " + data);
                            webSocket.Send(data);
                            return true;
                        }
                    } else if (webSocket != null) {
                        if (AttemptReconnect() && !recurs) {  // Reconnect was successful, send data again (safety to prevent recursion overload)
                            SendData(data, true);
                        } else {
                            Helpers.ConsolePrint("SOCKET", "Socket connection unsuccessfull, will try again on next device update (1min)");
                        }
                    } else {
                        Helpers.ConsolePrint("SOCKET", "Data sending attempted before socket initialization");
                    }
                } catch (Exception e) {
                    Helpers.ConsolePrint("SOCKET", e.ToString());
                }
                return false;
            }

            private static bool AttemptReconnect() {
                if (attemptingReconnect) {
                    return false;
                }
                if (webSocket.IsAlive) {  // no reconnect needed
                    return true;
                }
                attemptingReconnect = true;
                Helpers.ConsolePrint("SOCKET", "Attempting reconnect");
                for (int i = 0; i < 5; i++) {
                    webSocket.Connect();
                    Thread.Sleep(100);
                    if (webSocket.IsAlive) {
                        attemptingReconnect = false;
                        return true;
                    }
                    Thread.Sleep(1000);
                }
                attemptingReconnect = false;
                OnConnectionLost.Emit(null, EventArgs.Empty);
                return false;
            }
        }

        public static void StartConnection(string address) {
            NiceHashConnection.StartConnection(address);
        }

        #endregion

        #region Incoming socket calls

        private static void SetAlgorithmRates(JArray data) {
            try {
                foreach (var algo in data) {
                    var algoKey = (AlgorithmType)algo[0].Value<int>();
                    if (AlgorithmRates.ContainsKey(algoKey)) {
                        AlgorithmRates[algoKey].paying = algo[1].Value<double>();
                    }
                }
                OnSMAUpdate.Emit(null, EventArgs.Empty);
            } catch (Exception e) {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetBalance(string balance) {
            try {
                double bal = 0d;
                double.TryParse(balance, out bal);
                Balance = bal;
                OnBalanceUpdate.Emit(null, EventArgs.Empty);
            } catch (Exception e) {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetVersion(string version) {
            Version = version;
            OnVersionUpdate.Emit(null, EventArgs.Empty);
        }

        #endregion

        #region Outgoing socket calls

        public static void SetCredentials(string btc, string worker) {
            var data = new nicehash_credentials();
            data.btc = btc;
            data.worker = worker;
            if (BitcoinAddress.ValidateBitcoinAddress(data.btc) && BitcoinAddress.ValidateWorkerName(worker)) {
                var sendData = JsonConvert.SerializeObject(data);

                // Send as task since SetCredentials is called from UI threads
                Task.Factory.StartNew(() => NiceHashConnection.SendData(sendData));
            }
        }

        public static void SetDeviceStatus(List<ComputeDevice> devices) {
            var deviceList = new List<JArray>();
            var activeIDs = MinersManager.GetActiveMinersIndexes();
            foreach (var device in devices) {
                try {
                    var array = new JArray();
                    array.Add(device.Index);
                    array.Add(device.Name);
                    int status = Convert.ToInt32(activeIDs.Contains(device.Index)) + (((int)device.DeviceType + 1) * 2);
                    array.Add(status);
                    array.Add((uint)device.Load);
                    array.Add((uint)device.Temp);
                    array.Add((uint)device.FanSpeed);

                    deviceList.Add(array);
                } catch (Exception e) { Helpers.ConsolePrint("SOCKET", e.ToString()); }
            }
            var data = new nicehash_device_status();
            data.devices = deviceList;
            var sendData = JsonConvert.SerializeObject(data);
            // This function is run every minute and sends data every run which has two auxiliary effects
            // Keeps connection alive and attempts reconnection if internet was dropped
            NiceHashConnection.SendData(sendData);
        }

        #endregion

        public static string GetNiceHashAPIData(string URL, string worker)
        {
            string ResponseFromServer;
            try
            {
                string ActiveMinersGroup = MinersManager.GetActiveMinersGroup();

                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(URL);
                WR.UserAgent = "NiceHashMiner/" + Application.ProductVersion;
                if (worker.Length > 64) worker = worker.Substring(0, 64);
                WR.Headers.Add("NiceHash-Worker-ID", worker);
                WR.Headers.Add("NHM-Active-Miners-Group", ActiveMinersGroup);
                WR.Timeout = 30 * 1000;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 20 * 1000;
                StreamReader Reader = new StreamReader(SS);
                ResponseFromServer = Reader.ReadToEnd();
                if (ResponseFromServer.Length == 0 || ResponseFromServer[0] != '{')
                    throw new Exception("Not JSON!");
                Reader.Close();
                Response.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", ex.Message);
                return null;
            }

            return ResponseFromServer;
        }
    }
}

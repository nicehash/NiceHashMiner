using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners;
using NiceHashMiner.Devices;
using Newtonsoft.Json.Linq;
using WebSocketSharp;



namespace NiceHashMiner
{ 
    public class SocketEventArgs : EventArgs
    {
        public string Message = "";

        public SocketEventArgs(string message) {
            Message = message;
        }
    }
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

        const int deviceUpdateLaunchDelay = 20 * 1000;
        const int deviceUpdateInterval = 5 * 60 * 1000;

        public static Dictionary<AlgorithmType, NiceHashSMA> AlgorithmRates { get; private set; }
        private static NiceHashData niceHashData;
        public static double Balance { get; private set; }
        public static string Version { get; private set; }
        public static bool IsAlive { get { return NiceHashConnection.IsAlive; } }
        // Event handlers for socket
        public static event EventHandler OnBalanceUpdate = delegate { };
        public static event EventHandler OnSMAUpdate = delegate { };
        public static event EventHandler OnVersionUpdate = delegate { };
        public static event EventHandler OnConnectionLost = delegate { };
        public static event EventHandler OnConnectionEstablished = delegate { };
        public static event EventHandler<SocketEventArgs> OnVersionBurn = delegate { };

        static readonly Random random = new Random();

        static System.Threading.Timer deviceUpdateTimer;
        static System.Threading.Timer algoRatesUpdateTimer;

        #region Socket
        private class NiceHashConnection
        {
            static WebSocket webSocket;
            public static bool IsAlive { get { return webSocket.IsAlive; } }
            static bool attemptingReconnect = false;
            static bool connectionAttempted = false;
            static bool connectionEstablished = false;

            public static void StartConnection(string address)
            {
                UpdateAlgoRates(null);
                algoRatesUpdateTimer = new System.Threading.Timer(UpdateAlgoRates, null, deviceUpdateInterval, deviceUpdateInterval);
            }

            private static void UpdateAlgoRates(object state)
            {
                try
                {
                    // We get the algo payment info here - http://www.zpool.ca/api/status
                    var WR = (HttpWebRequest) WebRequest.Create("http://www.zpool.ca/api/status");
                    var Response = WR.GetResponse();
                    var SS = Response.GetResponseStream();
                    SS.ReadTimeout = 20 * 1000;
                    var Reader = new StreamReader(SS);
                    var ResponseFromServer = Reader.ReadToEnd().Trim();
                    if (ResponseFromServer.Length == 0 || ResponseFromServer[0] != '{')
                        throw new Exception("Not JSON!");
                    Reader.Close();
                    Response.Close();

                    var zData = JsonConvert.DeserializeObject<Dictionary<string, zPoolAlgo>>(ResponseFromServer);
                    zSetAlgorithmRates(zData.Values.ToArray());
                }
                catch (Exception e)
                {
                    int x = 0;
                }
            }

            private static void ConnectCallback(object sender, EventArgs e) {
                try {
                    if (AlgorithmRates == null || niceHashData == null) {
                        niceHashData = new NiceHashData();
                        AlgorithmRates = niceHashData.NormalizedSMA();
                    }
                    //send login
                    var version = "NHML/" + Application.ProductVersion;
                    var login = new nicehash_login();
                    login.version = version;
                    var loginJson = JsonConvert.SerializeObject(login);
                    SendData(loginJson);

                    DeviceStatus_Tick(null);  // Send device to populate rig stats

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
                        } else if (message.method == "balance") {
                            SetBalance(message.value.Value);
                        } else if (message.method == "versions") {
                            SetVersion(message.legacy.Value);
                        } else if (message.method == "burn") {
                            OnVersionBurn.Emit(null, new SocketEventArgs(message.message.Value));
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
                Helpers.ConsolePrint("SOCKET", $"Connection closed code {e.Code}: {e.Reason}");
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
                        if (!connectionAttempted) {
                            Helpers.ConsolePrint("SOCKET", "Data sending attempted before socket initialization");
                        } else {
                            Helpers.ConsolePrint("SOCKET", "webSocket not created, retrying");
                            StartConnection(Links.NHM_Socket_Address);
                        }
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
                var sleep = connectionEstablished ? 10 + random.Next(0, 20) : 0;
                Helpers.ConsolePrint("SOCKET", "Attempting reconnect in " + sleep + " seconds");
                // More retries on first attempt
                var retries = connectionEstablished ? 5 : 25;
                if (connectionEstablished) {  // Don't wait if no connection yet
                    Thread.Sleep(sleep * 1000);
                } else {
                    // Don't not wait again
                    connectionEstablished = true;
                }
                for (int i = 0; i < retries; i++) {
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
            deviceUpdateTimer = new System.Threading.Timer(DeviceStatus_Tick, null, deviceUpdateInterval, deviceUpdateInterval);
        }

        #endregion

        #region Incoming socket calls
        private static void SetAlgorithmRates(JArray data) {
            try {
                foreach (var algo in data) {
                    var algoKey = (AlgorithmType)algo[0].Value<int>();
                    niceHashData.AppendPayingForAlgo(algoKey, algo[1].Value<double>());
                }
                AlgorithmRates = niceHashData.NormalizedSMA();
                OnSMAUpdate.Emit(null, EventArgs.Empty);
            } catch (Exception e) {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void zSetAlgorithmRates(zPoolAlgo[] data)
        {
            try
            {
                if (niceHashData == null) niceHashData = new NiceHashData(data);
                foreach (var algo in data)
                {
                    niceHashData.AppendPayingForAlgo((AlgorithmType)algo.NiceHashAlgoId(), (double)algo.MidPoint24HrEstimate);
                }
                AlgorithmRates = niceHashData.NormalizedSMA();
                OnSMAUpdate.Emit(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetBalance(string balance) {
            try {
                double bal = 0d;
                double.TryParse(balance, NumberStyles.Number, CultureInfo.InvariantCulture, out bal);
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

        public static void DeviceStatus_Tick(object state) {
            var devices = ComputeDeviceManager.Avaliable.AllAvaliableDevices;
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
            //NiceHashConnection.SendData(sendData);
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

    public class zPoolAlgo
    {
        public string name { get; set; }
        public int port { get; set; }
        //public decimal coins { get; set; }
        //public decimal fees { get; set; }
        //public decimal hashrate { get; set; }
        //public int workers { get; set; }
        public decimal estimate_current { get; set; }
        public decimal estimate_last24h { get; set; }
        public decimal actual_last24h { get; set; }

        public decimal NormalizedEstimate => MagnitudeFactor(name) * estimate_current;
        public decimal Normalized24HrEstimate => MagnitudeFactor(name) * estimate_last24h;
        public decimal Normalized24HrActual => MagnitudeFactor(name) * actual_last24h * 0.001m;
        public decimal MidPoint24HrEstimate => (Normalized24HrEstimate + Normalized24HrActual) / 2m;

        // if the normalized estimate (now) is 20% less than the midpoint, we want to return the 
        // normalized estimate
        public decimal Safe24HrEstimate => NormalizedEstimate * 1.2m < MidPoint24HrEstimate
            ? NormalizedEstimate
            : MidPoint24HrEstimate;

        //public decimal hashrate_last24h { get; set; }
        //public decimal rental_current { get; set; }

        public zAlgorithm Algorithm => ToAlgorithm(name);
        private zAlgorithm ToAlgorithm(string s)
        {
            switch (s.ToLower())
            {
                case "bitcore": return zAlgorithm.bitcore;
                case "blake2s": return zAlgorithm.blake2s;
                case "blakecoin": return zAlgorithm.blake256r8;
                case "c11": return zAlgorithm.c11;
                case "decred": return zAlgorithm.decred;
                case "equihash": return zAlgorithm.equihash;
                case "groestl": return zAlgorithm.groestl;
                case "hmq1725": return zAlgorithm.hmq1725;
                case "lbry": return zAlgorithm.lbry;
                case "lyra2v2": return zAlgorithm.lyra2v2;
                case "m7m": return zAlgorithm.m7m;
                case "myr-gr": return zAlgorithm.myriad_groestl;
                case "neoscrypt": return zAlgorithm.neoscrypt;
                case "nist5": return zAlgorithm.nist5;
                case "quark": return zAlgorithm.quark;
                case "qubit": return zAlgorithm.qubit;
                case "scrypt": return zAlgorithm.scrypt;
                case "sha256": return zAlgorithm.sha256;
                case "sib": return zAlgorithm.sib;
                case "skein": return zAlgorithm.skein;
                case "timetravel": return zAlgorithm.timetravel;
                case "veltor": return zAlgorithm.veltor;
                case "x11": return zAlgorithm.x11;
                case "x11evo": return zAlgorithm.x11evo;
                case "x13": return zAlgorithm.x13;
                case "x14": return zAlgorithm.x14;
                case "x15": return zAlgorithm.x15;
                case "x17": return zAlgorithm.x17;
                case "xevan": return zAlgorithm.xevan;
                case "yescrypt": return zAlgorithm.yescrypt;
                case "skunk": return zAlgorithm.skunk;
                case "keccak": return zAlgorithm.keccak;
            }

            return zAlgorithm.unknown;
        }

        public int NiceHashAlgoId() {
            switch (name)
            {
                case "blake2s": return 28;
                case "blakecoin": return 16;
                case "decred": return 21;
                case "equihash": return 24;
                case "lbry": return 23;
                case "lyra2v2": return 14;
                case "neoscrypt": return 8;
                case "nist5": return 7;
                case "quark": return 12;
                case "qubit": return 11;
                case "scrypt": return 0;
                case "sha256": return 1;
                case "x11": return 3;
                case "x13": return 4;
                case "x15": return 6;
                case "keccak": return 5;
                case "skunk": return 29;
                case "sib": return 26;

                default: return -1;
            }
        }

        private decimal MagnitudeFactor(string s)
        {
            switch (s)
            {
                case "decred":
                case "bitcore":
                case "blakecoin":
                case "blake2s":
                case "keccak":
                    return 1;
                case "equihash": return 1e6m;
                case "sha256":
                case "x11":
                case "quark":
                case "qubit":
                    return 1e-3m;
                default: return 1e3m;
            }
        }

        private decimal Min(params decimal[] values) =>
            values.Length == 1
                ? values[0]
                : values.Length == 2
                    ? Math.Min(values[0], values[1])
                    : Min(values[0], Min(values.Skip(1).ToArray()));

        private decimal Max(params decimal[] values) =>
            values.Length == 1
                ? values[0]
                : values.Length == 2
                    ? Math.Max(values[0], values[1])
                    : Max(values[0], Min(values.Skip(1).ToArray()));
    }

    public enum zAlgorithm
    {
        scrypt,
        sha256,
        scryptnf,
        x11,
        x13,
        keccak,
        x15,
        nist5,
        neoscrypt,
        lyra2re,
        whirlpoolx,
        qubit,
        quark,
        axiom,
        lyra2v2,
        scryptjanenf16,
        blake256r8,
        blake256r14,
        blake256r8vnl,
        hodl,
        ethash,
        decred,
        cryptonight,
        lbry,
        equihash,
        pascal,
        x11gost,
        sia,
        blake2s,
        skunk,
        // This is the ones not supported by NiceHash ... we need to keep this in order .. oops
        lyra2z,
        yescrypt,
        skein,
        myriad_groestl,
        groestl,
        unknown,
        bitcore,
        c11,
        hmq1725,
        m7m,
        sib,
        timetravel,
        veltor,
        x11evo,
        x14,
        x17,
        xevan,
        tribus
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHM.Common;
using NHM.Common.Enums;
using NHM.DeviceMonitoring.TDP;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Switching;
using NHMCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;
// static imports
using NHLog = NHM.Common.Logger;

namespace NHMCore.Nhmws.V4
{
    static class NHWebSocketV4
    {
        #region locking

        private static readonly object _lock = new object();
        private class LockingProperty<T>
        {
            public LockingProperty(T v)
            {
                _value = v;
            }

            private T _value;
            public T Value
            {
                get
                {
                    lock (_lock)
                    {
                        return _value;
                    }
                }
                set
                {
                    lock (_lock)
                    {
                        _value = value;
                    }
                }
            }
        }
        static private LockingProperty<DateTime> _lastSendMinerStatusTimestamp = new LockingProperty<DateTime>(DateTime.MinValue);
        static private LockingProperty<DateTime?> _notifyMinerStatusAfter = new LockingProperty<DateTime?>(null);
        static private LockingProperty<bool> _isInRPC = new LockingProperty<bool>(false);

        #endregion locking

        private enum MessageType
        {
            CLOSE_WEBSOCKET = 0,
            SEND_MESSAGE_STATUS,
        }

        static private bool _isNhmwsRestart = false;

        static private bool IsWsAlive => _webSocket?.ReadyState == WebSocketState.Open;
        static private WebSocket _webSocket = null;
        static private string _address;

        static private LoginMessage _login = new LoginMessage
        {
            Version = new List<string> { "NHM/" + Application.ProductVersion, "NA/NA" },
            Btc = DemoUser.BTC,
        };

        static private ConcurrentQueue<MessageEventArgs> _recieveQueue { get; set; } = new ConcurrentQueue<MessageEventArgs>();
        static private ConcurrentQueue<IEnumerable<(MessageType type, string msg)>> _sendQueue { get; set; } = new ConcurrentQueue<IEnumerable<(MessageType type, string msg)>>();

        private static void EnqueueParams(this ConcurrentQueue<IEnumerable<(MessageType type, string msg)>> send, params (MessageType type, string msg)[] arr)
        {
            send.Enqueue(arr);
        }

        public static void NotifyStateChanged()
        {
            // check if we are in RPC and if not send miner status
            if (!_isInRPC.Value)
            {
                _notifyMinerStatusAfter.Value = DateTime.UtcNow.AddSeconds(1);
            }
        }

        public static Task MainLoop { get; private set; } = null;

        public static void StartLoop(string address, CancellationToken token)
        {
            MainLoop = Task.Run(() => Start(address, token));
        }

        private static async Task Start(string address, CancellationToken token)
        {
            try
            {
                var random = new Random();
                _address = address;

                NHLog.Info("NHWebSocket-WD", "Starting nhmws watchdog");
                // TODO use this or just use the application exit source
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await NewConnection(token);
                        // drain remaining messages
                        while (await HandleReceiveMessage()) { }
                        // after each connection is completed check if we should re-connect or exit the watchdog
                        // if we didn't initialize the restart delay reconnect
                        if (!_isNhmwsRestart && !token.IsCancellationRequested)
                        {
                            // delays re-connect 10 to 30 seconds
                            var delaySeconds = 10 + random.Next(0, 20);
                            NHLog.Info("NHWebSocket-WD", $"Attempting reconnect in {delaySeconds} seconds");
                            await TaskHelpers.TryDelay(TimeSpan.FromSeconds(delaySeconds), token);
                        }
                        else if (_isNhmwsRestart && !token.IsCancellationRequested)
                        {
                            NHLog.Info("NHWebSocket-WD", $"Restarting nhmws SESSION");
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        NHLog.Debug("NHWebSocket-WD", $"TaskCanceledException {e.Message}");
                        return;
                    }
                    catch (Exception e)
                    {
                        // delays re-connect 10 to 30 seconds
                        var delaySeconds = 10 + random.Next(0, 20);
                        NHLog.Error("NHWebSocket-WD", $"Attempting reconnect in {delaySeconds} seconds. Error occured: {e.Message}.");
                        await TaskHelpers.TryDelay(TimeSpan.FromSeconds(delaySeconds), token);
                    }
                }
            }
            finally
            {
                NHLog.Info("NHWebSocket-WD", "Ending nhmws watchdog");
            }
        }

        static private async Task NewConnection(CancellationToken stop)
        {
            NHLog.Info("NHWebSocket", "STARTING nhmws SESSION");
            try
            {
                if (_webSocket is IDisposable d && d is not null)
                {
                    NHLog.Debug("NHWebSocket", "Disposing old websocket");
                    d.Dispose();
                }

                // TODO think if we might want to dump prev data????
                // on each new connection clear the ConcurrentQueues, 
                _recieveQueue = new ConcurrentQueue<MessageEventArgs>();
                _sendQueue = new ConcurrentQueue<IEnumerable<(MessageType type, string msg)>>();
                _isNhmwsRestart = false;
                _notifyMinerStatusAfter.Value = null;

                NHLog.Info("NHWebSocket", "Creating socket");
                using var webSocket = new WebSocket(_address);
                _webSocket = webSocket;
                _webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                //stop.Register(() => _webSocket.Close(CloseStatusCode.Normal, "Closing CancellationToken"));
                _webSocket.OnOpen += Login;
                _webSocket.OnMessage += (s, eMsg) =>
                {
                    NHLog.Debug("NHWebSocket", $"OnMessage: {eMsg.Data}");
                    _recieveQueue.Enqueue(eMsg);
                };
                _webSocket.OnError += (s, e) => NHLog.Info("NHWebSocket", $"Error occured: {e.Message}");
                _webSocket.OnClose += (s, e) => NHLog.Info("NHWebSocket", $"Connection closed code {e.Code}: {e.Reason}"); ;
                _webSocket.Log.Level = LogLevel.Debug;
                _webSocket.Log.Output = (data, s) => NHLog.Info("NHWebSocket", data.ToString());
                _webSocket.EnableRedirection = true;

                NHLog.Info("NHWebSocket", "Connecting");
                _webSocket.Connect();

                const int MINER_STATUS_TICK_SECONDS = 45;
                var checkWaitTime = TimeSpan.FromMilliseconds(50);

                var skipMinerStatus = !CredentialValidators.ValidateBitcoinAddress(_login.Btc);

                NHLog.Info("NHWebSocket", "Starting Loop");
                while (IsWsAlive && !stop.IsCancellationRequested)
                {
                    if (IsWsAlive) HandleSendMessage();
                    if (IsWsAlive) await HandleReceiveMessage();
                    // TODO add here the last miner status send check
                    if (IsWsAlive) await TaskHelpers.TryDelay(checkWaitTime, stop);

                    if (skipMinerStatus) continue;
                    var elapsedTime = DateTime.UtcNow - _lastSendMinerStatusTimestamp.Value;
                    if (elapsedTime.TotalSeconds > MINER_STATUS_TICK_SECONDS)
                    {
                        var minerStatusJsonStr = CreateMinerStatusMessage();
                        _sendQueue.EnqueueParams((MessageType.SEND_MESSAGE_STATUS, minerStatusJsonStr));
                    }
                    if (_notifyMinerStatusAfter.Value.HasValue && DateTime.UtcNow >= _notifyMinerStatusAfter.Value.Value)
                    {
                        _notifyMinerStatusAfter.Value = null;
                        var minerStatusJsonStr = CreateMinerStatusMessage();
                        _sendQueue.EnqueueParams((MessageType.SEND_MESSAGE_STATUS, minerStatusJsonStr));
                    }
                }
                // Ws closed
                NHLog.Info("NHWebSocket", "Exited Loop");
                // drain queue
                try
                {
                    var first = _webSocket
                    .GetType()
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.Name == "_messageEventQueue")
                    .FirstOrDefault();
                    var queue = first?.GetValue(_webSocket) as IEnumerable<MessageEventArgs> ?? Enumerable.Empty<MessageEventArgs>();
                    foreach (var item in queue) _recieveQueue.Enqueue(item);
                }
                catch (Exception e)
                {
                    NHLog.Error("NHWebSocket", $"Drain queue {e.Message}");
                }
            }
            catch (TaskCanceledException e)
            {
                NHLog.Debug("NHWebSocket", $"TaskCanceledException {e.Message}");
            }
            finally
            {
                NHLog.Info("NHWebSocket", "ENDING nhmws SESSION");
                ApplicationStateManager.SetNhmwsConnectionChanged(false);
            }
        }

        private static void Send(string data)
        {
            NHLog.Info("NHWebSocket", $"Sending data: {data}");
            _webSocket?.Send(data);
        }

        static private void HandleSendMessage()
        {
            var ok = _sendQueue.TryDequeue(out var sendMsgCommands);
            if (!ok) return;
            var msgs = sendMsgCommands.Where(p => p.msg != null).ToArray();
            foreach (var (type, data) in msgs)
            {
                switch (type)
                {
                    case MessageType.CLOSE_WEBSOCKET:
                        _webSocket?.Close(CloseStatusCode.Normal, data);
                        _isNhmwsRestart = true;
                        break;
                    case MessageType.SEND_MESSAGE_STATUS:
                        Send(data);
                        _lastSendMinerStatusTimestamp.Value = DateTime.UtcNow;
                        break;
                    default:
                        // TODO throw if we get here
                        break;
                }
            }
        }

        static private async Task<bool> HandleReceiveMessage()
        {
            if (_recieveQueue.TryDequeue(out var recieveMsg))
            {
                await HandleMessage(recieveMsg);
                return true;
            }
            return false;
        }

        static private void Login(object sender, EventArgs e)
        {
            NHLog.Info("NHWebSocket", "Connected");
            ApplicationStateManager.SetNhmwsConnectionChanged(true);
            try
            {
                // always send login
                var loginJson = JsonConvert.SerializeObject(_login);
                _sendQueue.EnqueueParams((MessageType.SEND_MESSAGE_STATUS, loginJson));
            }
            catch (Exception er)
            {
                NHLog.Info("NHWebSocket", er.Message);
            }
        }

        static public void ResetCredentials(string btc = null, string worker = null, string group = null)
        {
            // TODO check protocol
            // send status first and re-set credentials
            var minerStatusJsonStr = CreateMinerStatusMessage();
            _sendQueue.EnqueueParams((MessageType.SEND_MESSAGE_STATUS, minerStatusJsonStr));
            // TODO check 
            SetCredentials(btc, worker, group);
        }

        static public void SetCredentials(string btc = null, string worker = null, string group = null)
        {
            _login = MessageParserV4.CreateLoginMessage(btc, worker, ApplicationStateManager.RigID(), AvailableDevices.Devices.SortedDevices());
            if (!string.IsNullOrEmpty(btc)) _login.Btc = btc;
            if (worker != null) _login.Worker = worker;
            //if (group != null) _login.Group = group;
            // on credentials change always send close websocket message
            var closeMsg = (MessageType.CLOSE_WEBSOCKET, $"Credentials change reconnecting {ApplicationStateManager.Title}.");
            _sendQueue.EnqueueParams(closeMsg);
        }

        #region Message handling

        private static string CreateMinerStatusMessage() => JsonConvert.SerializeObject(MessageParserV4.GetMinerState(_login.Worker, AvailableDevices.Devices.SortedDevices()));

        static private async Task HandleMessage(MessageEventArgs e)
        {
            try
            {
                if (!e.IsText) return;
                NHLog.Info("NHWebSocket", $"Received: {e.Data}");
                var msg = MessageParserV4.ParseMessage(e.Data);
                var task = msg switch
                {
                    ObsoleteMessage => Task.CompletedTask,
                    IReceiveRpcMessage rpcMsg => HandleRpcMessage(rpcMsg),
                    IReceiveMessage rMsg => HandleNonRpcMessage(rMsg),
                    _ => throw new Exception($"Unhandeled message type {msg.Method}"),
                };
                await task;
            }
            catch (Exception ex)
            {
                NHLog.Error("NHWebSocket", $"HandleMessage {ex.Message}");
            }
        }

        // TODO copy pasted crap from NiceHashStats
        #region NonRpcMessages

        private static Task HandleSMAMessage(SmaMessage sma)
        {
            // TODO no try catch
            var (payingDict, stables) = MessageParser.ParseSmaMessageData(sma);
            NHSmaData.UpdateStableAlgorithms(stables);
            NHSmaData.UpdateSmaPaying(payingDict);
            // TODO new check crap 
            foreach (var dev in AvailableDevices.Devices) dev.UpdateEstimatePaying(payingDict);
            return Task.CompletedTask;
        }

        private static Task SetBalance(BalanceMessage msg)
        {
            try
            {
                var balance = MessageParser.ParseBalanceMessage(msg);
                if (balance.HasValue)
                {
                    BalanceAndExchangeRates.Instance.BtcBalance = balance.Value;
                }
            }
            catch (Exception e)
            {
                NHLog.Error("NHWebSocket", $"SetBalance error: {e.Message}");
            }
            return Task.CompletedTask;
        }

        private static Task HandleBurn(BurnMessage msg)
        {
            try
            {
                ApplicationStateManager.Burn(msg.Message);
            }
            catch (Exception e)
            {
                NHLog.Error("NHWebSocket", $"SetBalance error: {e.Message}");
            }
            return Task.CompletedTask;
        }

        private static Task SetVersion(VersionsMessage msg)
        {
            string version = msg.V3;
            VersionState.Instance.OnVersionUpdate(version);
            return Task.CompletedTask;
        }

        private static Task SetExchangeRates(ExchangeRatesMessage msg)
        {
            try
            {
                var (usdBtcRate, exchangesFiat) = MessageParser.ParseExchangeRatesMessageData(msg);
                BalanceAndExchangeRates.Instance.UpdateExchangesFiat(usdBtcRate, exchangesFiat);
            }
            catch (Exception e)
            {
                NHLog.Error("NHWebSocket", $"SetExchangeRates error: {e.Message}");
            }
            return Task.CompletedTask;
        }

        #endregion NonRpcMessages

        static private Task HandleNonRpcMessage(IReceiveMessage msg)
        {
            return msg switch
            {
                SmaMessage m => HandleSMAMessage(m),
                BalanceMessage m => SetBalance(m),
                VersionsMessage m => SetVersion(m),
                BurnMessage m => HandleBurn(m),
                ExchangeRatesMessage m => SetExchangeRates(m),
                _ => throw new Exception($"NonRpcMessage operation not supported for method '{msg.Method}'"),
            };
        }

        #region RpcMessages

        #region Credentials setters (btc/username, worker, group)
        private static async Task<bool> miningSetUsername(string btc)
        {
            var userSetResult = await ApplicationStateManager.SetBTCIfValidOrDifferent(btc, true);
            return userSetResult switch
            {
                NhmwsSetResult.CHANGED => true, // we return executed
                NhmwsSetResult.INVALID => throw new RpcException("Mining address invalid", ErrorCode.InvalidUsername),
                NhmwsSetResult.NOTHING_TO_CHANGE => throw new RpcException($"Nothing to change btc \"{btc}\" already set", ErrorCode.RedundantRpc),
                _ => throw new RpcException($"", ErrorCode.InternalNhmError),
            };
        }

        private static Task<bool> miningSetWorker(string worker)
        {
            var workerSetResult = ApplicationStateManager.SetWorkerIfValidOrDifferent(worker, true);
            return workerSetResult switch
            {
                NhmwsSetResult.CHANGED => Task.FromResult(true), // we return executed
                NhmwsSetResult.INVALID => throw new RpcException("Worker name invalid", ErrorCode.InvalidWorker),
                NhmwsSetResult.NOTHING_TO_CHANGE => throw new RpcException($"Nothing to change worker name \"{worker}\" already set", ErrorCode.RedundantRpc),
                _ => throw new RpcException($"", ErrorCode.InternalNhmError),
            };
        }

        private static Task<bool> miningSetGroup(string group)
        {
            var groupSetResult = ApplicationStateManager.SetGroupIfValidOrDifferent(group, true);
            return groupSetResult switch
            {
                NhmwsSetResult.CHANGED => Task.FromResult(true), // we return executed
                NhmwsSetResult.INVALID => throw new RpcException("Group name invalid", ErrorCode.UnableToHandleRpc), // TODO error code not correct
                NhmwsSetResult.NOTHING_TO_CHANGE => throw new RpcException($"Nothing to change group \"{group}\" already set", ErrorCode.RedundantRpc),
                _ => throw new RpcException($"", ErrorCode.InternalNhmError),
            };
        }
        #endregion Credentials setters (btc/username, worker, group)

        private static async Task<bool> SetDevicesEnabled(string devs, bool enabled)
        {
            bool allDevices = devs == "*";
            // get device with uuid if it exists, devs can be single device uuid
            var deviceWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(devs);

            // Check if RPC should execute
            // check if redundant rpc
            if (allDevices && enabled && AvailableDevices.IsEnableAllDevicesRedundantOperation())
            {
                throw new RpcException("All devices are already enabled.", ErrorCode.RedundantRpc);
            }
            // all disable
            if (allDevices && !enabled && AvailableDevices.IsDisableAllDevicesRedundantOperation())
            {
                throw new RpcException("All devices are already disabled.", ErrorCode.RedundantRpc);
            }
            // if single and doesn't exist
            if (!allDevices && deviceWithUUID == null)
            {
                throw new RpcException("Device not found", ErrorCode.NonExistentDevice);
            }
            // if we have the device but it is redundant
            if (!allDevices && deviceWithUUID.IsDisabled == !enabled)
            {
                var stateStr = enabled ? "enabled" : "disabled";
                throw new RpcException($"Devices with uuid {devs} is already {stateStr}.", ErrorCode.RedundantRpc);
            }

            // if got here than we can execute the call
            await ApplicationStateManager.SetDeviceEnabledState(null, (devs, enabled));
            // TODO invoke the event for controls that use it
            //OnDeviceUpdate?.Invoke(null, new DeviceUpdateEventArgs(AvailableDevices.Devices.ToList()));
            return true;
        }

        #region Start
        private static async Task<bool> startMiningAllDevices()
        {
            var allDisabled = AvailableDevices.Devices.All(dev => dev.IsDisabled);
            if (allDisabled)
            {
                throw new RpcException("All devices are disabled cannot start", ErrorCode.DisabledDevice);
            }
            var (success, msg) = await ApplicationStateManager.StartAllAvailableDevicesTask();
            if (!success)
            {
                throw new RpcException(msg, ErrorCode.RedundantRpc);
            }
            return true;
        }

        private static async Task<bool> startMiningOnDeviceWithUuid(string uuid)
        {
            string errMsgForUuid = $"Cannot start device with uuid {uuid}";
            // get device with uuid if it exists, devs can be single device uuid
            var deviceWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(uuid);
            if (deviceWithUUID == null)
            {
                throw new RpcException($"{errMsgForUuid}. Device not found.", ErrorCode.NonExistentDevice);
            }
            if (deviceWithUUID.IsDisabled)
            {
                throw new RpcException($"{errMsgForUuid}. Device is disabled.", ErrorCode.DisabledDevice);
            }
            var (success, msg) = await ApplicationStateManager.StartDeviceTask(deviceWithUUID);
            if (!success)
            {
                // TODO this can also be an error
                throw new RpcException($"{errMsgForUuid}. {msg}.", ErrorCode.RedundantRpc);
            }
            return true;
        }

        private static Task<bool> StartMining(string devs)
        {
            if (devs == "*") return startMiningAllDevices();
            return startMiningOnDeviceWithUuid(devs);
        }
        #endregion Start

        #region Stop
        private static async Task<bool> stopMiningAllDevices()
        {
            var allDisabled = AvailableDevices.Devices.All(dev => dev.IsDisabled);
            if (allDisabled)
            {
                throw new RpcException("All devices are disabled cannot stop", ErrorCode.DisabledDevice);
            }
            var (success, msg) = await ApplicationStateManager.StopAllDevicesTask();
            if (!success)
            {
                throw new RpcException(msg, ErrorCode.RedundantRpc);
            }
            return success;
        }

        private static async Task<bool> stopMiningOnDeviceWithUuid(string uuid)
        {
            string errMsgForUuid = $"Cannot stop device with uuid {uuid}";
            // get device with uuid if it exists, devs can be single device uuid
            var deviceWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(uuid);
            if (deviceWithUUID == null)
            {
                throw new RpcException($"{errMsgForUuid}. Device not found.", ErrorCode.NonExistentDevice);
            }
            if (deviceWithUUID.IsDisabled)
            {
                throw new RpcException($"{errMsgForUuid}. Device is disabled.", ErrorCode.DisabledDevice);
            }
            var (success, msg) = await ApplicationStateManager.StopDeviceTask(deviceWithUUID);
            if (!success)
            {
                // TODO this can also be an error
                throw new RpcException($"{errMsgForUuid}. {msg}.", ErrorCode.RedundantRpc);
            }
            return success;
        }

        private static Task<bool> StopMining(string devs)
        {
            if (devs == "*") return stopMiningAllDevices();
            return stopMiningOnDeviceWithUuid(devs);
        }
        #endregion Stop

        private static Task<bool> SetPowerMode(string device, TDPSimpleType level)
        {
            if (GlobalDeviceSettings.Instance.DisableDevicePowerModeSettings) throw new RpcException("Not able to set Power Mode: Device Power Mode Settings Disabled", ErrorCode.UnableToHandleRpc);

            var devs = device == "*" ?
                AvailableDevices.Devices :
                AvailableDevices.Devices.Where(d => d.B64Uuid == device);

            var found = devs.Count() > 0;
            var hasEnabled = false;
            var setSuccess = new List<(bool success, DeviceType type)>();
            foreach (var dev in devs)
            {
                if (!dev.Enabled) continue;
                if (!dev.CanSetPowerMode) continue;
                hasEnabled = true;
                // TODO check if set
                var result = dev.SetPowerMode(level);
                setSuccess.Add((result, dev.DeviceType));
            }

            if (!setSuccess.All(t => t.success))
            {
                if (setSuccess.Any(res => res.type == DeviceType.NVIDIA && !Helpers.IsElevated && !res.success))
                {
                    throw new RpcException("Not able to set power modes for devices: Must start NiceHashMiner as Admin", ErrorCode.UnableToHandleRpc);
                }
                throw new RpcException("Not able to set power modes for all devices", ErrorCode.UnableToHandleRpc);
            }

            if (found && !hasEnabled)
            {
                throw new RpcException("No settable devices found", ErrorCode.UnableToHandleRpc);
            }

            if (!found)
            {
                throw new RpcException("No settable devices found", ErrorCode.UnableToHandleRpc);
            }

            return Task.FromResult(true);
        }

        private static async Task<string> MinerReset(string level)
        {
            string appBurn()
            {
                ApplicationStateManager.Burn("MinerReset app burn called");
                return "";
            }
            string rigRestart()
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3 * 1000);
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        Arguments = "-r -f -t 0",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using var reboot = Process.Start(startInfo);
                    reboot.WaitForExit();
                });
                return "";
            }
            async Task<string> systemDump()
            {
                var result = await Task.Run(async () => await Helpers.CreateAndUploadLogReport());
                return result.isUploaded ? result.uploadUrl : "";
            }
            return level switch
            {
                "app burn" => appBurn(),
                "rig restart" => rigRestart(),
                "system dump" => await systemDump(),
                _ => throw new RpcException($"RpcMessage MinerReset operation not supported for level '{level}'", ErrorCode.UnableToHandleRpc),
            };
        }

        #endregion RpcMessages

        static private async Task HandleRpcMessage(IReceiveRpcMessage rpcMsg)
        {
            bool success = false;
            ExecutedCall executedCall = null;
            try
            {
                _isInRPC.Value = true;
                // throw if pending
                if (ApplicationStateManager.CalcRigStatus() == RigStatus.Pending)
                {
                    throw new RpcException($"Cannot handle RPC call Rig is in PENDING state.", ErrorCode.UnableToHandleRpc);
                }
                object t = rpcMsg switch
                {
                    MiningSetUsername m => await miningSetUsername(m.Btc),
                    MiningSetWorker m => await miningSetWorker(m.Worker),
                    MiningSetGroup m => await miningSetGroup(m.Group),
                    MiningEnable m => await SetDevicesEnabled(m.Device, true),
                    MiningDisable m => await SetDevicesEnabled(m.Device, false),
                    MiningStart m => await StartMining(m.Device),
                    MiningStop m => await StopMining(m.Device),
                    MiningSetPowerMode m => await SetPowerMode(m.Device, (TDPSimpleType)m.PowerMode),
                    MinerReset m => await MinerReset(m.Level), // rpcAnswer
                    _ => throw new RpcException($"RpcMessage operation not supported for method '{rpcMsg.Method}'", ErrorCode.UnableToHandleRpc),
                };

                string rpcAnswer = t is string rpcAnws ? rpcAnws : null;
                if (t is bool ok) success = ok;
                executedCall = new ExecutedCall(rpcMsg.Id, 0, rpcAnswer);
            }
            catch (RpcException rpcEx)
            {
                executedCall = new ExecutedCall(rpcMsg.Id, rpcEx.Code, rpcEx.Message);
            }
            catch (Exception e)
            {
                NHLog.Error("NHWebSocket", $"Non RpcException - error: {e.Message}");
                // intenral nhm error
                if (executedCall == null) executedCall = new ExecutedCall(rpcMsg.Id, 1, "Internal NiceHash Miner Error");
            }
            finally
            {
                _isInRPC.Value = false;
                if (executedCall != null)
                {
                    // SEND ONLY WHEN status changed
                    // send miner status and send executed
                    var minerStatusMsg = CreateMinerStatusMessage();
                    Send(minerStatusMsg);
                    _lastSendMinerStatusTimestamp.Value = DateTime.UtcNow;
                    // Then executed
                    Send(JsonConvert.SerializeObject(executedCall));
                    // Login if we have to
                    if (success && rpcMsg is ISetCredentialsMessage credMsg)
                    {
                        var (btc, worker, group) = MessageParser.ParseCredentialsMessage(credMsg);
                        SetCredentials(btc, worker, group);
                    }
                }
            }
        }
        #endregion Message handling

    }
}

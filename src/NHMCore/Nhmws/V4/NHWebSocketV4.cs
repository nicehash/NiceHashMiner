using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHM.Common;
using NHM.Common.Enums;
using NHM.DeviceMonitoring.TDP;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Configs.Managers;
using NHMCore.Mining;
using NHMCore.Notifications;
using NHMCore.Switching;
using NHMCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using Windows.Media.PlayTo;
using Logger = NHM.Common.Logger;
// static imports
using NHLog = NHM.Common.Logger;

namespace NHMCore.Nhmws.V4
{
    public static class NHWebSocketV4
    {
        #region locking
        private static readonly string _logTag = "NHWebSocketV4";
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
        private static readonly string _TAG = "NHWebSocketV4";
        static private bool _isNhmwsRestart = false;

        static private bool IsWsAlive => _webSocket?.ReadyState == WebSocketState.Open;
        static private WebSocket _webSocket = null;
        static private string _address;

        static private LoginMessage _login = new LoginMessage
        {
            Version = new List<string> { $"NHM/{NHMApplication.ProductVersion}", Environment.OSVersion.ToString() },
            Btc = DemoUser.BTC,
        };
        private static MinerState CachedState = null;

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
            ActionMutableMap.ResetArrays();
            if (CachedState != null) CachedState = null;
            _login = MessageParserV4.CreateLoginMessage(btc, worker, ApplicationStateManager.RigID(), AvailableDevices.Devices.SortedDevices());
            if (!string.IsNullOrEmpty(btc)) _login.Btc = btc;
            if (worker != null) _login.Worker = worker;
            //if (group != null) _login.Group = group;
            // on credentials change always send close websocket message
            var closeMsg = (MessageType.CLOSE_WEBSOCKET, $"Credentials change reconnecting {ApplicationStateManager.Title}.");
            _sendQueue.EnqueueParams(closeMsg);
        }

        #region Message handling
        private static bool AreEqual(JArray first, JArray second)
        {
            if (first == null && second == null) return true;
            if (first == null || second == null) return false;
            return JsonConvert.SerializeObject(first) == JsonConvert.SerializeObject(second);
        }
        private static MinerState.DeviceState GetDeviceStateDelta(MinerState.DeviceState first, MinerState.DeviceState second)
        {
            if (first == null || second == null) return second;
            var devState = new MinerState.DeviceState();
            if (!AreEqual(first.OptionalDynamicValues, second.OptionalDynamicValues))
            {
                devState.OptionalDynamicValues = second.OptionalDynamicValues;
            }
            if (!AreEqual(first.MandatoryDynamicValues, second.MandatoryDynamicValues))
            {
                devState.MandatoryDynamicValues = second.MandatoryDynamicValues;
            }
            if (!AreEqual(first.OptionalMutableValues, second.OptionalMutableValues))
            {
                devState.OptionalMutableValues = second.OptionalMutableValues;
            }
            if (!AreEqual(first.MandatoryMutableValues, second.MandatoryMutableValues))
            {
                devState.MandatoryMutableValues = second.MandatoryMutableValues;
            }
            return devState;
        }
        private static bool AreDeviceListsComparable(List<MinerState.DeviceState> first, List<MinerState.DeviceState> second)
        {
            if (first == null || second == null) return true;
            if (first.Count != second.Count) return false;
            return true;
        }

        private static MinerState GetDeltaProperties(MinerState prev, MinerState next)
        {
            MinerState ret = new MinerState();
            if (!AreEqual(prev.MutableDynamicValues, next.MutableDynamicValues))
            {
                ret.MutableDynamicValues = next.MutableDynamicValues;
            }
            if (!AreEqual(prev.OptionalDynamicValues, next.OptionalDynamicValues))
            {
                ret.OptionalDynamicValues = next.OptionalDynamicValues;
            }
            if (!AreEqual(prev.OptionalMutableValues, next.OptionalMutableValues))
            {
                ret.OptionalMutableValues = next.OptionalMutableValues;
            }
            if (!AreEqual(prev.MandatoryMutableValues, next.MandatoryMutableValues))
            {
                ret.MandatoryMutableValues = next.MandatoryMutableValues;
            }

            if (AreDeviceListsComparable(prev.Devices, next.Devices))
            {
                if (prev.Devices == null && next.Devices == null) return ret;
                if(prev.Devices == null && next.Devices != null)
                {
                    ret.Devices = next.Devices;
                    return ret;
                }
                if(prev.Devices != null && next.Devices == null)
                {
                    ret.Devices = null;
                    return ret;
                }
                ret.Devices = new();
                for(int i = 0; i < next.Devices.Count; i++)
                {
                    ret.Devices.Add(GetDeviceStateDelta(prev.Devices[i], next.Devices[i]));
                }
            }
            else
            {
                ret.Devices = next.Devices;
            }
            return ret;
        }
        private static string CreateMinerStatusMessage()
        {
            var nextState = MessageParserV4.GetMinerState(_login.Worker, AvailableDevices.Devices.SortedDevices());
            var shrinkedState = new MinerState();
            var json = string.Empty;
            if (CachedState != null) //if we have something cached
            {
                shrinkedState = GetDeltaProperties(CachedState, nextState);
                json = JsonConvert.SerializeObject(shrinkedState);
            }
            else
            {
                json = JsonConvert.SerializeObject(nextState);
            }
            CachedState = nextState;
            return json;
        }

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

        #region Actions

        #endregion Actions

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
        private static Task<(ErrorCode err, string msg)> CallAction(MinerCallAction action)
        {
            var actionRecord = ActionMutableMap.FindActionOrNull(action.ActionId);
            if (actionRecord == null)
            {
                Logger.Error("NHWebSocketV4", "Action not found");
                return Task.FromResult((ErrorCode.ActionNotFound, "Action not found"));
            }
            //action has single parameter anyway FOR NOW
            //in the future return multiple actions success/partial/failiure
            var ret = (ErrorCode.NoError, string.Empty);
            foreach (var param in action.Parameters)
            {
                ret = ParseAndCallAction(actionRecord.DeviceUUID, action.Id, actionRecord.ActionType, param).Result;
            }
            if (!action.Parameters.Any())
            {
                ret = ParseAndCallAction(actionRecord.DeviceUUID, action.Id, actionRecord.ActionType, string.Empty).Result;
            }
            return Task.FromResult(ret);
        }
        private static Task<(ErrorCode err, string msg)> ParseAndCallAction(string deviceUUID, int messageID, SupportedAction typeOfAction, string parameters)
        {
            ErrorCode err = ErrorCode.NoError;
            var result = string.Empty;
            switch (typeOfAction)
            {
                case SupportedAction.ActionStartMining:
                    NHLog.Warn(_logTag, "This type of action is handled through old protocol: " + typeOfAction);
                    break;
                case SupportedAction.ActionStopMining:
                    NHLog.Warn(_logTag, "This type of action is handled through old protocol: " + typeOfAction);
                    break;
                case SupportedAction.ActionRebenchmark:
                    if(deviceUUID == string.Empty) (err, result) = ApplicationStateManager.StartReBenchmark().Result;
                    else
                    {
                        (err, result) = ApplicationStateManager.StartRebenchmarkSpecific(deviceUUID).Result;
                    }
                    break;
                case SupportedAction.ActionProfilesBundleSet:
                    if (!Helpers.IsElevated)
                    {
                        (err, result) = (ErrorCode.ErrNotAdmin, "No admin privileges");
                        break;
                    }
                    var bundle = JsonConvert.DeserializeObject<Bundle>(parameters);
                    _ = ExecuteProfilesBundleReset(false);
                    _ = ExecuteProfilesBundleSet(bundle);
                    MiningState.Instance.CalculateDevicesStateChange();
                    (err, result) = (ErrorCode.NoError, "OK");
                    if(err == ErrorCode.NoError)
                    {
                        EventManager.Instance.AddEvent(EventManager.EventType.BundleApplied);
                    }
                    break;
                case SupportedAction.ActionProfilesBundleReset:
                    if (!Helpers.IsElevated)
                    {
                        (err, result) = (ErrorCode.ErrNotAdmin, "No admin privileges");
                        break;
                    }
                    ExecuteProfilesBundleReset();
                    MiningState.Instance.CalculateDevicesStateChange();
                    (err, result) = (ErrorCode.NoError, "OK");
                    break;
                case SupportedAction.ActionDeviceEnable:
                    NHLog.Warn(_logTag, "This type of action is handled through old protocol: " + typeOfAction);
                    break;
                case SupportedAction.ActionDeviceDisable:
                    NHLog.Warn(_logTag, "This type of action is handled through old protocol: " + typeOfAction);
                    break;
                case SupportedAction.ActionOcProfileTest:
                    if (!Helpers.IsElevated)
                    {
                        (err, result) = (ErrorCode.ErrNotAdmin, "No admin privileges");
                        break;
                    }
                    var oc = JsonConvert.DeserializeObject<OcProfile>(parameters);
                    (err, result) = ExecuteOCTest(deviceUUID, oc).Result;
                    break;
                case SupportedAction.ActionOcProfileTestStop:
                    if (!Helpers.IsElevated)
                    {
                        (err, result) = (ErrorCode.ErrNotAdmin, "No admin privileges");
                        break;
                    }
                    (err, result) = StopOCTestForDevice(deviceUUID).Result;
                    break;
                case SupportedAction.ActionFanProfileTest:
                    if (!Helpers.IsElevated)
                    {
                        (err, result) = (ErrorCode.ErrNotAdmin, "No admin privileges");
                        break;
                    }
                    var fan = JsonConvert.DeserializeObject<FanProfile>(parameters);
                    (err, result) = ExecuteFanTest(deviceUUID, fan).Result;
                    break;
                case SupportedAction.ActionFanProfileTestStop:
                    if (!Helpers.IsElevated)
                    {
                        (err, result) = (ErrorCode.ErrNotAdmin, "No admin privileges");
                        break;
                    }
                    (err, result) = StopFanTestForDevice(deviceUUID).Result;
                    break;
                case SupportedAction.ActionElpProfileTest:
                    if (!Helpers.IsElevated)
                    {
                        (err, result) = (ErrorCode.ErrNotAdmin, "No admin privileges");
                        break;
                    }
                    var elp = JsonConvert.DeserializeObject<ElpProfile>(parameters);
                    (err, result) = ExecuteELPTest(deviceUUID, elp).Result;
                    MiningState.Instance.CalculateDevicesStateChange();
                    break;
                case SupportedAction.ActionElpProfileTestStop:
                    if (!Helpers.IsElevated)
                    {
                        (err, result) = (ErrorCode.ErrNotAdmin, "No admin privileges");
                        break;
                    }
                    (err, result) = StopELPTestForDevice(deviceUUID).Result;
                    MiningState.Instance.CalculateDevicesStateChange();
                    break;
                default:
                    NHLog.Warn(_logTag, "This type of action is unsupported: " + typeOfAction);
                    break;
            }
            _ = UpdateMinerStatus();
            return Task.FromResult((err, result));
        }
        public static Task UpdateMinerStatus()
        {
            var minerStatusJsonStr = CreateMinerStatusMessage();
            _sendQueue.EnqueueParams((MessageType.SEND_MESSAGE_STATUS, minerStatusJsonStr));
            return Task.CompletedTask;
        }

        private static Task ExecuteProfilesBundleSet(Bundle bundle)
        {
            BundleManager.SetBundleInfo(bundle.Name, bundle.Id);
            _ = BundleManager.SaveBundle(bundle);
            if (bundle.OcBundles != null)
            {
                var retOC = OCManager.Instance.ApplyOcBundle(bundle.OcBundles);
            }
            if (bundle.FanBundles != null)
            {
                var retFan = FanManager.Instance.ApplyFanBundle(bundle.FanBundles);
            }
            if (bundle.ElpBundles != null)
            {
                var retELP = ELPManager.Instance.ApplyELPBundle(bundle.ElpBundles);
            }
            return Task.CompletedTask;
        }
        private static Task ExecuteProfilesBundleReset(bool triggerSwitch = true)
        {
            BundleManager.ResetBundleInfo();
            var retOC = OCManager.Instance.ResetOcBundle(triggerSwitch);
            var retFan = FanManager.Instance.ResetFanBundle(triggerSwitch);
            var retElp = ELPManager.Instance.ResetELPBundle(triggerSwitch);
            return Task.CompletedTask;
        }
        private static Task<(ErrorCode err, string msg)> ExecuteOCTest(string deviceUUID, OcProfile ocBundle)
        {
            if (!Helpers.IsElevated) return Task.FromResult((ErrorCode.ErrNotAdmin, "No administrator privileges"));
            StopELPTestForDevice(deviceUUID, false);
            StopFanTestForDevice(deviceUUID, false);
            ELPManager.Instance.RestartMiningInstanceIfNeeded();
            var res = OCManager.Instance.ExecuteTest(deviceUUID, ocBundle);
            return Task.FromResult(res.Result);
        }
        private static Task<(ErrorCode err, string msg)> StopOCTestForDevice(string deviceUUID, bool triggerSwitch = true)
        {
            if (!Helpers.IsElevated) return Task.FromResult((ErrorCode.ErrNotAdmin, "No administrator privileges"));
            ELPManager.Instance.RestartMiningInstanceIfNeeded();
            var res = OCManager.Instance.StopTest(deviceUUID, triggerSwitch);
            return Task.FromResult(res.Result);
        }
        private static Task<(ErrorCode err, string msg)> ExecuteELPTest(string deviceUUID, ElpProfile elpBundle)
        {
            StopFanTestForDevice(deviceUUID, false);
            StopOCTestForDevice(deviceUUID, false);
            ELPManager.Instance.RestartMiningInstanceIfNeeded();
            var res = ELPManager.Instance.ExecuteTest(deviceUUID, elpBundle);
            return Task.FromResult(res.Result);
        }
        private static Task<(ErrorCode err, string msg)> StopELPTestForDevice(string deviceUUID, bool triggerSwitch = true)
        {
            var res = ELPManager.Instance.StopTest(deviceUUID, triggerSwitch);
            ELPManager.Instance.RestartMiningInstanceIfNeeded();
            return Task.FromResult(res.Result);
        }

        private static Task<(ErrorCode err, string msg)> ExecuteFanTest(string deviceUUID, FanProfile fanBundle)
        {
            if (!Helpers.IsElevated) return Task.FromResult((ErrorCode.ErrNotAdmin, "No administrator privileges"));
            StopELPTestForDevice(deviceUUID, false);
            StopOCTestForDevice(deviceUUID, false);
            ELPManager.Instance.RestartMiningInstanceIfNeeded();
            var res = FanManager.Instance.ExecuteTest(deviceUUID, fanBundle);
            return Task.FromResult(res.Result);
        }

        private static Task<(ErrorCode err, string msg)> StopFanTestForDevice(string deviceUUID, bool triggerSwitch = true)
        {
            if (!Helpers.IsElevated) return Task.FromResult((ErrorCode.ErrNotAdmin, "No administrator privileges"));
            ELPManager.Instance.RestartMiningInstanceIfNeeded();
            var res = FanManager.Instance.StopTest(deviceUUID, triggerSwitch);
            return Task.FromResult(res.Result);
        }
        private static Task<string> SetMutable(MinerSetMutable mutableCmd)
        {
            if (mutableCmd.Properties != null)
            {
                var resArray = new List<int>();
                foreach (var property in mutableCmd.Properties)
                {
                    resArray.Add(HandleProperty(property).Result);
                }
                if (resArray.All(r => r == 0)) return Task.FromResult(string.Empty);
                return Task.FromResult($"SetMutable error ({string.Join(",", resArray)})");
            }
            if (mutableCmd.Devices == null) return Task.FromResult(string.Empty);
            string result = string.Empty;
            foreach (var device in mutableCmd.Devices)
            {
                if (device.Properties == null) continue;
                var deviceTarget = AvailableDevices.Devices.Where(d => d.B64Uuid == device.Id).FirstOrDefault();
                if (deviceTarget == null) continue;
                if (deviceTarget.IsMiningBenchingTesting)
                {
                    result += $"({device.Id}):Stop device first\n";
                    continue;
                }
                foreach (var property in device.Properties)
                {
                    HandleProperty(property);
                }
            }
            return Task.FromResult(result);
        }
        private static Task<int> HandleProperty(object property)
        {
            if (property is not JToken token) return Task.FromResult(-1);
            var genericProperty = token.ToObject<Property>();
            var mutable = ActionMutableMap.FindMutableOrNull(genericProperty.PropId);//this is null if per rig
            if (mutable == null) return Task.FromResult(-2);
            object t = mutable.PropertyType switch
            {
                Type.String => ParseAndActMutableString(mutable, token),
                Type.Int => ParseAndActMutableInt(mutable, token),
                Type.Enum => ParseAndActMutableEnum(mutable, token),
                Type.Bool => ParseAndActMutableBool(mutable, token),
                _ => throw new InvalidOperationException()
            };
            if(t is Task<int> res) return Task.FromResult(res.Result);
            return Task.FromResult(0);
        }
        static Task<int> ParseAndActMutableString(OptionalMutableProperty property, JToken command)
        {
            var mutable = command.ToObject<PropertyString>();
            var res = property.ExecuteTask(mutable.Value);
            if (res.Result is int resInt) return Task.FromResult(resInt);
            return Task.FromResult(-100);
        }
        static Task ParseAndActMutableInt(OptionalMutableProperty property, JToken command)
        {
            var mutable = command.ToObject<PropertyInt>();
            return Task.CompletedTask;
        }
        static Task ParseAndActMutableEnum(OptionalMutableProperty property, JToken command)
        {
            var mutable = command.ToObject<PropertyEnum>();
            return Task.CompletedTask;
        }
        static Task<int> ParseAndActMutableBool(OptionalMutableProperty property, JToken command)
        {
            var mutable = command.ToObject<PropertyBool>();
            var res = property.ExecuteTask(mutable.Value);
            if (res.Result is int resInt) return Task.FromResult(resInt);
            return Task.FromResult(-101);
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
                    MinerCallAction m => await CallAction(m),
                    MinerSetMutable m => await SetMutable(m),
                    _ => throw new RpcException($"RpcMessage operation not supported for method '{rpcMsg.Method}'", ErrorCode.UnableToHandleRpc),
                };
                if (t is bool ok)
                {
                    success = ok;
                    executedCall = new ExecutedCall(rpcMsg.Id, 0, string.Empty);
                }
                else if (t is (ErrorCode err, string msg))
                {
                    success = err == ErrorCode.NoError ? true : false;
                    executedCall = new ExecutedCall(rpcMsg.Id, (int)err, msg);
                }
                else if (t is string answer)
                {
                    var errorCode = answer == string.Empty ? 0 : 1;
                    executedCall = new ExecutedCall(rpcMsg.Id, errorCode, answer);
                }
                else executedCall = new ExecutedCall(rpcMsg.Id, -1, "Failed to execute!");
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

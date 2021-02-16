using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace NHM.MinerPluginToolkitV1
{
    /// <summary>
    /// FreePortsCheckerManager class is checking reserved ports for miners (<see cref="MinerBase"/>).
    /// It is integrated with NiceHash Miner and client. User can specify ports for each algorithm.
    /// </summary>
    public static class FreePortsCheckerManager
    {
        public static int ApiBindPortPoolStart { get; set; } = 4000;
        private static int _portPlusRange => 2300;

        private static object _lock = new object();
        private static int _reserveTimeSeconds => 5;
        private static Dictionary<int, DateTime> _reservedPortsAtTime { get; set; } = new Dictionary<int, DateTime>();

        private static bool IsPortFree(int port, IPEndPoint[] tcpOrUdpPorts)
        {
            var isTaken = tcpOrUdpPorts.Any(tcp => tcp.Port == port);
            return !isTaken;
        }

        private static bool CanReservePort(int port, DateTime currentTime)
        {
            if (_reservedPortsAtTime.ContainsKey(port))
            {
                var reservedTime = _reservedPortsAtTime[port];
                var secondsDiff = (currentTime - reservedTime).TotalSeconds;
                return secondsDiff > _reserveTimeSeconds;
            }
            return true;
        }

        public static int GetAvaliablePortFromSettings()
        {
            return GetAvaliablePortInRange(Enumerable.Range(ApiBindPortPoolStart, _portPlusRange));
        }

        public static int GetAvaliablePortInRange(IEnumerable<int> portsRange)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpIpEndpoints = ipGlobalProperties.GetActiveTcpListeners();
            var udpIpEndpoints = ipGlobalProperties.GetActiveUdpListeners();

            var now = DateTime.UtcNow;
            foreach (var port in portsRange)
            {
                var tcpFree = IsPortFree(port, tcpIpEndpoints);
                var udpFree = IsPortFree(port, udpIpEndpoints);
                lock (_lock)
                {
                    var canReserve = CanReservePort(port, now);
                    if (tcpFree && udpFree && canReserve)
                    {
                        // reserve port and return
                        _reservedPortsAtTime[port] = now;
                        return port;
                    }
                }
            }
            return -1; // we can't retrive free port
        }
    }
}

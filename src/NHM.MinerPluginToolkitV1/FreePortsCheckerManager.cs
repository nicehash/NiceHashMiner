using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int _portPlusRange = 2300;

        private static readonly object _lock = new object();
        private const int _reserveTimeSeconds = 5;
        private static readonly Dictionary<int, DateTime> _reservedPortsAtTime = new Dictionary<int, DateTime>();

        private static bool CanReservePort(int port, DateTime currentTime)
        {
            if (!_reservedPortsAtTime.ContainsKey(port)) return true;
            var reservedTime = _reservedPortsAtTime[port];
            var secondsDiff = (currentTime - reservedTime).TotalSeconds;
            return secondsDiff > _reserveTimeSeconds;
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
                var isTcpTaken = tcpIpEndpoints.Any(e => e.Port == port);
                var isUdpTaken = udpIpEndpoints.Any(e => e.Port == port);
                if (isTcpTaken || isUdpTaken) continue;
                lock (_lock)
                {
                    if (CanReservePort(port, now))
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

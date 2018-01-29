using NiceHashMiner.Configs;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace NiceHashMiner.Miners
{
    public static class MinersApiPortsManager
    {
        private static readonly HashSet<int> UsedPorts = new HashSet<int>();

        public static bool IsPortAvaliable(int port)
        {
            var isAvailable = true;
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            // check TCP
            {
                var tcpIpEndpoints = ipGlobalProperties.GetActiveTcpListeners();
                isAvailable = tcpIpEndpoints.All(tcp => tcp.Port != port);
            }
            // check UDP
            if (isAvailable)
            {
                var udpIpEndpoints = ipGlobalProperties.GetActiveUdpListeners();
                if (udpIpEndpoints.Any(udp => udp.Port == port))
                {
                    isAvailable = false;
                }
            }
            return isAvailable;
        }

        public static int GetAvaliablePort()
        {
            var port = ConfigManager.GeneralConfig.ApiBindPortPoolStart;
            var newPortEnd = port + 3000;
            for (; port < newPortEnd; ++port)
            {
                if (MinersSettingsManager.AllReservedPorts.Contains(port) == false && IsPortAvaliable(port) && UsedPorts.Add(port))
                {
                    break;
                }
            }
            return port;
        }

        public static void RemovePort(int port)
        {
            UsedPorts.Remove(port);
        }
    }
}

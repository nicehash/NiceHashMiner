using System.Linq;
using System.Net.NetworkInformation;

namespace MinerPlugin.Toolkit
{
    // RENAME MinersApiPortsManager to FreePortsCheckerManager
    public static class MinersApiPortsManager
    {
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

        public static int GetAvaliablePortInRange(int portStart = 4000, int next = 300)
        {
            var port = portStart;
            var newPortEnd = portStart + next;
            for (; port < newPortEnd; ++port)
            {
                if (IsPortAvaliable(port))
                {
                    return port;
                }
            }
            return -1; // we can't retrive free port
        }

        // TODO add implement get random port
    }
}

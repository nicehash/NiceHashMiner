using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;

namespace XMRig
{
    internal static class PluginInternalSettings
    {
        static string _urlPort => $"{MinerCommandLineSettings.POOL_URL_TEMPLATE}:{MinerCommandLineSettings.POOL_PORT_TEMPLATE}";
        static string _url=> MinerCommandLineSettings.POOL_URL_TEMPLATE;
        static string _username => MinerCommandLineSettings.USERNAME_TEMPLATE;
        static string _apiPort => MinerCommandLineSettings.API_PORT_TEMPLATE;
        static string _extraLaunchParameters => MinerCommandLineSettings.EXTRA_LAUNCH_PARAMETERS_TEMPLATE;

        internal static MinerCommandLineSettings MinerCommandLineSettings = new MinerCommandLineSettings
        {
            DevicesSeparator = ",",
            AlgorithmCommandLine = new Dictionary<string, string>
            {
                {
                    $"{AlgorithmType.RandomXmonero}",
                    $"-a rx/0 -o {_urlPort} -u {_username} --http-enabled --http-port={_apiPort} --nicehash --donate-level=1 {_extraLaunchParameters}"
                }
            },
            AlgorithmCommandLineSSL = new Dictionary<string, string>
            {
                {
                    $"{AlgorithmType.RandomXmonero}",
                    $"-a rx/0 -o {_url}:443 -u {_username} --http-enabled --http-port={_apiPort} --nicehash --tls --donate-level=1 {_extraLaunchParameters}"
                }
            }
        };


    }
}

using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;

namespace NBMiner
{
    internal static class PluginInternalSettings
    {
        static string _urlPort => $"{MinerCommandLineSettings.POOL_URL_TEMPLATE}:{MinerCommandLineSettings.POOL_PORT_TEMPLATE}";
        static string _url => MinerCommandLineSettings.POOL_URL_TEMPLATE;
        static string _username => MinerCommandLineSettings.USERNAME_TEMPLATE;
        static string _apiPort => MinerCommandLineSettings.API_PORT_TEMPLATE;
        static string _devices => MinerCommandLineSettings.DEVICES_TEMPLATE;
        static string _extraLaunchParameters => MinerCommandLineSettings.EXTRA_LAUNCH_PARAMETERS_TEMPLATE;

        internal static MinerCommandLineSettings MinerCommandLineSettings = new MinerCommandLineSettings
        {
            AlgorithmCommandLine = new Dictionary<string, string>
            {
                {
                    $"{AlgorithmType.DaggerHashimoto}",
                    $"-a ethash -o nicehash+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.CuckooCycle}",
                    $"-a cuckoo_ae -o stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.KAWPOW}",
                    $"-a kawpow -o stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.BeamV3}",
                    $"-a beamv3 -o stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.Octopus}",
                    $"-a octopus -o stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.Autolykos}",
                    $"-a ergo -o stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                }
            },
            AlgorithmCommandLineSSL = new Dictionary<string, string>
            {
                {
                    $"{AlgorithmType.DaggerHashimoto}",
                    $"-a ethash -o nicehash+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.CuckooCycle}",
                    $"-a cuckoo_ae -o stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.KAWPOW}",
                    $"-a kawpow -o stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.BeamV3}",
                    $"-a beamv3 -o stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.Octopus}",
                    $"-a octopus -o stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.Autolykos}",
                    $"-a ergo -o stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --no-watchdog {_extraLaunchParameters}"
                }
            }
        };

        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 1, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout = DefaultTimeout,
        };

        internal static MinerBenchmarkTimeSettings BenchmarkTimeSettings = new MinerBenchmarkTimeSettings
        {
            PerAlgorithm = new Dictionary<BenchmarkPerformanceType, Dictionary<string, int>>(){
                { BenchmarkPerformanceType.Quick, new Dictionary<string, int>(){ { "KAWPOW", 160 } } },
                { BenchmarkPerformanceType.Standard, new Dictionary<string, int>(){ { "KAWPOW", 180 } } },
                { BenchmarkPerformanceType.Precise, new Dictionary<string, int>(){ { "KAWPOW", 260 } } }
            }
        };


    }
}

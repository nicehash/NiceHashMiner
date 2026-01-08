using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP.GMiner
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
                    $"-a ethash -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.EtcHash}",
                    $"-a etchash -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.KAWPOW}",
                    $"-a kawpow -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.Autolykos}",
                    $"-a autolykos2 -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.BeamV3}",
                    $"-a beamhash -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.CuckooCycle}",
                    $"-a aeternity -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.ZelHash}",
                    $"-a equihash125_4 -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.ZHash}",
                    $"-a equihash144_5 --pers auto -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.Octopus}",
                    $"-a octopus -s stratum+tcp://{_urlPort} -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
            },
            AlgorithmCommandLineSSL = new Dictionary<string, string>
            {
                {
                    $"{AlgorithmType.DaggerHashimoto}",
                    $"-a ethash -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.EtcHash}",
                    $"-a etchash -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.KAWPOW}",
                    $"-a kawpow -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.Autolykos}",
                    $"-a autolykos2 -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.BeamV3}",
                    $"-a beamhash -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.CuckooCycle}",
                    $"-a aeternity -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.ZelHash}",
                    $"-a equihash125_4 -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.ZHash}",
                    $"-a equihash144_5 --pers auto -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
                {
                    $"{AlgorithmType.Octopus}",
                    $"-a octopus -s stratum+ssl://{_url}:443 -u {_username} --api 127.0.0.1:{_apiPort} -d {_devices} --watchdog 0 {_extraLaunchParameters}"
                },
            }
        };

        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 5, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            UseUserSettings = true,
            Enabled = true,
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

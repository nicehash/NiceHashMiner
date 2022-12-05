using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;

namespace NanoMiner
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
                    $"-algo ethash -pool1 {_urlPort} -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.KAWPOW}",
                    $"-algo kawpow -pool1 {_urlPort} -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.Octopus}",
                    $"-algo octopus -pool1 {_urlPort} -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.Autolykos}",
                    $"-algo autolykos -pool1 {_urlPort} -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.EtcHash}",
                    $"-algo etchash -pool1 {_urlPort} -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.VerusHash}",
                    $"-algo verushash -pool1 {_urlPort} -wallet {_username} -webport {_apiPort} -watchdog 0 {_extraLaunchParameters} -rigname"
                }
            },
            AlgorithmCommandLineSSL = new Dictionary<string, string>
            {
                {
                    $"{AlgorithmType.DaggerHashimoto}",
                    $"-algo ethash -pool1 {_url}:443 -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.KAWPOW}",
                    $"-algo kawpow -pool1 {_url}:443 -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.Octopus}",
                    $"-algo octopus -pool1 {_url}:443 -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.Autolykos}",
                    $"-algo autolykos -pool1 {_url}:443 -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.EtcHash}",
                    $"-algo etchash -pool1 {_url}:443 -wallet {_username} -webport {_apiPort} -devices {_devices} -watchdog 0 {_extraLaunchParameters} -rigname"
                },
                {
                    $"{AlgorithmType.VerusHash}",
                    $"-algo verushash -pool1 {_url}:443 -wallet {_username} -webport {_apiPort} -watchdog 0 {_extraLaunchParameters} -rigname"
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

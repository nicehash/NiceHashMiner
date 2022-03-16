using NHM.Common.Configs;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NHM.Common
{
    public static class StratumServiceHelpers
    {
        #region CUSTOM_ENDPOINTS
        internal class ServiceCustomSettings
        {
            internal class StratumTemplateEntry
            {
                public string Template { get; set; } = "";
                public int Port { get; set; } = -1;
            }

            public string NhmSocketAddress { get; set; } = "";
            public Dictionary<AlgorithmType, StratumTemplateEntry> StratumEndpointTemplatesByAlgorithmType { get; set; } = new Dictionary<AlgorithmType, StratumTemplateEntry>();

            const string PREFIX_TEMPLATE = "{PREFIX://}";
            const string PORT_TEMPLATE = "{:PORT}";

            public static ServiceCustomSettings Defaults()
            {
                var nonDepricatedAlgorithms = Enum.GetValues(typeof(AlgorithmType))
                    .Cast<AlgorithmType>()
                    .Where(a => !a.IsObsolete())
                    .Where(a => GetAlgorithmUrlName(a).ok)
                    .Select(a => (algorithmType: a, name: GetAlgorithmUrlName(a).name))
                    .ToArray();
                var stratumTemplates = new Dictionary<AlgorithmType, StratumTemplateEntry>();
                foreach (var (algorithmType, name) in nonDepricatedAlgorithms)
                {
                    stratumTemplates[algorithmType] = new StratumTemplateEntry
                    {
                        // we get something like this "{PREFIX://}daggerhashimoto.{LOCATION}.nicehash.com{:PORT}"
                        Template = $"{PREFIX_TEMPLATE}{name}.auto.nicehash.com{PORT_TEMPLATE}",
                        Port = 9200 // 9200 or 443
                    };
                }
                return new ServiceCustomSettings
                {
                    NhmSocketAddress = Nhmws.BuildTagNhmSocketAddress(),
                    StratumEndpointTemplatesByAlgorithmType = stratumTemplates
                };
            }

            internal string GetLocationUrl(AlgorithmType algorithmType, NhmConectionType conectionType)
            {
                if (StratumEndpointTemplatesByAlgorithmType.ContainsKey(algorithmType))
                {
                    var customEndpointTemplateEntry = StratumEndpointTemplatesByAlgorithmType[algorithmType];
                    return GetCustomUrl(customEndpointTemplateEntry, conectionType);
                }
                return "";
            }

            internal (string url, int port) GetLocationUrlV2(AlgorithmType algorithmType, NhmConectionType conectionType)
            {
                if (StratumEndpointTemplatesByAlgorithmType.ContainsKey(algorithmType))
                {
                    var customEndpointTemplateEntry = StratumEndpointTemplatesByAlgorithmType[algorithmType];
                    var customPort = customEndpointTemplateEntry.Port;
                    var customEndpointTemplate = customEndpointTemplateEntry.Template
                        .Replace(PREFIX_TEMPLATE, "")
                        .Replace(PORT_TEMPLATE, "");
                    return (customEndpointTemplate, customPort);
                }
                return ("", 0);
            }

            private static string GetCustomUrl(StratumTemplateEntry customEndpointTemplateEntry, NhmConectionType conectionType)
            {
                var (prefix, _) = GetProtocolPrefixAndPort(conectionType);
                var customPort = customEndpointTemplateEntry.Port;

                var customEndpointTemplate = customEndpointTemplateEntry.Template
                    .Replace(PREFIX_TEMPLATE, prefix)
                    .Replace(PORT_TEMPLATE, $":{customPort}");
                return customEndpointTemplate;
            }
        }
        
        internal static string NhmSocketAddress => _serviceCustomSettings.NhmSocketAddress;
        private static ServiceCustomSettings _serviceCustomSettings;
        internal static void InitStratumServiceHelpers()
        {
            if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED == false) return;
            (_serviceCustomSettings, _) = InternalConfigs.GetDefaultOrFileSettings(Paths.RootPath("custom_endpoints_settings.json"), ServiceCustomSettings.Defaults());
        }
        #endregion CUSTOM_ENDPOINTS

        public static bool UseDNSQ { get; set; } = true;

        private static (string name, bool ok) GetAlgorithmUrlName(AlgorithmType algorithmType)
        {
            if (algorithmType < 0) return ("", false);
            var (name, ok) = algorithmType.GetName();
            // return lowercase
            return (name.ToLower(), ok);
        }

        private static (string prefix, int port) GetProtocolPrefixAndPort(NhmConectionType conectionType)
        {
            int port = conectionType == NhmConectionType.STRATUM_SSL ? 443 : 9200;
            switch (conectionType)
            {
                case NhmConectionType.STRATUM_TCP: return ("stratum+tcp://", port);
                case NhmConectionType.STRATUM_SSL: return ("stratum+ssl://", port);
                // NHMConectionType.NONE
                default: return ("", port);
            }
        }

        public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
        {
            if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED) return _serviceCustomSettings.GetLocationUrl(algorithmType, conectionType);
            
            var (name, okName) = GetAlgorithmUrlName(algorithmType);
            // if name is not ok return
            if (!okName) return "";

            var (prefix, port) = GetProtocolPrefixAndPort(conectionType);
            
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) 
                return $"{prefix}{name}-test.auto.nicehash.com:{port}";
            
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV)
                return $"{prefix}{name}-dev.auto.nicehash.com:{port}";

            //BuildTag.PRODUCTION
            var url = name + ".auto.nicehash.com";
            if (UseDNSQ)
            {
                var urlT = Task.Run(async () => await DNSQuery.QueryOrDefault(url));
                urlT.Wait();
                var (IP, gotIP) = urlT.Result;
                if (gotIP) url = IP;
            }
            return prefix + url + $":{port}";
        }

        public static async Task<(string url, int port)> GetLocationUrlV2(AlgorithmType algorithmType, bool ssl = false)
        {
            if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED) return _serviceCustomSettings.GetLocationUrlV2(algorithmType, NhmConectionType.NONE);

            var (name, okName) = GetAlgorithmUrlName(algorithmType);
            // if name is not ok return
            if (!okName) return ("", -1);
            int port = ssl ? 443 : 9200;

            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET)
                return ($"{name}-test.auto.nicehash.com", port);

            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV)
                return ($"{name}-dev.auto.nicehash.com", port);

            //BuildTag.PRODUCTION
            var url = name + ".auto.nicehash.com";
            if (UseDNSQ)
            {
                var (IP, gotIP) = await DNSQuery.QueryOrDefault(url);
                if (gotIP) return (IP, port);
            }
            return (url, port);
        }
    }
}

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
        private static int GetAlgorithmTypePort(AlgorithmType algorithmType, bool ssl = false)
        {
            int port = 3333 + (int)algorithmType;
            return ssl ? 30000 + port : port;
        }

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

            const string LOCATION_TEMPLATE = "{LOCATION}";
            const string PREFIX_TEMPLATE = "{PREFIX://}";
            const string PORT_TEMPLATE = "{:PORT}";
            //const string NAME_TEMPLATE = "NAME";

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
                        Template = $"{PREFIX_TEMPLATE}{name}.{LOCATION_TEMPLATE}.nicehash.com{PORT_TEMPLATE}",
                        Port = GetAlgorithmTypePort(algorithmType)
                    };
                }
                return new ServiceCustomSettings
                {
                    NhmSocketAddress = Nhmws.BuildTagNhmSocketAddress(),
                    StratumEndpointTemplatesByAlgorithmType = stratumTemplates
                };
            }

            public string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
            {
                if (StratumEndpointTemplatesByAlgorithmType.ContainsKey(algorithmType))
                {
                    var customEndpointTemplateEntry = StratumEndpointTemplatesByAlgorithmType[algorithmType];
                    return GetCustomUrl(customEndpointTemplateEntry, algorithmType, miningLocation, conectionType);
                }
                return "";
            }

            private static string GetCustomUrl(StratumTemplateEntry customEndpointTemplateEntry, AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
            {
                var (prefix, _) = GetProtocolPrefixAndPort(algorithmType, conectionType);
                var customPort = customEndpointTemplateEntry.Port;
                if (conectionType == NhmConectionType.STRATUM_SSL) customPort = customPort + 30000;

                var customEndpointTemplate = customEndpointTemplateEntry.Template
                    .Replace(PREFIX_TEMPLATE, prefix)
                    .Replace(LOCATION_TEMPLATE, miningLocation)
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

        public class Location
        {
            public Location(string code, string name, params string[] markets)
            {
                Code = code;
                Name = name;
                Markets = markets;
            }

            public string Code { get; }
            public string Name { get; }
            public IReadOnlyList<string> Markets { get; }
            public bool IsOperational { get; private set; } = true;

            public (bool IsOperationalBeforeSet, bool IsOperationalAfterSet) SetAndReturnIsOperational(IEnumerable<string> markets)
            {
                var before = IsOperational;
                IsOperational = Markets.Any(market => markets.Contains(market));
                return (before, IsOperational);
            }
        }

        public static IReadOnlyList<Location> MiningServiceLocations = new List<Location>
        {
            new Location("eu", "Europe", "EU", "EU_N"),
            new Location("usa", "USA", "USA", "USA_E"),
            new Location("eu-west", "Europe - West", "EU"),
            new Location("eu-north", "Europe - North", "EU_N"),
            new Location("usa-west", "USA - West", "USA"),
            new Location("usa-east", "USA - East", "USA_E"),
        };

        private static (string prefix, int port) GetProtocolPrefixAndPort(AlgorithmType algorithmType, NhmConectionType conectionType)
        {
            int port = GetAlgorithmTypePort(algorithmType, conectionType == NhmConectionType.STRATUM_SSL);
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
            if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED) return _serviceCustomSettings.GetLocationUrl(algorithmType, miningLocation, conectionType);
            if (NhmConectionType.LOCKED == conectionType) return miningLocation;

            var (name, okName) = GetAlgorithmUrlName(algorithmType);
            // if name is not ok return
            if (!okName) return "";

            var (prefix, port) = GetProtocolPrefixAndPort(algorithmType, conectionType);
            
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) 
                return $"{prefix}stratum-test.{miningLocation}.nicehash.com:{port}";
            
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV)
                return $"{prefix}stratum-dev.{miningLocation}.nicehash.com:{port}";

            //BuildTag.PRODUCTION
            var url = name + "." + miningLocation + ".nicehash.com";
            if (UseDNSQ)
            {
                var urlT = Task.Run(async () => await DNSQuery.QueryOrDefault(url));
                urlT.Wait();
                var (IP, gotIP) = urlT.Result;
                if (gotIP) url = IP;
            }
            return prefix + url + $":{port}";
        }
    }
}

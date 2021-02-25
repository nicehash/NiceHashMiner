using Newtonsoft.Json;
using NHM.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace NHM.Common
{
    public static class StratumServiceHelpers
    {
        #region CUSTOM_ENDPOINTS
        internal class StratumTemplateEntry
        {
            public string Template { get; set; } = "";
            public int Port { get; set; } = -1;
        }
        internal class ServiceCustomSettings
        {
            public string NhmSocketAddress { get; set; } = "";
            public Dictionary<AlgorithmType, StratumTemplateEntry> StratumEndpointTemplatesByAlgorithmType { get; set; } = new Dictionary<AlgorithmType, StratumTemplateEntry>();
        }
        const string LOCATION_TEMPLATE = "{LOCATION}";
        const string PREFIX_TEMPLATE = "{PREFIX://}";
        const string PORT_TEMPLATE = "{:PORT}";
        //const string NAME_TEMPLATE = "NAME";
        internal static string NhmSocketAddress { get; private set; }
        private static Dictionary<AlgorithmType, StratumTemplateEntry> _stratumEndpointTemplatesByAlgorithmType { get; set; } = new Dictionary<AlgorithmType, StratumTemplateEntry>();
        static StratumServiceHelpers()
        {
            if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED == false) return;
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
            };
            const string customSettingsFile = "custom_endpoints_settings.json";
            if (File.Exists(customSettingsFile))
            {
                var customSettings = JsonConvert.DeserializeObject<ServiceCustomSettings>(File.ReadAllText(customSettingsFile), jsonSettings);
                if (customSettings != null)
                {
                    NhmSocketAddress = customSettings.NhmSocketAddress;
                    _stratumEndpointTemplatesByAlgorithmType = customSettings.StratumEndpointTemplatesByAlgorithmType;
                }
            }
            else
            {
                foreach (AlgorithmType algorithmType in Enum.GetValues(typeof(AlgorithmType)))
                {
                    if (algorithmType < 0) continue;
                    var nPort = 3333 + algorithmType;
                    var name = GetAlgorithmUrlName(algorithmType).name;
                    var endpointTemplate = $"{PREFIX_TEMPLATE}{name}.{LOCATION_TEMPLATE}.nicehash.com{PORT_TEMPLATE}";
                    _stratumEndpointTemplatesByAlgorithmType[algorithmType] = new StratumTemplateEntry
                    {
                        Template = endpointTemplate,
                        Port = (int)nPort
                    };
                }

                // create defaults
                var defaultCustomSettings = new ServiceCustomSettings
                {
                    NhmSocketAddress = Nhmws.BuildTagNhmSocketAddress(),
                    StratumEndpointTemplatesByAlgorithmType = _stratumEndpointTemplatesByAlgorithmType,
                };
                File.WriteAllText(customSettingsFile, JsonConvert.SerializeObject(defaultCustomSettings, Formatting.Indented));
            }
        }
        #endregion CUSTOM_ENDPOINTS

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

        public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
        {
            var (name, ok) = GetAlgorithmUrlName(algorithmType);
            // if name is not ok return
            if (!ok) return "";

            var nPort = 3333 + algorithmType;
            var sslPort = 30000 + nPort;

            // NHMConectionType.NONE
            var prefix = "";
            var port = nPort;
            switch (conectionType)
            {
                case NhmConectionType.LOCKED:
                    return miningLocation;
                case NhmConectionType.STRATUM_TCP:
                    prefix = "stratum+tcp://";
                    break;
                case NhmConectionType.STRATUM_SSL:
                    prefix = "stratum+ssl://";
                    port = sslPort;
                    break;
            }
            if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED)
            {
                var customEndpointTemplateEntry = _stratumEndpointTemplatesByAlgorithmType[algorithmType];
                var customPort = customEndpointTemplateEntry.Port;
                if (conectionType == NhmConectionType.STRATUM_SSL)
                {
                    customPort = customPort + 30000;
                }
                var customEndpointTemplate = customEndpointTemplateEntry.Template;
                customEndpointTemplate = customEndpointTemplate.Replace(PREFIX_TEMPLATE, prefix);
                customEndpointTemplate = customEndpointTemplate.Replace(LOCATION_TEMPLATE, miningLocation);
                customEndpointTemplate = customEndpointTemplate.Replace(PORT_TEMPLATE, $":{customPort}");
                return customEndpointTemplate;
            }
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET)
            {
                return prefix
                   + "algo-test." + miningLocation
                   + ".nicehash.com:"
                   + port;
            }
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV)
            {
                return prefix
                   + "stratum-dev." + miningLocation
                   + ".nicehash.com:"
                   + port;
            }
            //BuildTag.PRODUCTION
            return prefix
                   + name
                   + "." + miningLocation
                   + ".nicehash.com:"
                   + port;
        }
    }
}

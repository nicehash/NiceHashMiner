using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NiceHashMinerLegacy.Common
{
    public static class StratumServiceHelpers
    {
#region CUSTOM_ENDPOINTS
#if CUSTOM_ENDPOINTS
        class StratumTemplateEntry
        {
            public string Template { get; set; } = "";
            public int Port { get; set; } = -1;
        }
        class ServiceCustomSettings
        {
            public string NhmSocketAddress { get; set; } = "";
            public Dictionary<AlgorithmType, StratumTemplateEntry> StratumEndpointTemplatesByAlgorithmType { get; set; } = new Dictionary<AlgorithmType, StratumTemplateEntry>();
        }
        const string LOCATION_TEMPLATE = "{LOCATION}";
        const string PREFIX_TEMPLATE = "{PREFIX://}";
        const string PORT_TEMPLATE = "{:PORT}";
        //const string NAME_TEMPLATE = "NAME";
        public static string NhmSocketAddress { get; private set; }
        private static Dictionary<AlgorithmType, StratumTemplateEntry> _stratumEndpointTemplatesByAlgorithmType { get; set; } = new Dictionary<AlgorithmType, StratumTemplateEntry>();
        static StratumServiceHelpers()
        {
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
                    var name = GetAlgorithmUrlName(algorithmType);
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
                    NhmSocketAddress = "https://nhmws.nicehash.com/v2/nhm",
                    StratumEndpointTemplatesByAlgorithmType = _stratumEndpointTemplatesByAlgorithmType,
                };
                File.WriteAllText(customSettingsFile, JsonConvert.SerializeObject(defaultCustomSettings, Formatting.Indented));
            }
        }
#endif
#endregion CUSTOM_ENDPOINTS

        private static string GetAlgorithmUrlName(AlgorithmType algorithmType)
        {
            if (algorithmType < 0)
            {
                return "";
            }
            const string NOT_FOUND = "NameNotFound type not supported";
            var name = Enum.GetName(typeof(AlgorithmType), algorithmType) ?? NOT_FOUND;
            if (name == NOT_FOUND)
            {
                return "";
            }
            // strip out the _UNUSED
            name = name.Replace("_UNUSED", "");
            return name.ToLower();
        }

        public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
        {
            var name = GetAlgorithmUrlName(algorithmType);
            // if name is empty return
            if (name == "") return "";

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

#if CUSTOM_ENDPOINTS
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
#elif TESTNET
            return prefix
                   + name
                   + "-test." + miningLocation
                   + ".nicehash.com:"
                   + port;
#elif TESTNETDEV
            return prefix
                   + "stratum-test." + miningLocation
                   + ".nicehash.com:"
                   + port;
#elif PRODUCTION_NEW
            return prefix
                   + name
                   + "." + miningLocation
                   + ".nicehash.com:"
                   + port;
#else
            return prefix
                   + name
                   + "." + miningLocation
                   + ".nicehash.com:"
                   + port;
#endif
        }
    }
}

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

#if CUSTOM_ENDPOINTS
        class ServiceCustomSettings
        {
            public string NhmSocketAddress { get; set; } = "";
            public Dictionary<AlgorithmType, int> AlgorithmTypePort { get; set; } = new Dictionary<AlgorithmType, int>();
            public Dictionary<AlgorithmType, string> AlgorithmTypeEndpoint { get; set; } = new Dictionary<AlgorithmType, string>();
            public bool IgnoreProtocolPrefix { get; set; } = false;
        }

        public static string NhmSocketAddress { get; private set; }
        private static Dictionary<AlgorithmType, int> _algorithmTypePort = new Dictionary<AlgorithmType, int>();
        private static Dictionary<AlgorithmType, string> _algorithmTypeEndpoint = new Dictionary<AlgorithmType, string>();
        private static bool _ignoreProtocolPrefix = false;
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
                    _algorithmTypePort = customSettings.AlgorithmTypePort;
                    _algorithmTypeEndpoint = customSettings.AlgorithmTypeEndpoint;
                    _ignoreProtocolPrefix = customSettings.IgnoreProtocolPrefix;
                }
            }
            else
            {
                var algorithmTypePort = new Dictionary<AlgorithmType, int>();
                var algorithmTypeEndpoint = new Dictionary<AlgorithmType, string>();
                foreach (AlgorithmType algorithmType in Enum.GetValues(typeof(AlgorithmType)))
                {
                    if (algorithmType < 0) continue;
                    var nPort = 3333 + algorithmType;
                    var name = GetAlgorithmUrlName(algorithmType);
                    var endpoint = $"PREFIX://{name}.LOCATION.nicehash.com";
                    algorithmTypePort[algorithmType] = (int)nPort;
                    algorithmTypeEndpoint[algorithmType] = endpoint;
                }

                // create defaults
                var defaultCustomSettings = new ServiceCustomSettings
                {
                    NhmSocketAddress = "https://nhmws.nicehash.com/v2/nhm",
                    IgnoreProtocolPrefix = false,
                    AlgorithmTypePort = algorithmTypePort,
                    AlgorithmTypeEndpoint = algorithmTypeEndpoint
                };
                File.WriteAllText(customSettingsFile, JsonConvert.SerializeObject(defaultCustomSettings, Formatting.Indented));
            }
        }
#endif

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
            if (_ignoreProtocolPrefix)
            {
                prefix = "";
            }
            var customEndpoint = _algorithmTypeEndpoint[algorithmType];
            var customPort = _algorithmTypeEndpoint[algorithmType];
            return $"{prefix}{customEndpoint}:{customPort}";
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

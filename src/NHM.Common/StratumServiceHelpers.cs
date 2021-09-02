using Newtonsoft.Json;
using NHM.Common.Configs;
using NHM.Common.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
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

            public static ServiceCustomSettings Defaults()
            {
                var ret = new ServiceCustomSettings
                {
                    NhmSocketAddress = Nhmws.BuildTagNhmSocketAddress()
                };
                foreach (AlgorithmType algorithmType in Enum.GetValues(typeof(AlgorithmType)))
                {
                    if (algorithmType < 0) continue;
                    var name = GetAlgorithmUrlName(algorithmType).name;
                    // we get something like this "{PREFIX://}daggerhashimoto.{LOCATION}.nicehash.com{:PORT}"
                    ret.StratumEndpointTemplatesByAlgorithmType[algorithmType] = new StratumTemplateEntry
                    {
                        Template = $"{PREFIX_TEMPLATE}{name}.{LOCATION_TEMPLATE}.nicehash.com{PORT_TEMPLATE}",
                        Port = (int)(3333 + algorithmType)
                    };
                }
                return ret;
            }
        }
        const string LOCATION_TEMPLATE = "{LOCATION}";
        const string PREFIX_TEMPLATE = "{PREFIX://}";
        const string PORT_TEMPLATE = "{:PORT}";

        //const string NAME_TEMPLATE = "NAME";
        internal static string NhmSocketAddress => _serviceCustomSettings.NhmSocketAddress;
        private static ServiceCustomSettings _serviceCustomSettings;
        internal static void InitStratumServiceHelpers()
        {
            if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED == false) return;
            (_serviceCustomSettings, _) = InternalConfigs.GetDefaultOrFileSettings(Paths.RootPath("custom_endpoints_settings.json"), ServiceCustomSettings.Defaults());
        }
        #endregion CUSTOM_ENDPOINTS

        private static readonly object _lock = new object();
        static ConcurrentDictionary<string, string> urlMap = new ConcurrentDictionary<string, string>();
        static List<string> onlyURLs = new List<string>();
        static string[] urlPrefix = { "stratum+tcp://", "stratum+ssl://" };
        public static async Task InitUrlMap()
        {
            foreach (AlgorithmType algorithmType in Enum.GetValues(typeof(AlgorithmType)))
            {
                foreach (Location location in MiningServiceLocations)
                {
                    foreach (var prefix in urlPrefix)
                    {
                        var originalUrl = prefix + algorithmType + "." + location + ".nicehash.com";
                        var testUrl = prefix + "algo-test." + location + ".nicehash.com";
                        var devUrl = prefix + "stratum-dev." + location + ".nicehash.com";
                        urlMap.TryAdd(originalUrl, await DNSQuery.QueryOrDefault(originalUrl));
                        urlMap.TryAdd(testUrl, await DNSQuery.QueryOrDefault(testUrl));
                        urlMap.TryAdd(devUrl, await DNSQuery.QueryOrDefault(devUrl));
                        onlyURLs.Add(originalUrl);
                        onlyURLs.Add(testUrl);
                        onlyURLs.Add(devUrl);
                    }
                }
            }
            InitIntervalTimerForUrlUpdate();
        }
        private static void InitIntervalTimerForUrlUpdate()
        {
            int wait = 60 * 1000;//1 min intervals?
            var timerDelegate = new TimerCallback(UpdateIntervalCallback);
            Timer _dispatcherTimer = new Timer(timerDelegate, null, 0, wait);
        }

        private static async void UpdateIntervalCallback(object state)
        {
            await OnUpdateUrlInterval();
        }

        private static async Task OnUpdateUrlInterval()
        {
            ConcurrentDictionary<string, string> newUrlList = new ConcurrentDictionary<string, string>();
            foreach (var urlItem in onlyURLs)
            {
                newUrlList.TryAdd(urlItem, await DNSQuery.QueryOrDefault(urlItem));
            }
            lock (_lock)
            {
                urlMap = new ConcurrentDictionary<string, string>(newUrlList);
            }

        }

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
                var customEndpointTemplateEntry = _serviceCustomSettings.StratumEndpointTemplatesByAlgorithmType[algorithmType];
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
                return urlMap[prefix + "algo-test." + miningLocation + ".nicehash.com"];
            }
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV)
            {
                return urlMap[prefix + "stratum-dev." + miningLocation + ".nicehash.com"];
            }
            //BuildTag.PRODUCTION
            return urlMap[prefix + name + "." + miningLocation + ".nicehash.com"];
        }
    }
}

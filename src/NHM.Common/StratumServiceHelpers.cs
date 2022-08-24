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
                var port = GetProtocolPrefixAndPort(NhmConectionType.NONE, BuildOptions.BUILD_TAG).port;
                foreach (var (algorithmType, name) in nonDepricatedAlgorithms)
                {
                    stratumTemplates[algorithmType] = new StratumTemplateEntry
                    {
                        // we get something like this "{PREFIX://}daggerhashimoto.{LOCATION}.nicehash.com{:PORT}"
                        Template = GetAlgorithmURL(BuildOptions.BUILD_TAG, PREFIX_TEMPLATE, name, PORT_TEMPLATE),
                        Port = port // 9200 or 443
                    };
                }
                return new ServiceCustomSettings
                {
                    NhmSocketAddress = Nhmws.BuildTagNhmSocketAddress(),
                    StratumEndpointTemplatesByAlgorithmType = stratumTemplates
                };
            }

            internal string GetLocationUrl(AlgorithmType algorithmType, NhmConectionType conectionType, BuildTag buildTag)
            {
                if (StratumEndpointTemplatesByAlgorithmType.ContainsKey(algorithmType))
                {
                    var customEndpointTemplateEntry = StratumEndpointTemplatesByAlgorithmType[algorithmType];
                    return GetCustomUrl(customEndpointTemplateEntry, conectionType, buildTag);
                }
                return "";
            }

            private static string GetCustomUrl(StratumTemplateEntry customEndpointTemplateEntry, NhmConectionType conectionType, BuildTag buildTag)
            {
                var (prefix, _) = GetProtocolPrefixAndPort(conectionType, buildTag);
                var customPort = customEndpointTemplateEntry.Port;

                var customEndpointTemplate = customEndpointTemplateEntry.Template
                    .Replace(PREFIX_TEMPLATE, prefix)
                    .Replace(PORT_TEMPLATE, $":{customPort}");
                return customEndpointTemplate;
            }
        }
        
        public static string NhmSocketAddress => _serviceCustomSettings.NhmSocketAddress;
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
            var algoPoolName = GetPoolAliasForAlgoOrDefault(name.ToLower());
            return (algoPoolName, ok);
        }

        private static (string prefix, int port) GetProtocolPrefixAndPort(NhmConectionType conectionType, BuildTag buildTag)
        {
            int port = (conectionType, buildTag) switch
            {
                (NhmConectionType.STRATUM_SSL, _) => 443,
                (_, BuildTag.TESTNETDEV) => 9300,
                _ => 9200,
            };
            return conectionType switch
            {
                NhmConectionType.STRATUM_TCP => ("stratum+tcp://", port),
                NhmConectionType.STRATUM_SSL => ("stratum+ssl://", port),
                _ => ("", port),
            };
        }

        private static string GetAlgorithmURL(BuildTag buildTag, string prefix, string name, string port)
        {
            return buildTag switch
            {
                BuildTag.TESTNET => $"{prefix}{name}-test.auto.nicehash.com{port}",
                BuildTag.TESTNETDEV => $"{prefix}{name}-dev.auto.nicehash.com{port}",
                _ => $"{prefix}{name}.auto.nicehash.com{port}",
            };
        }

        private static string GetLocationUrlInner(AlgorithmType algorithmType, NhmConectionType conectionType)
        {
            try
            {
                if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED) return _serviceCustomSettings.GetLocationUrl(algorithmType, conectionType, BuildOptions.BUILD_TAG);

                var (name, okName) = GetAlgorithmUrlName(algorithmType);
                // if name is not ok return
                if (!okName) {
                    Logger.Error("StratumServiceHelpers", $"GetLocationUrlInner algorithmType='{(int)algorithmType}' conectionType='{(int)conectionType}'");
                    return "";
                }
                
                var (prefix, port) = GetProtocolPrefixAndPort(conectionType, BuildOptions.BUILD_TAG);
                var ret = GetAlgorithmURL(BuildOptions.BUILD_TAG, prefix, name, $":{port}");
                return ret;
            }
            catch (Exception e)
            {
                Logger.Error("StratumServiceHelpers", $"GetLocationUrlInner algorithmType='{algorithmType}' conectionType='{conectionType}' error: {e.Message}");
                return "";
            }
        }

        // miningLocation is now auto location but keep it for backward compatibility
        public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
        {
            if (algorithmType <= AlgorithmType.NONE) return "";
            var url = GetLocationUrlInner(algorithmType, conectionType);
            if (UseDNSQ && url != "") {
                try
                {
                    var hostT = Task.Run(async () => await DNSQuery.QueryOrDefault(url));
                    hostT.Wait(); // <== BLOCKING
                    var (IP_or_default, gotIP) = hostT.Result;
                    return IP_or_default;
                }
                catch (Exception e)
                {
                    Logger.Error("StratumServiceHelpers", $"GetLocationUrl for url='{url}' error: {e.Message}");
                }
            }
            return url;
        }
        private static readonly Dictionary<string, string> PoolAliasList = new Dictionary<string, string>(){
            {"etchash", "daggerhashimotoetc"},
        };
        public static string GetPoolAliasForAlgoOrDefault(string algoName)
        {
            return PoolAliasList.ContainsKey(algoName) ? PoolAliasList[algoName] : algoName;
        }
    }
}

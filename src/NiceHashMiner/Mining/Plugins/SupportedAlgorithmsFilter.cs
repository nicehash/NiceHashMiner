using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Mining.Plugins
{
    internal static class SupportedAlgorithmsFilter
    {
        static List<List<AlgorithmType>> _filteredAlgorithms = new List<List<AlgorithmType>>();

        const string _internalSettingFilePath = "internals\\SupportedAlgorithmsFilter.json";

        static SupportedAlgorithmsFilter()
        {
            // new platform
#if (TESTNET || TESTNETDEV || PRODUCTION_NEW)
            _filteredAlgorithms.Add(new List<AlgorithmType> { AlgorithmType.MTP });
#else
            // old platform
            _filteredAlgorithms.Add(new List<AlgorithmType> { AlgorithmType.GrinCuckarood29 });
#endif
            var internalSettings = InternalConfigs.ReadFileSettings<List<List<AlgorithmType>>>(_internalSettingFilePath);
            if (internalSettings != null)
            {
                _filteredAlgorithms = internalSettings;
            }
            else
            {
                InternalConfigs.WriteFileSettings(_internalSettingFilePath, _filteredAlgorithms);
            }
        }

        static public bool IsSupported(IEnumerable<AlgorithmType> ids)
        {
            foreach (var filterIds in _filteredAlgorithms)
            {
                if (filterIds.Count != ids.Count()) continue;
                var sameIds = filterIds.Zip(ids, (f, s) => f == s).All(equal => equal);
                if (sameIds) return false;
            }
            return true;
        }
    }
}

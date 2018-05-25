using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Parsing
{
    public class MinerOption
    {
        public string Type;
        public string ShortName;
        public string LongName;
        public string Default;
        public MinerOptionFlagType FlagType;
        public string Separator;

        [JsonConstructor]
        public MinerOption(string iType, string iShortName, string iLongName, string iDefault, MinerOptionFlagType iFlagType,
            string iSeparator = "")
        {
            Type = iType;
            ShortName = iShortName;
            LongName = iLongName;
            Default = iDefault;
            FlagType = iFlagType;
            Separator = iSeparator;
        }

        // Constructor if no short name
        public MinerOption(string iType, string iLongName, string iDefault, MinerOptionFlagType iFlagType,
            string iSeparator = "")
            : this(iType, iLongName, iLongName, iDefault, iFlagType, iSeparator)
        { }
    }
}

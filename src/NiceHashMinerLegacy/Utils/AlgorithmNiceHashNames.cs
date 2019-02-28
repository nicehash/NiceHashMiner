using System;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner
{
    /// <summary>
    /// AlgorithmNiceHashNames class is just a data container for mapping NiceHash JSON API names to algo type
    /// </summary>
    public static class AlgorithmNiceHashNames
    {
        public static readonly string NOT_FOUND = "NameNotFound type not supported";
        public static string GetName(AlgorithmType type)
        {
            return Enum.GetName(typeof(AlgorithmType), type) ?? NOT_FOUND;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using NiceHashMiner.Enums;

namespace NiceHashMiner
{
    /// <summary>
    /// AlgorithmNiceHashNames class is just a data container for mapping NiceHash JSON API names to algo type
    /// </summary>
    public static class AlgorithmNiceHashNames
    {
        public static string GetName(AlgorithmType type) {
            if ((AlgorithmType.INVALID <= type && type <= AlgorithmType.Skunk) || (AlgorithmType.DaggerSia <= type && type <= AlgorithmType.DaggerPascal)) {
                return Enum.GetName(typeof(AlgorithmType), type);
            }
            return "NameNotFound type not supported";
        }
    }
}

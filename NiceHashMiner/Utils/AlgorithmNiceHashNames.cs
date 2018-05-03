using NiceHashMiner.Enums;
using System;

namespace NiceHashMiner
{
    /// <summary>
    /// AlgorithmNiceHashNames class is just a data container for mapping NiceHash JSON API names to algo type
    /// </summary>
    public static class AlgorithmNiceHashNames
    {
        public static string GetName(AlgorithmType type)
        {
            if ((AlgorithmType.INVALID <= type && type <= AlgorithmType.CryptoNightV7) ||
                (AlgorithmType.DaggerKeccak <= type && type <= AlgorithmType.DaggerPascal))
            {
                return Enum.GetName(typeof(AlgorithmType), type);
            }
            return "NameNotFound type not supported";
        }
    }
}

using System;

namespace NiceHashMinerLegacy.Common.Enums
{
    /// <summary>
    /// AlgorithmType enum should/must mirror the values from https://www.nicehash.com/algorithm
    /// </summary>
    public enum AlgorithmType
    {
        INVALID = -2,
        NONE = -1,

        #region NiceHashAPI
        [Obsolete("UNUSED Algorithm")]
        Scrypt = 0,
        [Obsolete("UNUSED Algorithm")]
        SHA256 = 1,
        [Obsolete("UNUSED Algorithm")]
        ScryptNf = 2,
        [Obsolete("UNUSED Algorithm")]
        X11 = 3,
        [Obsolete("UNUSED Algorithm")]
        X13 = 4,
        [Obsolete("UNUSED Algorithm")]
        Keccak = 5,
        [Obsolete("UNUSED Algorithm")]
        X15 = 6,
        [Obsolete("UNUSED Algorithm")]
        Nist5 = 7,
        [Obsolete("UNUSED Algorithm")]
        NeoScrypt = 8,
        [Obsolete("UNUSED Algorithm")]
        Lyra2RE = 9,
        [Obsolete("UNUSED Algorithm")]
        WhirlpoolX = 10,
        [Obsolete("UNUSED Algorithm")]
        Qubit = 11,
        [Obsolete("UNUSED Algorithm")]
        Quark = 12,
        [Obsolete("UNUSED Algorithm")]
        Axiom = 13,
        [Obsolete("UNUSED Algorithm")]
        Lyra2REv2 = 14,
        [Obsolete("UNUSED Algorithm")]
        ScryptJaneNf16 = 15,
        [Obsolete("UNUSED Algorithm")]
        Blake256r8 = 16,
        [Obsolete("UNUSED Algorithm")]
        Blake256r14 = 17,
        [Obsolete("UNUSED Algorithm")]
        Blake256r8vnl = 18,
        [Obsolete("UNUSED Algorithm")]
        Hodl = 19,
        //[Obsolete("UNUSED Algorithm")]
        DaggerHashimoto = 20,
        [Obsolete("UNUSED Algorithm. USED only as second algorithm.")]
        Decred = 21,
        [Obsolete("UNUSED Algorithm")]
        CryptoNight = 22,
        [Obsolete("UNUSED Algorithm")]
        Lbry = 23,
        [Obsolete("UNUSED Algorithm")]
        Equihash = 24,
        [Obsolete("UNUSED Algorithm")]
        Pascal = 25,
        [Obsolete("UNUSED Algorithm")]
        X11Gost = 26,
        [Obsolete("UNUSED Algorithm")]
        Sia = 27,
        [Obsolete("UNUSED Algorithm. USED only as second algorithm.")]
        Blake2s = 28,
        [Obsolete("UNUSED Algorithm")]
        Skunk = 29,
        [Obsolete("UNUSED Algorithm")]
        CryptoNightV7 = 30,
        [Obsolete("UNUSED Algorithm")]
        CryptoNightHeavy = 31,
        //[Obsolete("UNUSED Algorithm")]
        Lyra2Z = 32,
        //[Obsolete("UNUSED Algorithm")]
        X16R = 33,
        [Obsolete("UNUSED Algorithm")]
        CryptoNightV8 = 34,
        [Obsolete("UNUSED Algorithm")]
        SHA256AsicBoost = 35,
        //[Obsolete("UNUSED Algorithm")]
        ZHash = 36,
        //[Obsolete("UNUSED Algorithm")]
        Beam = 37,
        //[Obsolete("UNUSED Algorithm")]
        GrinCuckaroo29 = 38,
        //[Obsolete("UNUSED Algorithm")]
        GrinCuckatoo31 = 39,
        //[Obsolete("UNUSED Algorithm")]
        Lyra2REv3 = 40,
        //[Obsolete("UNUSED Algorithm")]
        MTP = 41,
        //[Obsolete("UNUSED Algorithm")]
        CryptoNightR = 42,
        //[Obsolete("UNUSED Algorithm")]
        CuckooCycle = 43,
        #endregion // NiceHashAPI
    }
}

namespace NiceHashMinerLegacy.Common.Enums
{
    /// <summary>
    /// AlgorithmType enum should/must mirror the values from https://www.nicehash.com/?p=api
    /// Some algorithms are not used anymore on the client, rename them with _UNUSED postfix so we can catch compile time errors if they are used.
    /// </summary>
    public enum AlgorithmType
    {
        // dual algos for grouping
        DaggerKeccak = -8,
        DaggerBlake2s = -7,
        DaggerSia = -6,
        DaggerDecred = -5,
        DaggerLbry = -4,
        DaggerPascal = -3,
        
        INVALID = -2,
        NONE = -1,

        #region NiceHashAPI

        #region Unused

        Scrypt_UNUSED = 0,
        SHA256_UNUSED = 1,
        ScryptNf_UNUSED = 2,
        X11_UNUSED = 3,
        X13_UNUSED = 4,
        //Keccak_UNUSED = 5,
        X15_UNUSED = 6,
        //Nist5_UNUSED = 7,

        WhirlpoolX_UNUSED = 10,
        Qubit_UNUSED = 11,
        Quark_UNUSED = 12,
        Axiom_UNUSED = 13,

        ScryptJaneNf16_UNUSED = 15,
        Blake256r8_UNUSED = 16,
        Blake256r14_UNUSED = 17,
        Blake256r8vnl_UNUSED = 18,
        Hodl = 19,

        #endregion

        Keccak = 5,
        Nist5 = 7,
        NeoScrypt = 8,
        Lyra2RE = 9,
        Lyra2REv2 = 14,
        DaggerHashimoto = 20,
        Decred = 21,
        CryptoNight = 22,
        Lbry = 23,
        Equihash = 24,
        Pascal = 25,
        X11Gost = 26,
        Sia = 27,
        Blake2s = 28,
        Skunk = 29,
        CryptoNightV7 = 30,
        CryptoNightHeavy = 31,
        Lyra2z = 32,
        X16R = 33

        #endregion // NiceHashAPI
    }
}

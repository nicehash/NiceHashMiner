using System;

namespace NHM.Common.Enums
{
    /// <summary>
    /// AlgorithmType enum should/must mirror the values from https://www.nicehash.com/algorithm
    /// </summary>
    public enum AlgorithmType
    {
        INVALID = -2,
        NONE = -1,

        #region NiceHashAPI
        [Obsolete("UNUSED Algorithm", true)]
        Scrypt = 0,
        [Obsolete("UNUSED Algorithm", true)]
        SHA256 = 1,
        [Obsolete("UNUSED Algorithm", true)]
        ScryptNf = 2,
        [Obsolete("UNUSED Algorithm", true)]
        X11 = 3,
        [Obsolete("UNUSED Algorithm", true)]
        X13 = 4,
        [Obsolete("UNUSED Algorithm", true)]
        Keccak = 5,
        [Obsolete("UNUSED Algorithm", true)]
        X15 = 6,
        [Obsolete("UNUSED Algorithm", true)]
        Nist5 = 7,
        [Obsolete("UNUSED Algorithm", true)]
        NeoScrypt = 8,
        [Obsolete("UNUSED Algorithm", true)]
        Lyra2RE = 9,
        [Obsolete("UNUSED Algorithm", true)]
        WhirlpoolX = 10,
        [Obsolete("UNUSED Algorithm", true)]
        Qubit = 11,
        [Obsolete("UNUSED Algorithm", true)]
        Quark = 12,
        [Obsolete("UNUSED Algorithm", true)]
        Axiom = 13,
        [Obsolete("UNUSED Algorithm", true)]
        Lyra2REv2 = 14,
        [Obsolete("UNUSED Algorithm", true)]
        ScryptJaneNf16 = 15,
        [Obsolete("UNUSED Algorithm", true)]
        Blake256r8 = 16,
        [Obsolete("UNUSED Algorithm", true)]
        Blake256r14 = 17,
        [Obsolete("UNUSED Algorithm", true)]
        Blake256r8vnl = 18,
        [Obsolete("UNUSED Algorithm", true)]
        Hodl = 19,
        //[Obsolete("UNUSED Algorithm")]
        DaggerHashimoto = 20,
        [Obsolete("UNUSED Algorithm. USED only as second algorithm.", true)]
        Decred = 21,
        [Obsolete("UNUSED Algorithm", true)]
        CryptoNight = 22,
        [Obsolete("UNUSED Algorithm", true)]
        Lbry = 23,
        [Obsolete("UNUSED Algorithm", true)]
        Equihash = 24,
        [Obsolete("UNUSED Algorithm", true)]
        Pascal = 25,
        [Obsolete("UNUSED Algorithm", true)]
        X11Gost = 26,
        [Obsolete("UNUSED Algorithm")]
        Sia = 27,
        [Obsolete("UNUSED Algorithm. USED only as second algorithm.", true)]
        Blake2s = 28,
        [Obsolete("UNUSED Algorithm", true)]
        Skunk = 29,
        [Obsolete("UNUSED Algorithm", true)]
        CryptoNightV7 = 30,
        [Obsolete("UNUSED Algorithm", true)]
        CryptoNightHeavy = 31,
        [Obsolete("UNUSED Algorithm", true)]
        Lyra2Z = 32,
        [Obsolete("UNUSED Algorithm", true)]
        X16R = 33,
        [Obsolete("UNUSED Algorithm", true)]
        CryptoNightV8 = 34,
        [Obsolete("UNUSED Algorithm", true)]
        SHA256AsicBoost = 35,
        //[Obsolete("UNUSED Algorithm")]
        ZHash = 36,
        [Obsolete("UNUSED Algorithm. Replaced by BeamV2", true)]
        Beam = 37,
        [Obsolete("UNUSED Algorithm", true)]
        GrinCuckaroo29 = 38,
        //[Obsolete("UNUSED Algorithm")]
        GrinCuckatoo31 = 39,
        //[Obsolete("UNUSED Algorithm")]
        Lyra2REv3 = 40,
        [Obsolete("NOT SUPPORTED. UNUSED Algorithm", true)]
        MTP = 41,
        [Obsolete("UNUSED Algorithm", true)]
        CryptoNightR = 42,
        //[Obsolete("UNUSED Algorithm")]
        CuckooCycle = 43,
        [Obsolete("UNUSED Algorithm")]
        GrinCuckarood29 = 44,
        [Obsolete("UNUSED Algorithm. Replaced by BeamV3", true)]
        BeamV2 = 45,
        [Obsolete("UNUSED Algorithm", true)]
        X16Rv2 = 46,
        //[Obsolete("UNUSED Algorithm")]
        RandomXmonero = 47,
        [Obsolete("UNUSED Algorithm", true)]
        Eaglesong = 48,
        [Obsolete("UNUSED Algorithm", true)]
        Cuckaroom = 49,
        //[Obsolete("UNUSED Algorithm")]
        GrinCuckatoo32 = 50,
        [Obsolete("UNUSED Algorithm", true)]
        Handshake = 51,
        //[Obsolete("UNUSED Algorithm")]
        KAWPOW = 52,
        [Obsolete("UNUSED Algorithm")]
        Cuckaroo29BFC = 53,
        //[Obsolete("UNUSED Algorithm")]
        BeamV3 = 54,
        [Obsolete("UNUSED Algorithm")]
        CuckaRooz29 = 55,
        //[Obsolete("UNUSED Algorithm")]
        Octopus = 56,
        #endregion // NiceHashAPI
    }

    public static class AlgorithmTypeExtensionMethods
    {
#pragma warning disable 0618
        public static string GetUnitPerSecond(this AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.ZHash:
                case AlgorithmType.BeamV3:
                    return "Sol/s";
                case AlgorithmType.GrinCuckatoo31:
                case AlgorithmType.GrinCuckatoo32:
                case AlgorithmType.CuckooCycle:
                case AlgorithmType.GrinCuckarood29:
                    return "G/s";
                default:
                    return "H/s";
            }
        }
#pragma warning restore 0618

        public static (string name, bool ok) GetName(this AlgorithmType algorithmType)
        {
            const string NA = "N/A";
            try
            {
                var name = Enum.GetName(typeof(AlgorithmType), algorithmType) ?? NA; // get name or not available
                var ok = name != NA;
                return (name, ok);
            }
            catch
            {
                return (NA, false);
            }
        }

        public static string GetNameFromAlgorithmTypes(this AlgorithmType[] ids)
        {
            var names = new string[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                var (name, _) = ids[i].GetName();
                // TODO should we check if ok?
                names[i] = name;
            }
            return string.Join("+", names);
        }
    }
}

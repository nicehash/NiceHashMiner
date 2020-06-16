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
        [Obsolete("UNUSED Algorithm")]
        Lyra2Z = 32,
        //[Obsolete("UNUSED Algorithm")]
        X16R = 33,
        [Obsolete("UNUSED Algorithm")]
        CryptoNightV8 = 34,
        [Obsolete("UNUSED Algorithm")]
        SHA256AsicBoost = 35,
        //[Obsolete("UNUSED Algorithm")]
        ZHash = 36,
        [Obsolete("UNUSED Algorithm. Replaced by BeamV2")]
        Beam = 37,
        //[Obsolete("UNUSED Algorithm")]
        GrinCuckaroo29 = 38,
        //[Obsolete("UNUSED Algorithm")]
        GrinCuckatoo31 = 39,
        //[Obsolete("UNUSED Algorithm")]
        Lyra2REv3 = 40,
        [Obsolete("NOT SUPPORTED. UNUSED Algorithm")]
        MTP = 41,
        [Obsolete("UNUSED Algorithm")]
        CryptoNightR = 42,
        //[Obsolete("UNUSED Algorithm")]
        CuckooCycle = 43,
        //[Obsolete("UNUSED Algorithm")]
        GrinCuckarood29 = 44,
        //[Obsolete("UNUSED Algorithm")]
        BeamV2 = 45,
        //[Obsolete("UNUSED Algorithm")]
        X16Rv2 = 46,
        //[Obsolete("UNUSED Algorithm")]
        RandomXmonero = 47,
        //[Obsolete("UNUSED Algorithm")]
        Eaglesong = 48,
        //[Obsolete("UNUSED Algorithm")]
        Cuckaroom = 49,
        //[Obsolete("UNUSED Algorithm")]
        GrinCuckatoo32 = 50,
        //[Obsolete("UNUSED Algorithm")]
        Handshake = 51,
        //[Obsolete("UNUSED Algorithm")]
        KAWPOW = 52,
        //[Obsolete("UNUSED Algorithm")]
        Cuckaroo29BFC = 53,
        #endregion // NiceHashAPI
    }

    public static class AlgorithmTypeExtensionMethods
    {
#pragma warning disable 0618
        public static string GetUnitPerSecond(this AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Equihash:
                case AlgorithmType.ZHash:
                case AlgorithmType.Beam:
                case AlgorithmType.BeamV2:
                    return "Sol/s";
                case AlgorithmType.GrinCuckaroo29:
                case AlgorithmType.GrinCuckatoo31:
                case AlgorithmType.GrinCuckatoo32:
                case AlgorithmType.CuckooCycle:
                case AlgorithmType.GrinCuckarood29:
                case AlgorithmType.Cuckaroom:
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

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using NiceHashMiner.Configs;
using NiceHashMiner.Switching;
using NiceHashMiner.Utils.Guid;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner
{
    public static partial class Globals
    {
#if TESTNET
        public static readonly string DemoUser = "2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS";
#elif TESTNETDEV
        public static readonly string DemoUser = "2N2e2ET1jMY9r5is9KaTKnU3bkCFaYHEEEx"; // TODO
#else
        public static readonly string DemoUser = "33hGFJZQAfbdzyHGqhJPvZwncDjUBdZqjW";
#endif

        // change this if TOS changes
        public const int CurrentTosVer = 4;

        // Variables
        public static JsonSerializerSettings JsonSettings = null;

        public static readonly string RigID;
    }
}

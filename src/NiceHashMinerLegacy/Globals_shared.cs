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
        // change this if TOS changes
        public const int CurrentTosVer = 4;

        // Variables
        public static JsonSerializerSettings JsonSettings = null;

        public static readonly string RigID;
    }
}

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using NiceHashMiner.Configs;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;
using System.Net;
using NHM.UUID;

namespace NiceHashMiner
{
    public static class Globals
    {
        // change this if TOS changes
        public const int CurrentTosVer = 4;

        // Variables
        public static JsonSerializerSettings JsonSettings = null;

        static Globals()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;
        }
    }
}

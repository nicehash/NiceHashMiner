using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using NiceHashMiner.Configs;
using NiceHashMiner.Switching;
using NiceHashMiner.Utils.Guid;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;
using System.Net;

namespace NiceHashMiner
{
    public static class Globals
    {
        // change this if TOS changes
        public const int CurrentTosVer = 4;

        // Variables
        public static JsonSerializerSettings JsonSettings = null;

        public static readonly string RigID;

        static Globals()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

            var guid = Helpers.GetMachineGuid();
            if (guid == null)
            {
                // TODO
                RigID = $"{0}-{Guid.NewGuid()}";
                return;
            }

            var uuid = UUID.V5(UUID.Nil().AsGuid(), $"NHML{guid}");
            RigID = $"{0}-{uuid.AsGuid().ToByteArray().ToBase64String()}";
        }
    }
}

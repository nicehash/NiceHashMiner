﻿using NHM.Common;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace NHM.UUID
{
    public static class WindowsMacUtils
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out System.Guid guid);

        private const long RPC_S_OK = 0L;
        private const long RPC_S_UUID_LOCAL_ONLY = 1824L;
        private const long RPC_S_UUID_NO_ADDRESS = 1739L;

        public static string GetMAC_UUID()
        {
            var macUuidFallbackPath = Paths.RootPath("macUuidFallback.txt");
            try
            {
                var fileReadUUID = File.ReadAllText(macUuidFallbackPath);
                if (System.Guid.TryParse(fileReadUUID, out var fileUUID))
                {
                    return fileUUID.ToString();
                }
                Logger.Warn("NHM.UUID", $"Unable to parse fileReadUUID: {fileReadUUID}");
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"WindowsMacUtils.GetMAC_UUID: {e.Message}");
            }
            Logger.Warn("NHM.UUID", $"WindowsMacUtils.GetMAC_UUID FALLBACK");
            var newMacUUID = System.Guid.NewGuid().ToString();
            SaveMacUuidToFile(macUuidFallbackPath, newMacUUID);
            return newMacUUID;
        }

        private static void SaveMacUuidToFile(string path, string uuid)
        {
            try
            {
                //write to macUuidFallbackPath
                File.WriteAllText(path, uuid);
                //log fallback to logs
                var logMacUuidFallbackPath = Paths.RootPath(Path.Combine("logs", "macUuidFallbackHistory.txt"));
                File.AppendAllText(logMacUuidFallbackPath, uuid + Environment.NewLine);
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", e.Message);
            }
        }
    }
}

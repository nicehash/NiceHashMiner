using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
            try
            {
                System.Guid guid;
                UuidCreateSequential(out guid);
                var splitted = guid.ToString().Split('-');
                var last = splitted.LastOrDefault();
                if (last != null) return last;
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"WindowsMacUtils.GetMAC_UUID: {e.Message}");
            }
            Logger.Warn("NHM.UUID", $"WindowsMacUtils.GetMAC_UUID FALLBACK");
            return System.Guid.NewGuid().ToString();
        }
    }
}

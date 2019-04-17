using NiceHashMinerLegacy.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace XmrStak
{
    internal static class TableParser
    {
        public static DeviceType GetDeviceTypeFromInfo(string deviceTypeIdAndThreadId)
        {
            var types = new DeviceType[] { DeviceType.CPU, DeviceType.NVIDIA, DeviceType.AMD };
            const DeviceType ERROR_UNKNOWN = (DeviceType)(-1);
            var splitted = deviceTypeIdAndThreadId.Split('.').ToArray();
            if (splitted.Count() < 1) return ERROR_UNKNOWN;
            var deviceTypeStr = splitted[0];
            foreach (var t in types)
            {
                if (deviceTypeStr.Contains(t.ToString())) return t;
            }
            return ERROR_UNKNOWN;
        }

        public static int GetDeviceConfigIdFromInfo(string deviceTypeIdAndThreadId)
        {
            var splitted = deviceTypeIdAndThreadId.Split('.').ToArray();
            if (splitted.Count() < 2) return -1;
            var deviceIdStrLen = splitted[1].IndexOf("]");
            if (deviceIdStrLen == -1) return -1;
            var deviceIdStr = splitted[1].Substring(0, deviceIdStrLen);
            int deviceId;
            if (int.TryParse(deviceIdStr, out deviceId)) return deviceId;

            return -1;
        }

        public static int GetThreadIdFromInfo(string deviceTypeIdAndThreadId)
        {
            var splitted = deviceTypeIdAndThreadId.Split(':').ToArray();
            if (splitted.Count() < 2) return -1;
            var threadIdStr = splitted[1];
            int threadId;
            if (int.TryParse(threadIdStr, out threadId)) return threadId;
            return -1;
        }

        // [NVIDIA.0]:0 || [AMD.0]:0 || [CPU.0]:0 => [DeviceType.DeviceID]:ThreadID
        private static string ParseDeviceTypeIdAndThreadId(string row)
        {
            var startIndex = row.IndexOf("[");
            var endIndex = row.IndexOf("</th>");
            var hasDeviceTypeIdAndThreadId = startIndex > -1 && endIndex > startIndex;
            if (!hasDeviceTypeIdAndThreadId) return null;
            var info = row.Substring(0, endIndex).Substring(startIndex);
            return info;
        }

        public static IEnumerable<string> ParseTable(string s)
        {
            var startTableIndex = s.IndexOf("<table>");
            var endTableIndex = s.IndexOf("</table>");
            var hasTable = startTableIndex > -1 && endTableIndex > startTableIndex;
            if (!hasTable) return Enumerable.Empty<string>();
            var tableStr = s.Substring(startTableIndex);
            var tableRows = tableStr.Replace("</tr>", "</tr>\n").Split('\n');
            // get info
            var tableDeviceThreadsRows = tableRows
                .Select(row => ParseDeviceTypeIdAndThreadId(row))
                .Where(row => row != null);
            return tableDeviceThreadsRows;
        }
    }
}

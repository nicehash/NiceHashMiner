using System.Linq;

namespace NHM.DeviceDetection.WMI
{
    internal static class PnpDeviceIDHelper
    {
        public static string ToCompareFormat(string input)
        {
            var pciStartIndex = input.IndexOf("PCI");
            if (pciStartIndex == -1) return "";
            var pciStart = input.Substring(pciStartIndex);
            var splittet = pciStart.Split('\\').Where(part => !(string.IsNullOrEmpty(part) || string.IsNullOrWhiteSpace(part)));
            var compareFormat = string.Join("|", splittet).Trim('\\');
            return compareFormat;
        }
    }
}

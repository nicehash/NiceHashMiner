using NHM.Common;
using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LolMiner
{
    internal class DevicesListParser
    {
        private static string[] _keywords = new string[] { "Device", "Name:", "Address:", "Vendor:", "Memory:" };
        private static bool KeepLine(string line) => _keywords.Any(word => line?.Contains(word) ?? false);

        private static int? NumberAfterPattern(string pattern, string line)
        {
            try
            {
                var index = line?.IndexOf(pattern) ?? -1;
                if (index < 0) return null;
                var numericChars = line
                    .Substring(index + pattern.Length)
                    .SkipWhile(c => !char.IsDigit(c))
                    .TakeWhile(char.IsDigit)
                    .ToArray();
                var numberStr = new string(numericChars);
                if (int.TryParse(numberStr, out var number)) return number;
            }
            catch
            { }
            return null;
        }

        private static int[] ChunkToGPU_PCIe_Pair(string[] chunk)
        {
            return _keywords.Zip(chunk, (pattern, line) => (pattern, line))
                .Where((p, index) => index == 0 || index == 2)
                .Select(p => NumberAfterPattern(p.pattern, p.line))
                .Where(num => num.HasValue)
                .Select(num => num.Value)
                .ToArray();
        }

        public static IEnumerable<(string uuid, int minerGpuId)> ParseLolMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            try
            {
                var gpus = baseDevices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .ToArray();

                var mappedDevices = output.Split(new[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(KeepLine)
                    .Select((line, index) => (line, index))
                    .GroupBy(p => p.index / _keywords.Length)
                    .Select(g => g.Select(p => p.line).ToArray())
                    .Select(ChunkToGPU_PCIe_Pair)
                    .Where(nums => nums.Length == 2)
                    .Select(nums => (minerGpuId: nums[0], pcie: nums[1]))
                    .Select(p => (gpu: gpus.FirstOrDefault(gpu => gpu.PCIeBusID == p.pcie), p.minerGpuId))
                    .Where(p => p.gpu != null)
                    .Select(p => (uuid: p.gpu.UUID, p.minerGpuId))
                    .ToArray();
                return mappedDevices;
            }
            catch (Exception e)
            {
                Logger.Error("LolMinerPlugin", $"DevicesListParser error: {e.Message}");
                return Enumerable.Empty<(string uuid, int minerGpuId)>();
            }
        }
    }
}

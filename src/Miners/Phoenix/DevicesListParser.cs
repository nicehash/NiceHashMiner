using NHM.Common;
using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Phoenix
{
    internal class DevicesListParser
    {
        private static string[] _keywords = new string[] { "GPU", "(pcie" };

        private static bool LineContainsGPU_and_PCIe_Pair(string line) => _keywords.All(word => line?.Contains(word) ?? false);

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

        private static int[] LineToGPU_PCIe_Pair(string line)
        {
            return _keywords
                .Select(pattern => NumberAfterPattern(pattern, line))
                .Where(num => num.HasValue)
                .Select(num => num.Value)
                .ToArray();
        }

        internal static IEnumerable<(string uuid, int gpuId)> ParsePhoenixOutput(string output, IEnumerable<BaseDevice> baseDevices)
        {
            try
            {
                var gpus = baseDevices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .ToArray();

                var mappedDevices = output.Split(new[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(LineContainsGPU_and_PCIe_Pair)
                    .Select(LineToGPU_PCIe_Pair)
                    .Where(numPair => numPair.Length == 2)
                    .Select(numPair => (gpuId: numPair[0], pcie: numPair[1]))
                    .Select(p => (gpu: gpus.FirstOrDefault(gpu => gpu.PCIeBusID == p.pcie), gpuId: p.gpuId))
                    .Where(p => p.gpu != null)
                    .Select(p => (uuid: p.gpu.UUID, gpuId: p.gpuId))
                    .ToArray();

                return mappedDevices;
            }
            catch (Exception e)
            {
                Logger.Error("PhoenixPlugin", $"DevicesListParser error: {e.Message}");
                return Enumerable.Empty<(string uuid, int gpuId)>();
            }
        }
    }
}

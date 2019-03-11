using System.Collections.Generic;
using System.Text;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices.Querying.Amd.OpenCL;

namespace NiceHashMiner.Devices.Querying.Amd
{
    internal abstract class AmdDeviceCreation
    {
        protected const string Tag = "AmdDeviceCreation";

        protected abstract bool IsFallback { get; }

        public List<AmdComputeDevice> CreateDevices(int numDevs, List<OpenCLDevice> oclDevs, Dictionary<string, bool> disableAlgos)
        {
            var gpus = CreateGpuDevices(oclDevs, disableAlgos);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("QueryAMD devices: ");

            var computeDevs = new List<AmdComputeDevice>();

            foreach (var gpu in gpus)
            {
                var isDisabledGroup = ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD;
                var skipOrAdd = isDisabledGroup ? "SKIPED" : "ADDED";
                var isDisabledGroupStr = isDisabledGroup ? " (AMD group disabled)" : "";

                stringBuilder.AppendLine($"\t{skipOrAdd} device{isDisabledGroupStr}:");
                stringBuilder.AppendLine($"\t\tID: {gpu.DeviceID}");
                stringBuilder.AppendLine($"\t\tNAME: {gpu.DeviceName}");
                stringBuilder.AppendLine($"\t\tCODE_NAME: {gpu.Codename}");
                stringBuilder.AppendLine($"\t\tUUID: {gpu.Uuid}");
                stringBuilder.AppendLine($"\t\tMEMORY: {gpu.DeviceGlobalMemory}");

                computeDevs.Add(new AmdComputeDevice(gpu, ++numDevs, IsFallback, gpu.Adl2Index));
            }

            Helpers.ConsolePrint(Tag, stringBuilder);

            return computeDevs;
        }

        protected abstract IEnumerable<AmdGpuDevice> CreateGpuDevices(List<OpenCLDevice> oclDevs, Dictionary<string, bool> disableAlgos);
    }
}

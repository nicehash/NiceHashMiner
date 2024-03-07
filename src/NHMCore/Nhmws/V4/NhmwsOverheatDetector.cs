
using NHMCore.Notifications;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Nhmws.V4
{
    internal class NhmwsOverheatDetector
    {
        public class OverheatInfo
        {
            public List<int> temperatures { get; set; } = new List<int>();
            public bool sentNotification { get; set; } = false;
        }
        private readonly int GPU_THR = 75;
        private readonly int VRAM_THR = 105;
        private readonly int CPU_THR = 90;
        private NhmwsOverheatDetector() { }
        private static NhmwsOverheatDetector instance = null;
        public static NhmwsOverheatDetector Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NhmwsOverheatDetector();
                }
                return instance;
            }
        }

        private Dictionary<int, OverheatInfo> GpuTemps = new Dictionary<int, OverheatInfo>();
        private Dictionary<int, OverheatInfo> VRAMTemps = new Dictionary<int, OverheatInfo>();
        private Dictionary<int, OverheatInfo> CPUTemps = new Dictionary<int, OverheatInfo>();
        private void InsertOrUpdateGpuTemp(int key, int value)
        {
            if (GpuTemps.ContainsKey(key))
            {
                GpuTemps[key].temperatures.Add(value);
            }
            else
            {
                GpuTemps[key] = new OverheatInfo() { temperatures = new List<int> { value } };
            }
            if (GpuTemps[key].temperatures.Count > 20)
            {
                GpuTemps[key].temperatures.RemoveAt(0);
            }
        }

        private void InsertOrUpdateVRAMTemp(int key, int value)
        {
            if (VRAMTemps.ContainsKey(key))
            {
                VRAMTemps[key].temperatures.Add(value);
            }
            else
            {
                VRAMTemps[key] = new OverheatInfo() { temperatures = new List<int> { value } };
            }
            if (VRAMTemps[key].temperatures.Count > 20)
            {
                VRAMTemps[key].temperatures.RemoveAt(0);
            }
        }
        private void InsertOrUpdateCPUTemp(int key, int value)
        {
            if (CPUTemps.ContainsKey(key))
            {
                CPUTemps[key].temperatures.Add(value);
            }
            else
            {
                CPUTemps[key] = new OverheatInfo() { temperatures = new List<int> { value } };
            }
            if (CPUTemps[key].temperatures.Count > 20)
            {
                CPUTemps[key].temperatures.RemoveAt(0);
            }
        }
        private bool CheckIfSentForKey(int key)
        {
            if (GpuTemps.ContainsKey(key) && GpuTemps[key].sentNotification)
            {
                return true;
            }
            if (VRAMTemps.ContainsKey(key) && VRAMTemps[key].sentNotification)
            {
                return true;
            }
            return false;
        }

        private bool CheckIfSentForKeyCPU(int key)
        {
            if (CPUTemps.ContainsKey(key) && CPUTemps[key].sentNotification)
            {
                return true;
            }
            return false;
        }

        
        public void UpdateCPUTempsAndWarnIfNeeded(int busID, string name, string b64id, int cpuTemp)
        {
            InsertOrUpdateCPUTemp(busID, cpuTemp);
            bool CPUOverheat = false;
            if (CPUTemps[busID].temperatures.Count > 2)
            {
                int first = CPUTemps[busID].temperatures.Last();
                int second = CPUTemps[busID].temperatures.ElementAt(CPUTemps[busID].temperatures.Count - 2);
                var alreadySent = CheckIfSentForKey(busID);
                CPUOverheat = first > CPU_THR && second > CPU_THR;
                if (CPUOverheat && !alreadySent)
                {
                    NHM.Common.Logger.Warn("OVERHEAT DETECTOR", $"CPU OVERHEAT WARNING {name}");
                    EventManager.Instance.AddEventDeviceOverheating(name, b64id);
                    CPUTemps[busID].sentNotification = true;
                }
            }
            if (CPUTemps[busID].temperatures.Count > 2
                && CPUTemps[busID].temperatures.Count > 2
                && !CPUOverheat
                && CheckIfSentForKey(busID))
            {
                CPUTemps[busID].sentNotification = false;
                NHM.Common.Logger.Warn("OVERHEAT DETECTOR", $"CPU not overheating anymore {name}");
            }
        }
        public void UpdateTempsAndWarnIfNeeded(int busID, string gpu_name, string gpu_id, int gpuTemp, int vramTemp)
        {
            InsertOrUpdateGpuTemp(busID, gpuTemp);
            InsertOrUpdateVRAMTemp(busID, vramTemp);
            bool GPUoverheat = false;
            bool VRAMoverheat = false;
            if (GpuTemps[busID].temperatures.Count > 2)
            {
                int first = GpuTemps[busID].temperatures.Last();
                int second = GpuTemps[busID].temperatures.ElementAt(GpuTemps[busID].temperatures.Count - 2);
                var alreadySent = CheckIfSentForKey(busID);
                GPUoverheat = first > GPU_THR && second > GPU_THR;
                if (GPUoverheat && !alreadySent)
                {
                    NHM.Common.Logger.Warn("OVERHEAT DETECTOR", $"GPU OVERHEAT WARNING {gpu_name}");
                    EventManager.Instance.AddEventDeviceOverheating(gpu_name, gpu_id);
                    GpuTemps[busID].sentNotification = true;
                }
            }
            if (VRAMTemps[busID].temperatures.Count > 2)
            {
                int first = VRAMTemps[busID].temperatures.Last();
                int second = VRAMTemps[busID].temperatures.ElementAt(VRAMTemps[busID].temperatures.Count - 2);
                var alreadySent = CheckIfSentForKey(busID);
                VRAMoverheat = first > VRAM_THR && second > VRAM_THR;
                if (VRAMoverheat && !alreadySent)
                {
                    NHM.Common.Logger.Warn("OVERHEAT DETECTOR", $"VRAM OVERHEAT WARNING {gpu_name}");
                    EventManager.Instance.AddEventDeviceOverheating(gpu_name, gpu_id);
                    VRAMTemps[busID].sentNotification = true;
                }
            }

            if (VRAMTemps[busID].temperatures.Count > 2
                && GpuTemps[busID].temperatures.Count > 2
                && !GPUoverheat
                && !VRAMoverheat
                && CheckIfSentForKey(busID))
            {
                GpuTemps[busID].sentNotification = false;
                VRAMTemps[busID].sentNotification = false;
                NHM.Common.Logger.Warn("OVERHEAT DETECTOR", $"{gpu_name} not overheating anymore ");
            }
        }
    }
}

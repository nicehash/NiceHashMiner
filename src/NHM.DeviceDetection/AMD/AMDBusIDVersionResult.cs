using System;

namespace NHM.DeviceDetection.Models.AMDBusIDVersionResult
{
    internal record AMDBusIDVersionResult
    {
        public string AdrenalinVersion { get; set; } = "unknown";
        public int BUS_ID { get; set; } = -1;
        public int ADLRetCode { get; set; } = -1;
        public int FunctionCall { get; set; } = -1;
        public bool IsIntegrated { get; set; } = false;
    }
}

using ManagedCuda.Nvml;
using System;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal class NvmlException : Exception
    {
        public nvmlReturn ReturnCode { get; private set; }
        string _messagePart = "";

        public NvmlException(string message, nvmlReturn returnCode)
        {
            _messagePart = message;
            ReturnCode = returnCode;
        }

        public override string Message => $"NVML failed with code {ReturnCode}. For '{_messagePart}'";
    }
}

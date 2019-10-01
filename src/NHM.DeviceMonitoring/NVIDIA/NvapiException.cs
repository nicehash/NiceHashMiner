using NVIDIA.NVAPI;
using System;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal class NvapiException : Exception
    {
        public NvStatus ReturnCode { get; private set; }
        string _messagePart = "";

        public NvapiException(string message, NvStatus returnCode)
        {
            _messagePart = message;
            ReturnCode = returnCode;
        }

        public override string Message => $"NVAPI failed with code {ReturnCode}. For '{_messagePart}'";
    }
}

using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakePlugin
{
    [Serializable]
    internal class testSettingsJson
    {
        public Version version { get; set; }
        public string name { get; set; }
        public int exitTimeWaitSeconds { get; set; }
        public List<SupportedAlgorithmsPerType> supportedAlgorithmsPerType { get; set; }
        public List<AlgorithmType> rebenchmarkAlgorithms { get; set; }
    }

    [Serializable]
    internal class SupportedAlgorithmsPerType
    {
        public DeviceType type { get; set; }
        public List<AlgorithmType> algorithms { get; set; }
    }
}

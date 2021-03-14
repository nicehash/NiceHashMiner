using NHM.Common.Enums;
using System;
using System.Collections.Generic;

namespace FakePlugin
{
    [Serializable]
    internal class testSettingsJson
    {
        public Version version { get; set; }
        public string name { get; set; }
        public int exitTimeWaitSeconds { get; set; }
        public List<AlgorithmType> rebenchmarkAlgorithms { get; set; }
    }
}

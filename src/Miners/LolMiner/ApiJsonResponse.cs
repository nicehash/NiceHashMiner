using System;
using System.Collections.Generic;

namespace LolMiner
{
    [Serializable]
    internal class Session
    {
        public int Active_GPUs { get; set; }
        public double Performance_Summary { get; set; }
        public string Performance_Unit { get; set; }
    }

    [Serializable]
    internal class GPU
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public double Performance { get; set; }
    }

    [Serializable]
    internal class ApiJsonResponse
    {
        public Session Session { get; set; }
        public List<GPU> GPUs { get; set; }
    }
}

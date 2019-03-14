using System;
using System.Collections.Generic;
using System.Text;

namespace LolMinerBeam
{
    [Serializable]
    internal class Session
    {
        public int Active_GPUs { get; set; }
        public double Performance_Summary { get; set; }
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

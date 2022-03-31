using System;
using System.Collections.Generic;

namespace LolMiner
{
    [Serializable]
    internal class Algo
    {
        public double Total_Performance { get; set; }
        public string Performance_Unit { get; set; }
        public List<double> Worker_Performance { get; set; }
    }

    [Serializable]
    internal class GPU
    {
        public int Index { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    internal class ApiJsonResponse
    {
        public List<Algo> Algorithms { get; set; }
        public List<GPU> Workers { get; set; }
        public int Num_Workers { get; set; }
    }
}

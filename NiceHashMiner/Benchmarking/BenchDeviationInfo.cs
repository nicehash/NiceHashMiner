using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Benchmarking
{
    internal struct BenchDeviationInfo
    {
        public readonly bool IsDeviant;
        public readonly double Deviation;
        public readonly double NewWeight;

        public BenchDeviationInfo(double deviation, double weight)
        {
            IsDeviant = true;
            Deviation = deviation;
            NewWeight = weight;
        }

        public BenchDeviationInfo(double weight)
        {
            IsDeviant = false;
            Deviation = 0;
            NewWeight = weight;
        }
    }
}

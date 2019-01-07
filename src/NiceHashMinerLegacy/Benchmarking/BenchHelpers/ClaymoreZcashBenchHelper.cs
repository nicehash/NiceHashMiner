using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Benchmarking.BenchHelpers
{
    public class ClaymoreZcashBenchHelper
    {
        private const int MaxBench = 2;
        private readonly string[] _asmModes = { " -asm 1", " -asm 0" };
        private readonly string _originalExtraParams;

        private readonly double[] _speeds = { 0.0d, 0.0d };
        private int _curIndex;

        public int Time = 180;

        public ClaymoreZcashBenchHelper(string oep)
        {
            _originalExtraParams = oep;
        }

        public bool HasTest()
        {
            return _curIndex < MaxBench;
        }

        public void SetSpeed(double speed)
        {
            if (HasTest()) _speeds[_curIndex] = speed;
        }

        public void SetNext()
        {
            _curIndex += 1;
        }

        public string GetTestExtraParams()
        {
            if (HasTest()) return _originalExtraParams + _asmModes[_curIndex];
            return _originalExtraParams;
        }

        private int FastestIndex()
        {
            var maxIndex = 0;
            var maxValue = _speeds[maxIndex];
            for (var i = 1; i < _speeds.Length; ++i)
                if (_speeds[i] > maxValue)
                {
                    maxIndex = i;
                    maxValue = _speeds[i];
                }

            return 0;
        }

        public string GetFastestExtraParams()
        {
            return _originalExtraParams + _asmModes[FastestIndex()];
        }

        public double GetFastestTime()
        {
            return _speeds[FastestIndex()];
        }
    }
}

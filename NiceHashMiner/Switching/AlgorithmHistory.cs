using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Switching
{
    public class AlgorithmHistory : IEnumerable<double>
    {
        private readonly int _maxHistory;

        private readonly List<double> _history = new List<double>();

        public AlgorithmHistory(int maxHistory)
        {
            _maxHistory = maxHistory;
        }

        public void Add(double profit)
        {
            _history.Add(profit);
            if (_history.Count > _maxHistory)
            {
                _history.RemoveAt(0);
            }
        }

        public int CountOverProfit(double profit)
        {
            var count = 0;

            for (var i = _history.Count - 1; i >= 0; i--)
            {
                if (_history[i] > profit)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }

        public IEnumerator<double> GetEnumerator()
        {
            return _history.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

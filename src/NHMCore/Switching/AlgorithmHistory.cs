using System.Collections;
using System.Collections.Generic;

namespace NHMCore.Switching
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a limited history of profitability
    /// </summary>
    public class AlgorithmHistory : IEnumerable<double>
    {
        private readonly int _maxHistory;

        private readonly List<double> _history = new List<double>();

        /// <summary>
        /// Initialize new empty instance
        /// </summary>
        /// <param name="maxHistory">Maximum history to keep</param>
        public AlgorithmHistory(int maxHistory)
        {
            _maxHistory = maxHistory;
        }

        /// <summary>
        /// Append profit and remove oldest if at maximum history
        /// </summary>
        /// <param name="profit"></param>
        public void Add(double profit)
        {
            _history.Add(profit);
            if (_history.Count > _maxHistory)
            {
                _history.RemoveAt(0);
            }
        }

        /// <summary>
        /// Count the number of times the algorithm has been above a profit.
        /// <para/>Count starts from the most recent profit and must be consecutive.
        /// </summary>
        /// <param name="profit">Profit to check if over</param>
        /// <returns>Number of consecutive times over from most recent profit</returns>
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

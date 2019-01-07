using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices
{
    public class PowerOutOfRangeException : ArgumentOutOfRangeException
    {
        /// <summary>
        /// The next closest power limit that can be set
        /// </summary>
        public double ClosestValue;

        public PowerOutOfRangeException(double closest)
        {
            ClosestValue = closest;
        }
    }
}

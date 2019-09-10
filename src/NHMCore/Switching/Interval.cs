using Newtonsoft.Json;
using System;

namespace NHMCore.Switching
{
    /// <summary>
    /// A two point interval
    /// </summary>
    [Serializable]
    public class Interval
    {
        /// <summary>
        /// Inclusive lower end of the interval
        /// </summary>
        public int Lower { get; set; }
        /// <summary>
        /// Inclusive upper end of the interval
        /// </summary>
        public int Upper { get; set; }

        /// <summary>
        /// Upper end as an exclusive interval
        /// </summary>
        [JsonIgnore]
        public int UpperExclusive => Upper + 1;

        public Interval()
        { }

        public Interval(int lower, int upper)
        {
            Lower = lower;
            Upper = upper;
        }

        /// <summary>
        /// Enforce range starts non-negative and is at least 1 long
        /// </summary>
        public void FixRange()
        {
            if (Lower < 0) Lower = 0;
            if (Upper <= Lower) Upper = Lower + 1;
        }

        /// <summary>
        /// Get a random integer inclusive on the range
        /// </summary>
        /// <param name="r">Random object to use</param>
        /// <returns>Random integer</returns>
        public int RandomInt(Random r)
        {
            return r.Next(Lower, UpperExclusive);
        }
    }
}

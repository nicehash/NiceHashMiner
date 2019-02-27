using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Devices.Querying
{
    internal abstract class QueryGpu
    {
        protected static void SortBusIDs(IEnumerable<ComputeDevice> devs)
        {
            var sortedDevs = devs.OrderBy(d => d.BusID).ToList();

            for (var i = 0; i < sortedDevs.Count; i++)
            {
                sortedDevs[i].IDByBus = i;
            }
        }
    }
}

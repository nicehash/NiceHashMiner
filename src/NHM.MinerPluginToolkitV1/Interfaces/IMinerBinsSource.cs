using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1.Interfaces
{
    public interface IMinerBinsSource
    {
        IEnumerable<string> GetMinerBinsUrlsForPlugin();
    }
}

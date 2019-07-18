using System.Collections.Generic;

namespace MinerPluginToolkitV1.Interfaces
{
    public interface IMinerBinsSource
    {
        IEnumerable<string> GetMinerBinsUrlsForPlugin();
    }
}

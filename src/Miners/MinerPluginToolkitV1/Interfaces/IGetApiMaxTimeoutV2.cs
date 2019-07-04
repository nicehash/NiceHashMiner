using MinerPlugin;
using System;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.Interfaces
{
    public interface IGetApiMaxTimeoutV2
    {
        bool IsGetApiMaxTimeoutEnabled { get; }
        TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs);
    }
}

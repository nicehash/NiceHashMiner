using NHM.MinerPlugin;
using System;
using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1.Interfaces
{
    public interface IGetApiMaxTimeoutV2
    {
        bool IsGetApiMaxTimeoutEnabled { get; }
        TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs);
    }
}

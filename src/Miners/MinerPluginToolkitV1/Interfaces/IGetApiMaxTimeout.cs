using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Interfaces
{
    [Obsolete("Use IGetApiMaxTimeoutV2 instead")]
    public interface IGetApiMaxTimeout
    {
        TimeSpan GetApiMaxTimeout();
    }
}

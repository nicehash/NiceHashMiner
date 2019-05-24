using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IBinaryPackageMissingFilesChecker interface is used by plugin to return missing required files and depencencies (*.dll or *.exe files) from miner binary package.
    /// If the return value is not empty, this will indicate that the binary cannot be properly executed.
    /// </summary>
    public interface IBinaryPackageMissingFilesChecker
    {
        IEnumerable<string> CheckBinaryPackageMissingFiles();
    }
}

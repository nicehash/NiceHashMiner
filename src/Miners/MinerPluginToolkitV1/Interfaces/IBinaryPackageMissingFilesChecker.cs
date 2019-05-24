using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// MANDATORY INTERFACE
    /// IBinaryPackageMissingFilesChecker interface is used by plugin to check if all required files needed by miner are existing
    /// For stability purposes this interface is MANDATORY to be implemented by plugin
    /// </summary>
    public interface IBinaryPackageMissingFilesChecker
    {
        IEnumerable<string> CheckBinaryPackageMissingFiles();
    }
}

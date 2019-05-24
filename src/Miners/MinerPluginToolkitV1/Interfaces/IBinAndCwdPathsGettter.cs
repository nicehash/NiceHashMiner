using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IBinAndCwdPathsGettter interface is used by plugins to define their binary and (current) working directory paths
    /// All <see cref="MinerBase"/> plugins must implement this interface. It is used in combination with <see cref="IBinaryPackageMissingFilesChecker"/>.
    /// </summary>
    public interface IBinAndCwdPathsGettter
    {
        Tuple<string, string> GetBinAndCwdPaths();
    }
}

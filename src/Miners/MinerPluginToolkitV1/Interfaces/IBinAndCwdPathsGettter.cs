using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// MANDATORY INTERFACE
    /// IBinAndCwdPathsGettter interface is used by plugins to define their binary and cwd paths
    /// Each plugin should have this interface implemented
    /// </summary>
    public interface IBinAndCwdPathsGettter
    {
        Tuple<string, string> GetBinAndCwdPaths();
    }
}

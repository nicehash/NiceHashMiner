using System.Collections.Generic;
using System.IO;

namespace NHM.MinerPluginToolkitV1
{
    /// <summary>
    /// BinaryPackageMissingFilesCheckerHelpers class is used in combination with IBinaryPackageMissingFilesChecker interface to get missing files if there are any
    /// </summary>
    public static class BinaryPackageMissingFilesCheckerHelpers
    {
        /// <summary>
        /// ReturnMissingFiles returns list of paths where files are missing.
        /// </summary>
        public static List<string> ReturnMissingFiles(string rootPath, IEnumerable<string> filesSubPaths)
        {
            var ret = new List<string>();
            foreach (var filePath in filesSubPaths)
            {
                var fullPath = Path.Combine(rootPath, filePath);
                if (!File.Exists(fullPath))
                {
                    ret.Add(fullPath);
                }
            }
            return ret;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1
{
    public static class BinaryPackageMissingFilesCheckerHelpers
    {
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

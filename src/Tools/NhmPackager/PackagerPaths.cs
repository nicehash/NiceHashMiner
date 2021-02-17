using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NhmPackager
{
    static class PackagerPaths
    {
        private static string[] PrependToPaths(string firstPathPart, params string[] subPaths)
        {
            var paths = new List<string> { firstPathPart };
            paths.AddRange(subPaths);
            return paths.ToArray();
        }

        public static string GetAbsolutePath(params string[] subPaths)
        {
            var ret = Path.Combine(subPaths.ToArray());
            var retAbsolutePath = new Uri(ret).LocalPath;
            return retAbsolutePath;
        }

        private static string _temporaryWorkFolder = null;
        public static void SetTemporaryWorkFolder(string temporaryWorkFolder)
        {
            _temporaryWorkFolder = temporaryWorkFolder;
        }

        private static string ExecutableRootPath() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string GetRootPath(params string[] subPaths) => GetAbsolutePath(PrependToPaths(ExecutableRootPath(), subPaths));

        public static string GetCachedPluginsPath(params string[] subPaths) => GetRootPath(PrependToPaths("cached_plugins", subPaths));

        public static string GetTemporaryWorkFolder(params string[] subPaths)
        {
            if (_temporaryWorkFolder == null) throw new Exception($"TemporaryWorkFolder is null. You must set with '{nameof(SetTemporaryWorkFolder)}'");
            return GetRootPath(PrependToPaths(_temporaryWorkFolder, subPaths));
        }

        public static string GetPluginsPackagesPath(params string[] subPaths) => GetTemporaryWorkFolder(PrependToPaths("plugins_packages", subPaths));

        public static string GetMinerPluginsPath(params string[] subPaths) => GetTemporaryWorkFolder(PrependToPaths("miner_plugins", subPaths));
    }
}

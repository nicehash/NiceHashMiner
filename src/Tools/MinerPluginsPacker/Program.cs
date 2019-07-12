using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MinerPluginLoader;
using System.IO.Compression;
using MinerPluginToolkitV1.Interfaces;

namespace MinerPluginsPacker
{
    class Program
    {
        //private static List<string> _mandatoryParameters = new List<string>
        //{
        //    "-rootDir",
        //    "-type", // r - release || d - debug
        //};
        //var rootPath = "";
        //var type = "";

        //for (int i = 0; i < args.Length - 1; i++)
        //{
        //    var param = args[i];
        //    var value = 
        //}

        // TODO add more options 
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Set miner plugins root path");
                return;
            }
            

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pluginPackagesFolder = Path.Combine(exePath, "plugins_packages");
            var pluginsSearchRoot = args[0];

            // get all managed plugin dll's 
            var dllFiles = Directory.GetFiles(pluginsSearchRoot, "*.dll", SearchOption.AllDirectories)
                .Where(filePath => !filePath.Contains("MinerPlugin") && filePath.Contains("net45") && filePath.Contains("Release") && !filePath.Contains("bin")).ToList();

            var packedPlugins = new HashSet<string>();

            if (Directory.Exists(pluginPackagesFolder))
            {
                Console.WriteLine("Deleting old plugins packages");
                Directory.Delete(pluginPackagesFolder, true);
            }
            if (!Directory.Exists(pluginPackagesFolder))
            {
                Directory.CreateDirectory(pluginPackagesFolder);
            }

            foreach (var filePath in dllFiles)
            {
                var dllDir = Path.GetDirectoryName(filePath);
                var loaded = MinerPluginHost.LoadPlugin(dllDir);
                if (loaded.Count() == 0)
                {
                    // log what we couldn't load and continue
                    Console.WriteLine($"Skipping: {filePath}");
                    continue;
                }
                var newPlugins = MinerPluginHost.MinerPlugin
                    .Where(kvp => packedPlugins.Contains(kvp.Key) == false)
                    .Select(kvp => kvp.Value);

                foreach (var plugin in newPlugins)
                {
                    // TODO workaround to check if it is built with the Toolkit
                    var isToolkitMiner = plugin is IInitInternals;
                    var versionStr = $"v{plugin.Version.Major}.{plugin.Version.Minor}";
                    if (isToolkitMiner)
                    {
                        versionStr = $"{versionStr}_mptoolkitV1";
                    }
                    var pluginZipFileName = $"{plugin.Name}_{versionStr}_{plugin.PluginUUID}.zip";
                    var dllPackageZip = Path.Combine(pluginPackagesFolder, pluginZipFileName);
                    Console.WriteLine($"Packaging: {dllPackageZip}");
                    var fileName = Path.GetFileName(filePath);

                    using (var archive = ZipFile.Open(dllPackageZip, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(filePath, fileName);
                    }

                    packedPlugins.Add(plugin.PluginUUID);
                }
            }
        }
    }
}

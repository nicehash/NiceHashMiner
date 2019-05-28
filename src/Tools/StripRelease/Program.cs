using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StripRelease
{
    class Program
    {
        private static string[] _extensions = { ".pdb", ".xml", ".dll.config", ".exe.config" };
        private static string[] _files = { "MinerDeviceDetection.exe" };
        private static bool IsPackExtension(string filePath)
        {
            foreach (var ext in _extensions)
            {
                if (filePath.EndsWith(ext)) return true;
            }
            return false;
        }
        private static bool IsFilename(string filePath)
        {
            foreach (var name in _files)
            {
                if (filePath.Contains(name)) return true;
            }
            return false;
        }

        static void Main(string[] args)
        {
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var allFiles = Directory.GetFiles(exePath, "*.*", SearchOption.AllDirectories);
            var startWithStr = Path.Combine(exePath, "Release");
            var filesToDelete = allFiles
                .Where(s => s.StartsWith(startWithStr) && (IsPackExtension(s) || IsFilename(s)))
                .ToList();
            var gotException = false;
            foreach(var file in filesToDelete)
            {
                //Console.WriteLine(file);
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error while deleting file {file}: {e.Message}");
                    gotException = true;
                }
            }
            if (gotException) Console.ReadKey();
        }
    }
}

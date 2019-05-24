using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CreateLogReport
{
    class Program
    {

        private static string[] _extensions = { ".txt", ".log", ".json" };
        private static bool IsPackExtension(string filePath)
        {
            foreach (var ext in _extensions)
            {
                if (filePath.EndsWith(ext)) return true;
            }
            return false;
        }

        private static void Run_device_detection_test_Bat()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = true,
                    FileName = @"device_detection_test.bat",
                    CreateNoWindow = true
                };
                using (var p = new Process { StartInfo = startInfo })
                {
                    p.Start();
                    p.WaitForExit(30 *1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Run_device_detection_test_Bat error: {e.Message}");
            }
        }

        static void Main(string[] args)
        {
            // TODO
            // RUN device_detection_test.bat
            Console.WriteLine($"Running device_detection_test.bat...");
            Run_device_detection_test_Bat();

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filesToPack = Directory.GetFiles(exePath, "*.*", SearchOption.AllDirectories).Where(s => IsPackExtension(s)).ToList();
            //Console.WriteLine(filesToPack.Count);
            string archiveFileName = "_archive_logs.zip";
            Console.Write($"Preparing logs archive file '{archiveFileName}'...");

            double max = filesToPack.Count;
            double step = 0;
            using (var progress = new ProgressBar())
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var filePath in filesToPack)
                    {
                        step += 1;
                        var entryPath = filePath.Replace(exePath, "");
                        entryPath = entryPath.Substring(1);
                        var zipFile = archive.CreateEntry(entryPath);
                        var fileRawBytes = File.ReadAllBytes(filePath);
                        using (var entryStream = zipFile.Open())
                        using (var b = new BinaryWriter(entryStream))
                        {
                            b.Write(fileRawBytes);
                        }
                        progress.Report(step / max);
                    }
                }

                using (var fileStream = new FileStream(archiveFileName, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
            Console.WriteLine("Done.");
            Console.WriteLine($"Packed file: '{archiveFileName}'");
            Console.WriteLine("You can close this window. Press any key to close.");
            Console.ReadKey();
        }
    }
}

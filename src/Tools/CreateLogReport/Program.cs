using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

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

        private static void Run_device_detection_Bat(string appRoot)
        {
            try
            {
                var appRootFull = new Uri(Path.Combine(Environment.CurrentDirectory, appRoot)).LocalPath;
                var appRootFullExe = Path.Combine(appRootFull, "device_detection.bat");
                Console.WriteLine(appRootFull);
                Console.WriteLine(appRootFullExe);
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = true,
                    FileName = appRootFullExe,
                    WorkingDirectory = appRootFull,
                    CreateNoWindow = true
                };
                using (var p = new Process { StartInfo = startInfo })
                {
                    p.Start();
                    p.WaitForExit(30 * 1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Run_device_detection_Bat error: {e.Message}");
            }
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            catch
            { }

            //file is not locked
            return false;
        }

        static void Main(string[] args)
        {
            // TODO
            // RUN device_detection.bat
            Console.WriteLine($"Running device_detection.bat...");
            var appRoot = args.Length > 0 ? args[0] : "";
            Run_device_detection_Bat(appRoot);

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filesToPack = Directory.GetFiles(exePath, "*.*", SearchOption.AllDirectories)
                .Where(s => IsPackExtension(s))
                .ToList();

            //Console.WriteLine(filesToPack.Count);
            string archiveFileName = "tmp._archive_logs.zip";
            Console.Write($"Preparing logs archive file '{archiveFileName}'...");

            const string tmpLocked = "tmpLocked";
            double max = filesToPack.Count;
            double step = 0;
            using (var progress = new ProgressBar())
            using (var fileStream = new FileStream(archiveFileName, FileMode.Create))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                foreach (var filePath in filesToPack)
                {
                    step += 1;
                    var entryPath = filePath.Replace(exePath, "").Substring(1);
                    var zipFile = archive.CreateEntry(entryPath);
                    var lockedFile = IsFileLocked(filePath);
                    if (lockedFile) File.Copy(filePath, tmpLocked, true);
                    var openFilePath = lockedFile ? tmpLocked : filePath;
                    using (var readFile = File.Open(openFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var entryStream = zipFile.Open())
                    {
                        readFile.CopyTo(entryStream);
                    }
                    if (lockedFile) File.Delete(tmpLocked);
                    progress.Report(step / max);
                }
            }
            Console.WriteLine("Done.");
            Console.WriteLine($"Packed file: '{archiveFileName}'");
            Console.WriteLine("You can close this window. Press any key to close.");
            if (args.Length == 0) Console.ReadKey();
        }
    }
}

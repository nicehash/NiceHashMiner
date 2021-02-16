using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace runnhmasadmin
{
    class Program
    {
        private static void TryKillPid(string pid)
        {
            try
            {
                var p = Process.GetProcessById(Int32.Parse(pid));
                if (p != null) p.Kill();
            }
            catch (Exception)
            {
            }
        }
        private static void RunProgram(string path)
        {
            try
            {
                using (var p = Process.Start(path))
                {
                }
            }
            catch (Exception)
            {
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var programPath = Directory.GetFiles(rootPath).Where(file => file.Contains("NiceHashMiner.exe")).FirstOrDefault();
                RunProgram(programPath);
            }
            else
            {
                TryKillPid(args[0]);
                RunProgram(args[1]);
            }
        }
    }
}

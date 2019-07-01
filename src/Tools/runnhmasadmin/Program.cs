using System;
using System.Diagnostics;

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
            TryKillPid(args[0]);
            RunProgram(args[1]);
        }
    }
}

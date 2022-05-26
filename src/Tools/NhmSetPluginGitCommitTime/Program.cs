using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NhmSetPluginGitCommitTime
{
    class Program
    {
        private static string GetGitCommitHash(string pluginProjectPath)
        {
            using var getGitHash = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"rev-list --all {pluginProjectPath}\\**",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            var ok = getGitHash.Start();
            if (!getGitHash.StandardOutput.EndOfStream)
                return getGitHash.StandardOutput.ReadLine();
            return null;
        }

        private static string GetLastCommitDateTime(string commitHash)
        {
            using var getGitCommitDateTime = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"show {commitHash}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            getGitCommitDateTime.Start();

            while (!getGitCommitDateTime.StandardOutput.EndOfStream)
            {
                var line = getGitCommitDateTime.StandardOutput.ReadLine();
                if (line.StartsWith("Date:")) return line.Replace("Date:", "").Trim();
            }
            return null;
        }

        static Task Main(string[] args)
        {
            var arguments = args.FirstOrDefault().Split(' ');


            var projectPath = arguments[0];
            var dllPath = arguments[1];

            var commitHash = GetGitCommitHash(projectPath);
            if (commitHash == null) return Task.CompletedTask;

            var dateTimeStr = GetLastCommitDateTime(commitHash);
            if (dateTimeStr == null) return Task.CompletedTask;

            DateTimeOffset.TryParseExact(dateTimeStr, "ddd MMM d HH:mm:ss yyyy K",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateTimeOffset);

            try
            {
                File.SetLastWriteTime(dllPath, dateTimeOffset.DateTime);
            }
            catch (Exception e)
            {
                Console.WriteLine("SetLastWriteTime failed: {0}", e);
            }

            return Task.CompletedTask;
        }
    }
}

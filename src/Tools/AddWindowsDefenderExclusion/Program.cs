using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AddWindowsDefenderExclusion
{
    class Program
    {
        static void SetWindowsDefenderExclusion(string ruleArgument)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = ruleArgument,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using (var setRule = Process.Start(startInfo))
            {
                setRule.WaitForExit();
                Environment.ExitCode = setRule.ExitCode;
            }
        }

        static void AddExclusion(string programFullPath)
        {
            var escapedPath = programFullPath.Contains('"') ? programFullPath : $"'{programFullPath}'";
            SetWindowsDefenderExclusion($"Add-MpPreference -ExclusionPath {escapedPath}");
        }

        static void RemoveExclusion(string programFullPath)
        {
            var escapedPath = programFullPath.Contains('"') ? programFullPath : $"'{programFullPath}'";
            SetWindowsDefenderExclusion($"Remove-MpPreference -ExclusionPath {escapedPath}");
        }

        static void Main(string[] args)
        {
            if (args.Count() < 2)
            {
                Console.WriteLine("Error usage:*.exe [add|rem] directoryPath");
                return;
            }

            var operation = args[0];
            var isValidOperation = operation == "add" || operation == "rem";
            if (!isValidOperation)
            {
                Console.WriteLine("Invalid operation! Use add or rem\nError usage:*.exe [add|rem] directoryPath");
                return;
            }

            var directoryPath = args[1];
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Invalid directory path! Use add or rem\nError usage:*.exe [add|rem] directoryPath");
                return;
            }

            if (operation == "add")
            {
                AddExclusion(directoryPath);
            }
            if (operation == "rem")
            {
                RemoveExclusion(directoryPath);
            }
        }
    }
}

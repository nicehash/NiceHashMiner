using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace AssemblyInfoVersionManager
{
    class AssemblyInfoVersionManagerTool
    {
        static void DisplayHelp()
        {
            var helpStr = "Help:\n";
            helpStr += "\t--help, -h          -> HELP (this text)\n";
            helpStr += "\t--next, -n          -> Increments next revision version\n";
            helpStr += "\t--set, -s <VERSION> -> Custom version in format \"INT.INT.INT.INT\" without brackets\n";
            Console.WriteLine(helpStr);
        }
        static (bool ok, string[] files) ScanFiles(string path, string pattern, Regex versionRegex)
        {
            List<string> fileArray = new();
            var rootPath = String.Join("\\", path.Split("\\").SkipLast(2));
            string[] files = Directory.GetFiles(rootPath, pattern, SearchOption.AllDirectories);
            foreach(string file in files)
            {
                var ret = GetVersionFromFile(file, versionRegex);
                if (ret.ok) fileArray.Add(file);
            }
            Console.WriteLine($"Total file count found: {files.Length}");
            return (fileArray.Count != 0, fileArray.ToArray());
        }
        static (bool ok, Version version) GetVersionFromFile(string file,Regex versionRule)
        {
            var text = File.ReadAllText(file);
            var match = versionRule.Matches(text);
            if (match.Count <= 0) return (false, new Version());
            List<Version> foundVersions = new();
            foreach (Match m in match)
            {
                if (!m.Success) return (false, new Version());
                foundVersions.Add(new Version(m.Value));
            }
            if (foundVersions.Count <= 0) return (false, new Version());
            bool allEqual = foundVersions.GroupBy(o => o).Count() == 1;
            if (!allEqual) return (false, new Version());
            return (true, foundVersions[0]);
        }

        static bool SetVersion(string file, Regex versionRule, Version next)
        {
            var text = File.ReadAllText(file);
            var newText = Regex.Replace(text, versionRule.ToString(), next.ToString());
            if (newText == null) return false;
            File.WriteAllText(file, newText);
            return true;
        }

        static bool GetCurrentVersionAndIncrement(string[] files, Regex versionRule)
        {
            var versions = new List<Version>();
            try
            {
                foreach (var file in files)
                {
                    var ret = GetVersionFromFile(file, versionRule);
                    if (!ret.ok)
                    {
                        Console.WriteLine($"Error, versions not the same in: {file}");
                        return false;
                    }
                    versions.Add(ret.version);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error, something went wrong while reading files, exiting");
                Console.WriteLine(e.Message);
                return false;
            }
            bool allEqual = versions.GroupBy(o => o).Count() == 1;
            if (!allEqual || versions.Count <= 0)
            {
                Console.WriteLine($"Error, no version files found or versions not equal, try with -s flag");
                return false;
            }
            var nextVersion = new Version(versions[0].Major, versions[0].Minor, versions[0].Build, versions[0].Revision + 1);
            try
            {
                foreach (var file in files)
                {
                    SetVersion(file, versionRule, nextVersion);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error, something went wrong while writing changes");
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        static void Main(string[] args)
        {
            string precisePattern = @"[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,2}";
            var versionRule = new Regex(precisePattern);
            Console.WriteLine("Assembly info version manager tool");
            string currentDir = Directory.GetCurrentDirectory();

            if (!currentDir.EndsWith("\\installer\\version_manager"))
            {
                Console.WriteLine("Error, this .exe must be inside ...NHM_ROOT_FOLDER...\\installer\\version_manager to function properly");
                return;
            }
            if (args.Length == 0)
            {
                DisplayHelp();
                return;
            }
            if (args[0] == "--help" || args[0] == "-h")
            {
                DisplayHelp();
                return;
            }
            if (args[0] == "--next" || args[0] == "-n")
            {
                var foundFiles1 = ScanFiles(currentDir, "AssemblyInfo.cs", versionRule);
                var foundFiles2 = ScanFiles(currentDir, "NiceHashMiner.csproj", versionRule);
                if (!foundFiles1.ok && !foundFiles2.ok)
                {
                    Console.WriteLine("Error, something went wrong while reading files");
                    return;
                }
                var ret = GetCurrentVersionAndIncrement(foundFiles1.files, versionRule) &&
                    GetCurrentVersionAndIncrement(foundFiles2.files, versionRule);
                if (!ret)
                {
                    Console.WriteLine($"Error, failed to increment versions");
                    return;
                }
                Console.WriteLine("Successfully set versions to next revision");
                return;
            }
            else if ((args[0] == "--set" || args[0] == "-s") && args.Length == 2)
            {
                try
                {
                    var foundFiles1 = ScanFiles(currentDir, "AssemblyInfo.cs", versionRule);
                    var foundFiles2 = ScanFiles(currentDir, "NiceHashMiner.csproj", versionRule);
                    if (!foundFiles1.ok && !foundFiles2.ok)
                    {
                        Console.WriteLine("Error, something went wrong while reading files");
                        return;
                    }
                    Version setVersion = new Version(args[1]);
                    foreach(var file in foundFiles1.files)
                    {
                        var ret = SetVersion(file, versionRule, setVersion);
                        if (!ret)
                        {
                            Console.WriteLine($"Error, cant set version in {file}");
                            return;
                        }
                    }
                    foreach(var file in foundFiles2.files)
                    {
                        var ret = SetVersion(file, versionRule, setVersion);
                        if (!ret)
                        {
                            Console.WriteLine($"Error, cant set version in {file}");
                            return;
                        }
                    }
                    Console.WriteLine("Successfully set versions");
                    return;
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error, {e.Message}");
                }
            }
            else
            {
                Console.WriteLine("Error, unknown commands!");
                DisplayHelp();
            }
            Console.WriteLine("Process ended");
        }
    }
}


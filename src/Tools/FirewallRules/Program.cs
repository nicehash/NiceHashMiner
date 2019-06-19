using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FirewallRules
{
    class Program
    {
        static Dictionary<string, string> DirSearch(string dir)
        {
            var miners = new Dictionary<string, string>();

            try
            {
                foreach (string f in Directory.GetFiles(dir, "*.exe", SearchOption.AllDirectories))
                {
                    miners.Add(f, Path.GetFileNameWithoutExtension(f));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"DirSearch error: {e.Message}!");
            }
            return miners;
        }

        static void SetFirewallRule(string ruleArgument)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh.exe",
                Arguments = ruleArgument,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using (var setRule = Process.Start(startInfo))
            {
                setRule.WaitForExit();
            }
        }

        static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        static string GetHashedName(string programFullPath, string name)
        {
            var hashed = CalculateMD5Hash($"{name}_{programFullPath}");
            var validName = String.Concat(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c))); 
            return $"{validName}_{hashed}";
        }

        static void AllowFirewallRule(string programFullPath, string name)
        {
            var escapedPath = programFullPath.Contains(' ') ? $"\"{programFullPath}\"" : programFullPath;
            SetFirewallRule($"advfirewall firewall add rule name=nhm_{GetHashedName(programFullPath, name)} program={escapedPath} protocol=tcp dir=in enable=yes action=allow");
        }

        static void RemoveFirewallRule(string programFullPath, string name)
        {
            SetFirewallRule($"advfirewall firewall delete rule name=nhm_{GetHashedName(programFullPath, name)}");
        }

        static void Main(string[] args)
        {
            if (args.Count() < 3)
            {
                Console.WriteLine("Error usage:*.exe rootPath [add|rem] relativePaths...");
                return;
            }
            var rootPath = args[0];
            if (!Directory.Exists(rootPath))
            {
                Console.WriteLine($"Directory {rootPath} doesn't exist!");
                return;
            }

            var operation = args[1];
            var isValidOperation = operation == "add" || operation == "rem" || operation == "update";
            if (!isValidOperation)
            {
                Console.WriteLine("Invalid operation! Use add or rem\nError usage:*.exe rootPath [add|rem|update] relativePaths...");
                return;
            }

            var relativePaths = new List<string>();
            for (int i=2; i<args.Length; i++)
            {
                relativePaths.Add(args[i]);
            }

            Dictionary<string, string> miners = new Dictionary<string, string>();

            foreach (string binPath in relativePaths)
            {
                var tmpBins = DirSearch(Path.Combine(rootPath, binPath));
                foreach(var kmp in tmpBins)
                {
                    miners.Add(kmp.Key, kmp.Value);
                }
            }

            if (operation == "add")
            {
                foreach (var miner in miners)
                {
                    AllowFirewallRule(miner.Key, miner.Value);
                }
            }
            if (operation == "rem")
            {
                foreach (var miner in miners)
                {
                    RemoveFirewallRule(miner.Key, miner.Value);
                }
            }
            if (operation == "update")
            {
                foreach (var miner in miners)
                {
                    RemoveFirewallRule(miner.Key, miner.Value);
                }
                foreach (var miner in miners)
                {
                    AllowFirewallRule(miner.Key, miner.Value);
                }
            }
        }
    }
}

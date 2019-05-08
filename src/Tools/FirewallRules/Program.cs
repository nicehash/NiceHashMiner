using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            }
            return miners;
        }

        static void SetFirewallRule(string ruleArgument)
        {
            Process setRule = new Process();
            setRule.StartInfo.FileName = "netsh.exe";
            setRule.StartInfo.Arguments = ruleArgument;
            setRule.StartInfo.CreateNoWindow = true;
            setRule.StartInfo.UseShellExecute = false;
            setRule.Start();
            setRule.WaitForExit();
        }

        static void AllowFirewallRule(string programFullPath, string name)
        {
            SetFirewallRule($"advfirewall firewall add rule name={name} program={programFullPath} protocol=tcp dir=in enable=yes action=allow");
        }

        static void RemoveFirewallRule(string name)
        {
            SetFirewallRule($"advfirewall firewall delete rule name={name}");
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
                    RemoveFirewallRule(miner.Value);
                }
            }
            if (operation == "update")
            {
                foreach (var miner in miners)
                {
                    RemoveFirewallRule(miner.Value);
                    AllowFirewallRule(miner.Key, miner.Value);
                }
            }
        }
    }
}

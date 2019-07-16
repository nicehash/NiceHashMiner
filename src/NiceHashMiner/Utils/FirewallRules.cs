using NHM.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MinerPluginToolkitV1.Configs;
using NiceHashMiner.Mining.Plugins;

namespace NiceHashMiner.Utils
{
    public static class FirewallRules
    {
        public static bool RunFirewallRulesOnStartup { get; set; } = false;
        static string _firewallRulesAddedFilePath => Path.Combine("internals", "firewall_rules_added.json");
        private static List<string> _pluginsUUIDsWithVersions = new List<string>();

        static FirewallRules()
        {
            var lastSaved = InternalConfigs.ReadFileSettings<List<string>>(_firewallRulesAddedFilePath);
            if (lastSaved != null) _pluginsUUIDsWithVersions = lastSaved;
        }

        public static bool IsFirewallRulesOutdated()
        {
            var installedPlugins = MinerPluginsManager.GetPluginUUIDsAndVersionsList();
            return installedPlugins.Except(_pluginsUUIDsWithVersions).Count() > 0;
        }

        public static void UpdateFirewallRules()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = @"FirewallRules.exe",
                    Arguments = $"{Directory.GetCurrentDirectory()} update miner_plugins",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                startInfo.WindowStyle = ProcessWindowStyle.Hidden; // used for hidden window
                using (var setFirewallRulesProcess = new Process { StartInfo = startInfo })
                {
                    setFirewallRulesProcess.Start();
                    setFirewallRulesProcess?.WaitForExit(10 * 1000);
                    if (setFirewallRulesProcess?.ExitCode != 0)
                    {
                        Logger.Info("NICEHASH", "setFirewallRulesProcess returned error code: " + setFirewallRulesProcess.ExitCode);
                    }
                    else
                    {
                        Logger.Info("NICEHASH", "setFirewallRulesProcess all OK");
                        var installedPlugins = MinerPluginsManager.GetPluginUUIDsAndVersionsList();
                        _pluginsUUIDsWithVersions = installedPlugins;
                        InternalConfigs.WriteFileSettings(_firewallRulesAddedFilePath, _pluginsUUIDsWithVersions);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NICEHASH", $"SetFirewallRules error: {ex.Message}");
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using NHM.Common;
using NHM.Common.Enums;

namespace Excavator
{
    internal static class CmdConfig
    {
        class Command
        {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("method")]
            public string Method { get; set; }
            [JsonProperty("params")]
            public List<string> Params { get; set; }
        }

        class CommandList
        {
            [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
            public uint? Time { get; set; } = null;
            [JsonProperty("loop", NullValueHandling = NullValueHandling.Ignore)]
            public uint? Loop { get; set; } = null;
            [JsonProperty("event", NullValueHandling = NullValueHandling.Ignore)]
            public string Event { get; set; } = null;
            [JsonProperty("commands")]
            public List<Command> Commands { get; set; } = new List<Command>();
        }

        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Culture = CultureInfo.InvariantCulture,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public static string CreateTemplate(IEnumerable<int> gpuUuids, string algorithmName)
        {
            return CreateDefaultTemplateAndCreateCMD("__SUBSCRIBE_PARAM_LOCATION__", "__SUBSCRIBE_PARAM_USERNAME__", gpuUuids, algorithmName);
        }

        public static string CommandFileTemplatePath(string pluginUUID, string binPath, string fileName)
        {
            var path = Paths.MinerPluginsPath(pluginUUID, binPath, fileName);
            return path;
        }

        private static List<Command> CreateInitialCommands(string subscribeLocation, string subscribeUsername, IEnumerable<int> excavatorIds, string algorithmName)
        {
            var initialCommands = new List<Command>
                {
                    new Command { Id = 1, Method = "subscribe", Params = new List<string>{ subscribeLocation, subscribeUsername } },
                    new Command { Id = 2, Method = "algorithm.add", Params = new List<string>{ algorithmName.ToLower() } },
                };
            return initialCommands;
        }
        private static List<Command> CreateExtraCommands(IEnumerable<int> excavatorIds, string algorithmName, List<Command> mandatoryCMDS = null)
        {
            var initialCommands = new List<Command>();
            if (algorithmName == "randomx")
            {
                initialCommands.AddRange(excavatorIds.Select((dev, index) => new Command { Id = index + 3, Method = "worker.add", Params = new List<string> { algorithmName, dev.ToString(), "NTHREADS=0", "HIGHPRIORITY=0", "USELARGEPAGE=1", "USEMSR=1" } }));
                if (mandatoryCMDS != null)
                {
                    foreach (var c in initialCommands)
                    {
                        var crossRef = mandatoryCMDS.FirstOrDefault(m => m.Id == c.Id);
                        if (crossRef == null) continue;
                        c.Params = crossRef.Params;
                    }
                }

            }
            else initialCommands.AddRange(excavatorIds.Select((dev, index) => new Command { Id = index + 3, Method = "worker.add", Params = new List<string> { algorithmName.ToLower(), dev.ToString() } }));
            return initialCommands;
        }

        private static string CreateDefaultTemplateAndCreateCMD(string subscribeLocation, string subscribeUsername, IEnumerable<int> excavatorIds, string algorithmName)
        {
            try
            {
                var commandListTemplate = new List<CommandList>
                {
                    new CommandList
                    {
                        Time = 0,
                        Commands = CreateInitialCommands(subscribeLocation, subscribeUsername, excavatorIds, algorithmName),
                    },
                    new CommandList
                    {
                        Time = 1,
                        Commands = CreateExtraCommands(excavatorIds, algorithmName),
                    },
                    new CommandList
                    {
                        Event = "on_quit",
                        Commands = new List<Command>{ },
                    }
                };
                return JsonConvert.SerializeObject(commandListTemplate, Formatting.Indented);
            }
            catch (Exception e)
            {
                Logger.Error("Excavator.CmdConfig", $"CreateCommandFile error {e.Message}");
                return null;
            }
        }
        private static string[] _invalidTemplateMethods = new string[] { "subscribe", "algorithm.add" };
        private static string ParseTemplateFileAndCreateCMD(string templateFilePath, IEnumerable<int> excavatorIds, string subscribeLocation, string subscribeUsername, string algorithmName)
        {
           
            if (!File.Exists(templateFilePath)) return null;
            try
            {
                var template = JsonConvert.DeserializeObject<List<CommandList>>(File.ReadAllText(templateFilePath), _jsonSettings);
                var validCmds = template
                    .Where(cmd => cmd.Commands.All(c => !_invalidTemplateMethods.Contains(c.Method)))
                    .Select(cmd => (cmd, commands: cmd.Commands.ToList()))
                    .Where(p => p.commands.Any())
                    .ToArray();

                var otherCmds = template
                    .Where(cmd => cmd.Commands.All(c => _invalidTemplateMethods.Contains(c.Method)))
                    .Select(cmd => (cmd, commands: cmd.Commands.ToList()))
                    .Where(p => p.commands.Any())
                    .ToArray();

                foreach (var (cmd, commands) in validCmds)
                {
                    cmd.Commands = commands;
                    foreach(var c in cmd.Commands)
                    {
                        if(c.Method == "worker.add")
                        {
                            if (c.Params.Count >= 2 && c.Params[0].ToLower() != "randomx")
                            {
                                c.Params = new List<string> { algorithmName.ToLower(), c.Params[1] };
                            }
                        }
                    }
                }
                var commandListTemplate = new List<CommandList>
                {
                    new CommandList
                    {
                        Time = 0,
                        Commands = CreateInitialCommands(subscribeLocation, subscribeUsername, excavatorIds, algorithmName),
                    },
                };
                if (validCmds.Any())
                {
                    commandListTemplate.AddRange(validCmds.Select(p => p.cmd));
                }
                return JsonConvert.SerializeObject(commandListTemplate, Formatting.Indented, _jsonSettings);
            }
            catch (Exception e)
            {
                Logger.Error("Excavator.CmdConfig", $"ParseTemplateFile error {e.Message}");
                return null;
            }
        }

        private static string CreateCommandWithTemplate(string subscribeLocation, string subscribeUsername, IEnumerable<int> excavatorIds, string templateFilePath, string algorithmName)
        {
            var template = ParseTemplateFileAndCreateCMD(templateFilePath, excavatorIds, subscribeLocation, subscribeUsername, algorithmName);
            if (template == null)
            {
                Logger.Warn("Excavator.CmdConfig", "Template file not found, using default!");
                template = CreateDefaultTemplateAndCreateCMD(subscribeLocation, subscribeUsername, excavatorIds, algorithmName);
            }
            return template;
        }
        private static string GetServiceLocation(string miningLocation)
        { 
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return $"nhmp-test.auto.nicehash.com:443";
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return $"nhmp-dev.auto.nicehash.com:443";
            //BuildTag.PRODUCTION
            return $"nhmp.auto.nicehash.com:443";
        }

        public static string CmdJSONString(string pluginUUID, string _miningLocation, string username, string algorithmName, string fileName, string binPath, params int[] excavatorIds) {
            var miningLocation = GetMiningLocation(_miningLocation);
            var templatePath = CommandFileTemplatePath(pluginUUID, binPath, fileName);
            var miningServiceLocation = GetServiceLocation(miningLocation);
            var command = CreateCommandWithTemplate(miningServiceLocation, username, excavatorIds, templatePath, algorithmName);
            if (command == null) Logger.Error("Excavator.CmdConfig", "command is NULL");
            return command;
        }

        private static string GetMiningLocation(string location)
        {
            // new mining locations new clients
            if (location.StartsWith("eu") || location.StartsWith("usa")) return location;
            // old mining locations old clients with obsolete locations fallback to usa
            return "usa";
        }
    }
}

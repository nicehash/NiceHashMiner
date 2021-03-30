using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using NHM.Common;

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

        class Cmd
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

        public static string CreateTemplate(IEnumerable<string> gpuUuids)
        {
            return CreateCommandFile("__SUBSCRIBE_PARAM_LOCATION__", "__SUBSCRIBE_PARAM_USERNAME__", gpuUuids);
        }

        public static string CommandFileTemplatePath(string pluginUUID)
        {
            return Paths.MinerPluginsPath(pluginUUID, "internals", "CommandLineTemplate.json");
        }

        public static string CreateCommandFile(string subscribeLocation, string subscribeUsername, IEnumerable<string> gpuUuids)
        {
            try
            {
                var initialCommands = new List<Command>
                {
                    new Command { Id = 1, Method = "subscribe", Params = new List<string>{ subscribeLocation, subscribeUsername } },
                    new Command { Id = 2, Method = "algorithm.add", Params = new List<string>{ "daggerhashimoto" } },
                };
                initialCommands.AddRange(gpuUuids.Select((gpu, index) => new Command { Id = index + 3, Method = "worker.add", Params = new List<string> { "daggerhashimoto", gpu } }));
                var TEMPLATE = new List<Cmd>
                {
                    new Cmd
                    {
                        Time = 0,
                        Commands = initialCommands,
                    },
                    new Cmd
                    {
                        Event = "on_quit",
                        Commands = new List<Command>{ },
                    }
                };
                return JsonConvert.SerializeObject(TEMPLATE, Formatting.Indented);
            }
            catch (Exception e)
            {
                Logger.Error("Excavator.CmdConfig", $"CreateCommandFile error {e.Message}");
                return null;
            }
        }

        private static List<Cmd> ParseTemplateFile(string templateFilePath)
        {
            if (!File.Exists(templateFilePath)) return null;
            try
            {
                return JsonConvert.DeserializeObject<List<Cmd>>(File.ReadAllText(templateFilePath), _jsonSettings);
            }
            catch (Exception e)
            {
                Logger.Error("Excavator.CmdConfig", $"ParseTemplateFile error {e.Message}");
                return null;
            }
        }

        private static bool IsValidSessionCommand(Command command, IEnumerable<string> gpuUuids)
        {
            var anyMissingGpuUuidParams = command.Params
                .Where(p => p.StartsWith("GPU"))
                .Any(pGpu => !gpuUuids.Contains(pGpu));
            return !anyMissingGpuUuidParams;
        }

        private static string[] _invalidTemplateMethiods = new string[] { "subscribe", "algorithm.add", "worker.add" };
        public static string CreateCommandFileWithTemplate(string subscribeLocation, string subscribeUsername, IEnumerable<string> gpuUuids, string templateFilePath)
        {
            // Parse template file
            var template = ParseTemplateFile(templateFilePath);
            if (template == null) return CreateCommandFile(subscribeLocation, subscribeUsername, gpuUuids);
            var validCmds = template
                .Where(cmd => cmd.Commands.All(c => !_invalidTemplateMethiods.Contains(c.Method)))
                .Select(cmd => (cmd, commands: cmd.Commands.Where(c => IsValidSessionCommand(c, gpuUuids)).ToList()))
                .Where(p => p.commands.Any())
                .ToArray();
            foreach (var (cmd, commands) in validCmds)
            {
                // modify commands to not include GPUs that are not part of this mining session
                cmd.Commands = commands;
            }
            try
            {
                var initialCommands = new List<Command>
                {
                    new Command { Id = 1, Method = "subscribe", Params = new List<string>{ subscribeLocation, subscribeUsername } },
                    new Command { Id = 2, Method = "algorithm.add", Params = new List<string>{ "daggerhashimoto" } },
                };
                initialCommands.AddRange(gpuUuids.Select((gpu, index) => new Command { Id = index + 3, Method = "worker.add", Params = new List<string> { "daggerhashimoto", gpu } }));
                var TEMPLATE = new List<Cmd>
                {
                    new Cmd
                    {
                        Time = 0,
                        Commands = initialCommands,
                    },
                };
                if (validCmds.Any()) TEMPLATE.AddRange(validCmds.Select(p => p.cmd));
                return JsonConvert.SerializeObject(TEMPLATE, Formatting.Indented, _jsonSettings);
            }
            catch (Exception e)
            {
                Logger.Error("Excavator.CmdConfig", $"CreateCommandFileWithTemplate error {e.Message}");
                return null;
            }
        }
    }
}

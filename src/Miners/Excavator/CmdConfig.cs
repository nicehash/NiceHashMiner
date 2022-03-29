using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
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
            return CreateCommandString("__SUBSCRIBE_PARAM_LOCATION__", "__SUBSCRIBE_PARAM_USERNAME__", gpuUuids);
        }

        public static string CommandFileTemplatePath(string pluginUUID)
        {
            return Paths.MinerPluginsPath(pluginUUID, "internals", "CommandLineTemplate.json");
        }

        private static string CreateCommandString(string subscribeLocation, string subscribeUsername, IEnumerable<string> gpuUuids)
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

        private static string CreateSafeFallbackCMD(string subscribeLocation, string subscribeUsername, IEnumerable<string> gpuUuids)
        {
            string TEMPLATE = "";
            TEMPLATE += "[\n";
            TEMPLATE += "  {\n";
            TEMPLATE += "    \"time\": 0,\n";
            TEMPLATE += "    \"commands\": [\n";
            TEMPLATE += "      {\n";
            TEMPLATE += "        \"id\": 1,\n";
            TEMPLATE += "        \"method\": \"subscribe\",\n";
            TEMPLATE += "        \"params\": [\n";
            TEMPLATE += "          \"" + subscribeLocation + "\",\n";
            TEMPLATE += "          \"" + subscribeUsername + "\"\n";
            TEMPLATE += "        ]\n";
            TEMPLATE += "      },\n";
            TEMPLATE += "      {\n";
            TEMPLATE += "        \"id\": 2,\n";
            TEMPLATE += "        \"method\": \"algorithm.add\"\n";
            TEMPLATE += "        \"params\": [\n";
            TEMPLATE += "          \"daggerhashimoto\"\n";
            TEMPLATE += "        ]\n";
            TEMPLATE += "      },\n";
            TEMPLATE += "_WORKERS_";
            TEMPLATE += "    ]\n";
            TEMPLATE += "  }\n";
            TEMPLATE += "]\n";

            string WORKERTEMPLATE = "";
            int currentIndex = 0;
            gpuUuids.ToList().ForEach(uuid =>
            {
                WORKERTEMPLATE += "      {\n";
                WORKERTEMPLATE += "        \"id\": 3,\n";
                WORKERTEMPLATE += "        \"method\": \"worker.add\",\n";
                WORKERTEMPLATE += "        \"params\": [\n";
                WORKERTEMPLATE += "          \"daggerhashimoto\",\n";
                WORKERTEMPLATE += "          \"" + uuid + "\"\n";
                WORKERTEMPLATE += "        ]\n";
                if (currentIndex == gpuUuids.Count() - 1) WORKERTEMPLATE += "      }\n";
                else WORKERTEMPLATE += "      },\n";
                currentIndex++;
            });
            TEMPLATE = TEMPLATE.Replace("_WORKERS_", WORKERTEMPLATE);
            return TEMPLATE;
        }

        private static string[] _invalidTemplateMethiods = new string[] { "subscribe", "algorithm.add", "worker.add" };
        private static string CreateCommandWithTemplate(string subscribeLocation, string subscribeUsername, IEnumerable<string> gpuUuids, string templateFilePath)
        {
            // Parse template file
            var template = ParseTemplateFile(templateFilePath);
            if (template == null) return CreateCommandString(subscribeLocation, subscribeUsername, gpuUuids);
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
        private static string GetServiceLocation(string miningLocation)
        {
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return $"nhmp-ssl-test.{miningLocation}.nicehash.com:443";
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return $"stratum-dev.{miningLocation}.nicehash.com:443";
            //BuildTag.PRODUCTION
            return $"nhmp.auto.nicehash.com:443";
        }

        public static string CmdJSONString(string pluginUUID, string _miningLocation, string username, params string[] uuids)
        {
            var miningLocation = GetMiningLocation(_miningLocation);
            var templatePath = CommandFileTemplatePath(pluginUUID);
            var miningServiceLocation = GetServiceLocation(miningLocation);
            var command = CreateCommandWithTemplate(miningServiceLocation, username, uuids, templatePath);
            if (command == null)
            {
                Logger.Error("Excavator.CmdConfig", "command is NULL, creating with safe method");
                command = CreateSafeFallbackCMD(miningServiceLocation, username, uuids);
            }
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

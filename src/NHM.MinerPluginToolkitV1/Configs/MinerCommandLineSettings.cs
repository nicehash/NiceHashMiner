using Newtonsoft.Json;
using NHM.Common.Configs;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1.Configs
{
    [Serializable]
    public class MinerCommandLineSettings : IInternalSetting
    {
        public const string USERNAME_TEMPLATE = "{USERNAME}";
        public const string API_PORT_TEMPLATE = "{API_PORT}";
        public const string POOL_URL_TEMPLATE = "{POOL_URL}";
        public const string POOL_PORT_TEMPLATE = "{POOL_PORT}";
        public const string POOL2_URL_TEMPLATE = "{POOL2_URL}";
        public const string POOL2_PORT_TEMPLATE = "{POOL2_PORT}";
        public const string DEVICES_TEMPLATE = "{DEVICES}";
        public const string OPEN_CL_AMD_PLATFORM_NUM = "{OPEN_CL_AMD_PLATFORM_NUM}";
        public const string EXTRA_LAUNCH_PARAMETERS_TEMPLATE = "{EXTRA_LAUNCH_PARAMETERS}";

        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("devices_separator")]
        public string DevicesSeparator { get; set; } = ",";

        [JsonProperty("algorithm_command_line")]
        public Dictionary<string, string> AlgorithmCommandLine { get; set; } = new Dictionary<string, string>
        {
            //{AlgorithmType.DaggerHashimoto.ToString(), $"--user {USERNAME_TEMPLATE} --pool {POOL_URL_TEMPLATE}:{POOL_PORT_TEMPLATE} --algo dagger --apiport {API_PORT_TEMPLATE} --devices {DEVICES_TEMPLATE} {EXTRA_LAUNCH_PARAMETERS_TEMPLATE}"},
        };


        public class CommandLineParams
        {
            public string Username { set; get; } = "";
            public string ApiPort { set; get; } = "";
            public string Url { set; get; } = "";
            public string Port { set; get; } = "";
            public string Url2 { set; get; } = "";
            public string Port2 { set; get; } = "";
            public string Devices { set; get; } = "";
            public string OpenClAmdPlatformNum { set; get; } = "";
            public string ExtraLaunchParameters { set; get; } = "";
        }

        public static string MiningCreateCommandLine(string template, CommandLineParams p)
        {
            var commandLine = template
                .Replace(USERNAME_TEMPLATE, p.Username)
                .Replace(API_PORT_TEMPLATE, p.ApiPort)
                .Replace(POOL_URL_TEMPLATE, p.Url)
                .Replace(POOL_PORT_TEMPLATE, p.Port)
                .Replace(POOL2_URL_TEMPLATE, p.Url2)
                .Replace(POOL2_PORT_TEMPLATE, p.Port2)
                .Replace(DEVICES_TEMPLATE, p.Devices)
                .Replace(OPEN_CL_AMD_PLATFORM_NUM, p.OpenClAmdPlatformNum)
                .Replace(EXTRA_LAUNCH_PARAMETERS_TEMPLATE, p.ExtraLaunchParameters)
                ;
            return commandLine;
        }
    }
}

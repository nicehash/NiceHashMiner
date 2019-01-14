using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.Forms;
using NiceHashMiner.PInvoke;
using NiceHashMiner.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            // Set working directory to exe
            var pathSet = false;
            var path = Path.GetDirectoryName(Application.ExecutablePath);
            if (path != null)
            {
                Environment.CurrentDirectory = path;
                pathSet = true;
            }

            // Add common folder to path for launched processes
            var pathVar = Environment.GetEnvironmentVariable("PATH");
            pathVar += ";" + Path.Combine(Environment.CurrentDirectory, "common");
            Environment.SetEnvironmentVariable("PATH", pathVar);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            //Console.OutputEncoding = System.Text.Encoding.Unicode;
            // #0 set this first so data parsing will work correctly
            Globals.JsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
            };

            // #1 first initialize config
            ConfigManager.InitializeConfig();

            // #2 check if multiple instances are allowed
            if (ConfigManager.GeneralConfig.AllowMultipleInstances == false)
            {
                try
                {
                    var current = Process.GetCurrentProcess();
                    foreach (var process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            // already running instance, return from Main
                            return;
                        }
                    }
                }
                catch { }
            }

            // start program
            if (ConfigManager.GeneralConfig.LogToFile)
            {
                Logger.ConfigureWithFile();
            }

            if (ConfigManager.GeneralConfig.DebugConsole)
            {
                PInvokeHelpers.AllocConsole();
            }

            // init active display currency after config load
            ExchangeRateApi.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;
                
            Helpers.ConsolePrint("NICEHASH", "Starting up NiceHashMiner v" + Application.ProductVersion);

            if (!pathSet)
            {
                Helpers.ConsolePrint("NICEHASH", "Path not set to executable");
            }
            
            var tosChecked = ConfigManager.GeneralConfig.agreedWithTOS == Globals.CurrentTosVer;
            if (!tosChecked)
            {
                Helpers.ConsolePrint("NICEHASH",
                    "No config file found. Running NiceHash Miner Legacy for the first time. Choosing a default language.");
                Application.Run(new Form_ChooseLanguage());
            }
            // Init languages
            Translations.SetLanguage(ConfigManager.GeneralConfig.Language);
            // 3rdparty miners TOS check if setting set
            if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.NOT_SET)
            {
                Application.Run(new Form_3rdParty_TOS());
            }

            // if system requirements are not ensured it will fail the program
            var canRun = ApplicationStateManager.SystemRequirementsEnsured();
            // make an ensure TOS
            if (canRun && ConfigManager.GeneralConfig.agreedWithTOS == Globals.CurrentTosVer)
            {
                Application.Run(new Form_Main());
            }
            
        }
    }
}

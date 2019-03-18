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

            // check TOS
            if (ConfigManager.GeneralConfig.agreedWithTOS != Globals.CurrentTosVer)
            {
                Helpers.ConsolePrint("NICEHASH", $"TOS differs! agreed: {ConfigManager.GeneralConfig.agreedWithTOS} != Current {Globals.CurrentTosVer}. Showing TOS Form.");
                Application.Run(new FormEula()); 
                // check TOS after 
                if (ConfigManager.GeneralConfig.agreedWithTOS != Globals.CurrentTosVer) {
                    Helpers.ConsolePrint("NICEHASH", $"TOS differs AFTER TOS confirmation FORM");
                    // TOS not confirmed return from Main
                    return;
                }
            }
            // if config created show language select
            if (string.IsNullOrEmpty(ConfigManager.GeneralConfig.Language))
            {
                if (Translations.GetAvailableLanguagesNames().Count > 1)
                {
                    Application.Run(new Form_ChooseLanguage());
                }
                else
                {
                    ConfigManager.GeneralConfig.Language = "en";
                    ConfigManager.GeneralConfigFileCommit();
                }
            }
            Translations.SetLanguage(ConfigManager.GeneralConfig.Language);
            
            // check WMI
            if (Helpers.IsWmiEnabled())
            {
                // if no BTC address show login/register form
                if (ConfigManager.GeneralConfig.BitcoinAddress.Trim() == "") Application.Run(new EnterBTCDialogSwitch());
                // finally run
                Application.Run(new Form_Main());
            }
            else
            {
                MessageBox.Show(Translations.Tr("NiceHash Miner Legacy cannot run needed components. It seems that your system has Windows Management Instrumentation service Disabled. In order for NiceHash Miner Legacy to work properly Windows Management Instrumentation service needs to be Enabled. This service is needed to detect RAM usage and Avaliable Video controler information. Enable Windows Management Instrumentation service manually and start NiceHash Miner Legacy."),
                    Translations.Tr("Windows Management Instrumentation Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

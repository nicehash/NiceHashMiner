using log4net.Core;
using NHM.Common;
using NHM.Wpf.Views;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Stats;
using NHMCore.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace NHM.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if TESTNET
        private static readonly string BuildTag = "TESTNET";
#elif TESTNETDEV
        private static readonly string BuildTag = "TESTNETDEV";
#else
        private static readonly string BuildTag = "PRODUCTION";
#endif
        private const string Tag = "NICEHASH";

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            NHMCore.BUILD_TAG.ASSERT_COMPATIBLE_BUILDS(BuildTag);
            // Set working directory to exe
            var pathSet = false;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path != null)
            {
                Paths.SetRoot(path);
                Environment.CurrentDirectory = path;
                pathSet = true;
            }

            // Add common folder to path for launched processes
            const string pathKey = "PATH";
            var pathVar = Environment.GetEnvironmentVariable(pathKey);
            pathVar += $";{Path.Combine(Environment.CurrentDirectory, "common")}";
            Environment.SetEnvironmentVariable(pathKey, pathVar);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // Set security protocols
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12 |
                                                   SecurityProtocolType.Ssl3;

            // Initialize config
            ConfigManager.InitializeConfig();

            // Check multiple instances
            if (!ConfigManager.GeneralConfig.AllowMultipleInstances)
            {
                try
                {
                    var current = Process.GetCurrentProcess();
                    if (Process.GetProcessesByName(current.ProcessName).Any(p => p.Id != current.Id))
                    {
                        // Shutdown to exit
                        Shutdown();
                    }
                }
                catch
                { }
            }
            
            // Init logger
            Logger.ConfigureWithFile(ConfigManager.GeneralConfig.LogToFile, Level.Info, ConfigManager.GeneralConfig.LogMaxFileSize);

            if (ConfigManager.GeneralConfig.DebugConsole)
            {
                PInvokeHelpers.AllocConsole();
                Logger.ConfigureConsoleLogging(Level.Info);
            }

            if (!pathSet)
            {
                Logger.Warn(Tag, "Path not set to executable");
            }

            // Set to explicit shutdown or else these intro windows will cause shutdown
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Init ExchangeRateApi
            ExchangeRateApi.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

            Logger.Info(Tag, $"Starting up {ApplicationStateManager.Title}");
            if (ConfigManager.GeneralConfig.agreedWithTOS != ApplicationStateManager.CurrentTosVer)
            {
                Logger.Info(Tag, $"TOS differs! agreed: {ConfigManager.GeneralConfig.agreedWithTOS} != Current {ApplicationStateManager.CurrentTosVer}");

                var eula = new EulaWindow{};
                var accepted = eula.ShowDialog();
                if (accepted.HasValue && eula.AcceptedTos)
                {
                    ConfigManager.GeneralConfig.agreedWithTOS = ApplicationStateManager.CurrentTosVer;
                }
                else
                {
                    Logger.Error(Tag, "TOS differs AFTER TOS confirmation window");
                    Shutdown();
                    return;
                }
            }

            // Check 3rd party miners TOS
            if (ConfigManager.GeneralConfig.Use3rdPartyMinersTOS != ApplicationStateManager.CurrentTosVer)
            {
                var thirdPty = new _3rdPartyTosWindow{};
                thirdPty.ShowDialog();
                if (!thirdPty.Accepted)
                {
                    Logger.Error(Tag, "3rd party TOS not accepted");
                    Shutdown();
                    return;
                }
                ConfigManager.GeneralConfig.Use3rdPartyMinersTOS = ApplicationStateManager.CurrentTosVer;
                ConfigManager.GeneralConfigFileCommit();
            }

            // Chose lang
            if (string.IsNullOrEmpty(ConfigManager.GeneralConfig.Language))
            {
                var langToSet = "en";
                if (Translations.GetAvailableLanguagesNames().Count > 1)
                {
                    var lang = new ChooseLanguageWindow
                    {
                        LangNames = Translations.GetAvailableLanguagesNames()
                    };

                    lang.ShowDialog();
                    langToSet = Translations.GetLanguageCodeFromIndex(lang.SelectedLangIndex);
                }

                ConfigManager.GeneralConfig.Language = langToSet;
                ConfigManager.GeneralConfigFileCommit();
            }

            Translations.SelectedLanguage = ConfigManager.GeneralConfig.Language;

            // Check sys requirements
            var canRun = ApplicationStateManager.SystemRequirementsEnsured();
            if (!canRun) Shutdown();

            var main = new MainWindow() {};
            main.Show();

            // Set shutdown mode back to default
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
    }
}

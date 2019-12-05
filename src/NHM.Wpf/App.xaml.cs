//#define __DESIGN_DEVELOP

using log4net.Core;
using NHM.Common;
#if __DESIGN_DEVELOP
using NHM.Wpf._DESIGN_DEVELOP;
#endif
using NHM.Wpf.Views;
using NHMCore;
using NHMCore.Configs;
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
        private const string Tag = "NICEHASH";

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
#if __DESIGN_DEVELOP
            var designWindow = new __DESIGN_DEVELOP();
            designWindow.ShowDialog();
            return;
#endif

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
            if (!MiscSettings.Instance.AllowMultipleInstances)
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
            Logger.ConfigureWithFile(LoggingDebugConsoleSettings.Instance.LogToFile, Level.Info, LoggingDebugConsoleSettings.Instance.LogMaxFileSize);

            if (LoggingDebugConsoleSettings.Instance.DebugConsole)
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

            Logger.Info(Tag, $"Starting up {ApplicationStateManager.Title}");
            if (ToSSetings.Instance.AgreedWithTOS != ApplicationStateManager.CurrentTosVer)
            {
                Logger.Info(Tag, $"TOS differs! agreed: {ToSSetings.Instance.AgreedWithTOS} != Current {ApplicationStateManager.CurrentTosVer}");

                var eula = new EulaWindow{};
                var accepted = eula.ShowDialog();
                if (accepted.HasValue && eula.AcceptedTos)
                {
                    ToSSetings.Instance.AgreedWithTOS = ApplicationStateManager.CurrentTosVer;
                }
                else
                {
                    Logger.Error(Tag, "TOS differs AFTER TOS confirmation window");
                    Shutdown();
                    return;
                }
            }

            // Check 3rd party miners TOS
            if (ToSSetings.Instance.Use3rdPartyMinersTOS != ApplicationStateManager.CurrentTosVer)
            {
                var thirdPty = new _3rdPartyTosWindow{};
                thirdPty.ShowDialog();
                if (!thirdPty.Accepted)
                {
                    Logger.Error(Tag, "3rd party TOS not accepted");
                    Shutdown();
                    return;
                }
                ToSSetings.Instance.Use3rdPartyMinersTOS = ApplicationStateManager.CurrentTosVer;
                ConfigManager.GeneralConfigFileCommit();
            }

            // Chose lang
            if (string.IsNullOrEmpty(TranslationsSettings.Instance.Language))
            {
                if (Translations.GetAvailableLanguagesNames().Count > 1)
                {
                    var lang = new ChooseLanguageWindow{};
                    lang.ShowDialog();
                }
                // check if user didn't choose anything
                if (string.IsNullOrEmpty(TranslationsSettings.Instance.Language))
                {
                    TranslationsSettings.Instance.Language = "en";
                }
                ConfigManager.GeneralConfigFileCommit();
            }

            Translations.SelectedLanguage = TranslationsSettings.Instance.Language;

            //var login = new LoginWindow { };
            //var nek = login.ShowDialog();

            // Check sys requirements
            var canRun = ApplicationStateManager.SystemRequirementsEnsured();
            if (!canRun) Shutdown();

            //var main = new MainWindow() {};
            //main.Show();
            //var m2 = new MainWindowNew();
            //m2.Show();
            //var m2 = new EulaWindow();
            //var m2 = new ChooseLanguageWindow();
            //var m2 = new _3rdPartyTosWindow();
            //var m2 = new LoginWindow();
            var m2 = new MainWindowNew2();
            m2.Show();

            // Set shutdown mode back to default
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
    }
}

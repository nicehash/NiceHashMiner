//#define __DESIGN_DEVELOP

using log4net.Core;
using NHM.Common;
#if __DESIGN_DEVELOP
using NiceHashMiner._DESIGN_DEVELOP;
#endif
using NiceHashMiner.Views;
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
#if ENABLE_LOGIN
using NiceHashMiner.Views.Login;
using System.Windows.Media;
using System.Windows.Interop;
#endif

namespace NiceHashMiner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string Tag = "NICEHASH";

        public static int ParseLauncherPID()
        {
            try
            {
                var launcherPIDArg = Environment.GetCommandLineArgs().Where(arg => arg.Contains("-PID")).FirstOrDefault();
                if (launcherPIDArg != null && int.TryParse(launcherPIDArg.Replace("-PID", ""), out var pid)) return pid;
            }
            catch
            { }
            return -1;
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
#if __DESIGN_DEVELOP
            var designWindow = new __DESIGN_DEVELOP();
            designWindow.ShowDialog();
            return;
#endif
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            ApplicationStateManager.App = this;
            ApplicationStateManager.ApplicationExit = () =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Shutdown();
                });
            };
            var isDevelop = Environment.GetCommandLineArgs().Contains("-dev");
            var isLauncher = Environment.GetCommandLineArgs().Contains("-lc");
            var launcherPID = ParseLauncherPID();
            Launcher.SetIsUpdated(Environment.GetCommandLineArgs().Contains("-updated"));
            Launcher.SetIsUpdatedFailed(Environment.GetCommandLineArgs().Contains("-updateFailed"));
            Launcher.SetIsLauncher(isLauncher);
            // Set working directory to exe
            var pathSet = false;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path != null)
            {
                if (isLauncher)
                {
                    var oneUpPath = new Uri(Path.Combine(path, @"..\")).LocalPath;
                    Paths.SetRoot(oneUpPath);
                    Paths.SetAppRoot(path);
                    // TODO this might be problematic
                    Environment.CurrentDirectory = oneUpPath;
                }
                else if (isDevelop)
                {
                    Paths.SetRoot(path);
                    Paths.SetAppRoot(path);
                    Environment.CurrentDirectory = path;
                }
                else
                {
                    MessageBox.Show("You must run the application via 'NiceHashMiner.exe' launcher", "Direct run disabled!", MessageBoxButton.YesNo);
                    Application.Current.Shutdown();
                    return;
                }
                pathSet = true;
            }

            // Add common folder to path for launched processes
            const string pathKey = "PATH";
            var pathVar = Environment.GetEnvironmentVariable(pathKey);
            pathVar += $";{Path.Combine(Paths.AppRoot, "common")}";
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

            ThemeSetterManager.SetTheme(GUISettings.Instance.DisplayTheme);

            // Check multiple instances
            if (!MiscSettings.Instance.AllowMultipleInstances)
            {
                try
                {
                    var current = Process.GetCurrentProcess();
                    var processesIds = Process.GetProcessesByName(current.ProcessName).Select(p => p.Id);
                    if (processesIds.Any(pid => pid != current.Id && pid != launcherPID))
                    {
                        var singleInstanceNotice = new SingleInstanceNotice { };
                        singleInstanceNotice.ShowDialog();
                        // Shutdown to exit
                        Shutdown();
                        return;
                    }
                }
                catch
                { }
            }

            // Init logger
            Logger.ConfigureWithFile(LoggingDebugConsoleSettings.Instance.LogToFile, Level.Info, LoggingDebugConsoleSettings.Instance.LogMaxFileSize);
            BuildOptions.Init();
            Logger.Info(Tag, $"Build {BuildOptions.BUILD_TAG}");

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

                var eula = new EulaWindowFirstLong { };
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
                var thirdPty = new EulaWindowSecondShort { };
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
            if (string.IsNullOrEmpty(TranslationsSettings.Instance.Language) && AppRuntimeSettings.ShowLanguage)
            {
                if (Translations.GetAvailableLanguagesNames().Count > 1)
                {
                    var lang = new ChooseLanguageWindow { };
                    lang.ShowDialog();
                }
                // check if user didn't choose anything
                if (string.IsNullOrEmpty(TranslationsSettings.Instance.Language))
                {
                    TranslationsSettings.Instance.Language = "en";
                }
                ConfigManager.GeneralConfigFileCommit();
            }
            else if (string.IsNullOrEmpty(TranslationsSettings.Instance.Language) && !AppRuntimeSettings.ShowLanguage)
            {
                // while we have locale disabled set english
                TranslationsSettings.Instance.Language = "en";
                ConfigManager.GeneralConfigFileCommit();
            }

            Translations.SelectedLanguage = TranslationsSettings.Instance.Language;

            // Check sys requirements
            var canRun = ApplicationStateManager.SystemRequirementsEnsured();
            if (!canRun)
            {
                Shutdown();
                return;
            }

            bool? loginSuccess = null;
#if ENABLE_LOGIN
            FilterOSSpecific.GetWindowsVersion();
            // show login if no BTC
            if (!CredentialsSettings.Instance.IsBitcoinAddressValid && AppRuntimeSettings.ShowLoginWindow && SystemVersion.BuildNumber >= 17110)
            {
                var login = new LoginWindow { };
                login.ShowDialog();
                loginSuccess = login.LoginSuccess;
            }
#endif
            if (!CredentialsSettings.Instance.IsBitcoinAddressValid)
            {
                var btcNotice = new DemoBTCNotice { };
                btcNotice.ShowDialog();
            }

            var main = new MainWindow(loginSuccess);
            main.Show();

            //// Set shutdown mode back to default
            //ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Exception ex_inner = ex.InnerException;
            string msg = ex.Message + "\n\n" + ex.StackTrace + "\n\n" +
                "Inner Exception:\n" + ex_inner.Message + "\n\n" + ex_inner.StackTrace;
            MessageBox.Show(msg, "Application Halted!", MessageBoxButton.OK);
            e.Handled = true;
            Application.Current.Shutdown();
        }
    }
}

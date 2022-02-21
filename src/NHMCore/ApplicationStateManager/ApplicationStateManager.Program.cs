using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Mining.Plugins;
using NHMCore.Nhmws;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NHMCore.Translations;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        #region BuildTag
        private const string BetaAlphaPostfixString = "";

        private static string BuildTagStr
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return " (TESTNET)";
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return " (TESTNETDEV)";
                //BuildTag.PRODUCTION
                return "";
            }
        }

        public static string Title
        {
            get
            {
                return $"{NHMProductInfo.Name} v" + Application.ProductVersion + BetaAlphaPostfixString + BuildTagStr;
            }
        }
        #endregion BuildTag

        public static void VisitMiningStatsPage()
        {
            var urlLink = Links.CheckStatsRig.Replace("{RIG_ID}", RigID());
            Helpers.VisitUrlLink(urlLink);
        }

        public static Action ApplicationExit;

        public static void ExecuteApplicationExit()
        {
            Application.Exit();
            ApplicationExit?.Invoke();
        }

        public static CancellationTokenSource ExitApplication { get; } = new CancellationTokenSource();

        private static bool _beforeExitCalled = false;
        public static async Task BeforeExit()
        {
            if (_beforeExitCalled) return;
            _beforeExitCalled = true;
            try
            {
                // should close websocket  
                ExitApplication.Cancel();
                ConfigManager.GeneralConfigFileCommit();
                var waitTasks = new List<Task>();
                waitTasks.Add(MiningManager.RunninLoops);
                waitTasks.Add(NHWebSocket.MainLoop);
                waitTasks.Add(MinerPluginsManager.RunninLoops);
                waitTasks.Add(UpdateHelpers.RunninLoops);
                await Task.WhenAll(waitTasks.Where(t => t != null));
            }
            catch (Exception e)
            {
                Logger.Info("BeforeExit", e.Message);
            }
            finally
            {
            }
        }

        private static bool _restartCalled = false;
        public static async Task RestartProgram()
        {
            if (_restartCalled) return;
            _restartCalled = true;
            await BeforeExit();
            //var startInfo = new ProcessStartInfo { FileName = Application.ExecutablePath };
            //using (var pHandle = new Process { StartInfo = startInfo })
            //{
            //    pHandle.Start();
            //}
            // TODO we can have disable multiple instances so make a helper program that "swaps"/restarts parent/child
            if (!Launcher.IsLauncher)
            {
                Process.Start(Application.ExecutablePath);
            }
            else
            {
                try
                {
                    File.Create(Paths.RootPath("do.restart"));
                }
                catch (Exception e)
                {
                    Logger.Error("ApplicationStateManager.Program", $"do.restart error: {e.Message}");
                }
            }
            ExecuteApplicationExit();
        }

        public static bool BurnCalled { get; private set; } = false;
        public static Action BurnCalledAction;
        public static void Burn(string message)
        {
            if (BurnCalled) return;
            BurnCalled = true;
            _ = BeforeExit();
            BurnCalledAction?.Invoke();
        }

        // EnsureSystemRequirements will check if all system requirements are met if not it will show an error/warning message box and exit the application
        // TODO this one holds
        public static bool SystemRequirementsEnsured()
        {
            // check WMI
            if (!SystemSpecs.IsWmiEnabled())
            {
                MessageBox.Show(Tr("{0} cannot run needed components. It seems that your system has Windows Management Instrumentation service Disabled. In order for {0} to work properly Windows Management Instrumentation service needs to be Enabled. This service is needed to detect RAM usage and Avaliable Video controler information. Enable Windows Management Instrumentation service manually and start {0}.", NHMProductInfo.Name),
                        Tr("Windows Management Instrumentation Error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }

            if (!Helpers.Is45NetOrHigher())
            {
                MessageBox.Show(Tr("{0} requires .NET Framework 4.5 or higher to work properly. Please install Microsoft .NET Framework 4.5", NHMProductInfo.Name),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);

                return false;
            }

            if (!Helpers.Is64BitOperatingSystem)
            {
                MessageBox.Show(Tr("{0} supports only x64 platforms. You will not be able to use {0} with x86", NHMProductInfo.Name),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);

                return false;
            }

            return true;
        }

        public static Action NoDeviceAction;
    }
}

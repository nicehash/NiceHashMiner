// SHARED
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Forms.Components;
using NiceHashMiner.Miners;
using NiceHashMiner.Plugin;
using NiceHashMiner.Stats;
using NiceHashMiner.Utils;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static NiceHashMiner.Translations;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        delegate void InitStep();
        class ActionWithMessage
        {
            public InitStep initStep { get; set; }
            public string message { get; set; }
        }

        public static async Task InitializeManagersAndMiners(StartupLoadingControl loadingControl, IProgress<(string loadMessageText, int prog)> progress, IProgress<(string loadMessageText, int prog)> progressDownload)
        {
            var allSteps = 14; 
            var currentStep = 0;
            var nextProgPerc = new Func<int>(() =>
            {
                ++currentStep;
                var perc = (int)(((double)currentStep / allSteps) * 100);
                if (perc > 100) return 100;
                return perc;
            });
            var runVCRed = !MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit;

            // STEP
            // Checking System Memory
            progress?.Report((Tr("Checking System Memory"), nextProgPerc()));
            await Task.Run(() => WindowsManagementObjectSearcher.QueryWin32_OperatingSystemData());
            var TotalVisibleMemorySize = WindowsManagementObjectSearcher.TotalVisibleMemorySize;
            var TotalVirtualMemorySize = WindowsManagementObjectSearcher.TotalVirtualMemorySize;
            var PageFileSize = WindowsManagementObjectSearcher.PageFileSize;
            var FreePhysicalMemory = WindowsManagementObjectSearcher.FreePhysicalMemory;
            var FreeVirtualMemory = WindowsManagementObjectSearcher.FreeVirtualMemory;
            Logger.Info("NICEHASH", $"TotalVisibleMemorySize: {TotalVisibleMemorySize}, {TotalVisibleMemorySize / 1024} MB");
            Logger.Info("NICEHASH", $"TotalVirtualMemorySize: {TotalVirtualMemorySize}, {TotalVirtualMemorySize / 1024} MB");
            Logger.Info("NICEHASH", $"PageFileSize = {PageFileSize}, {PageFileSize / 1024} MB");
            Logger.Info("NICEHASH", $"FreePhysicalMemory = {FreePhysicalMemory}, {FreePhysicalMemory / 1024} MB");
            Logger.Info("NICEHASH", $"FreeVirtualMemory = {FreeVirtualMemory}, {FreeVirtualMemory / 1024} MB");

            // STEP
            // Checking Windows Video Controllers
            progress?.Report((Tr("Checking Windows Video Controllers"), nextProgPerc()));
            await Task.Run(() => WindowsManagementObjectSearcher.QueryWin32_VideoController());

            // STEP + 2
            // TODO split CPU, AMD and NVIDIA detection into separate functions
            // device detection 3 steps
            var detectionProgress = new Progress<string>(info => progress?.Report((info, nextProgPerc())));
            // Query Available ComputeDevices
            var query = await ComputeDeviceManager.QueryDevicesAsync(detectionProgress, false);
            ApplicationStateManager.ShowQueryWarnings(query);

            // STEP
            // load plugins
            progress?.Report((Tr("Loading miner plugins..."), nextProgPerc()));
            // Plugin Loading
            MinerPluginsManager.LoadMinerPlugins();
            /////////////////////////////////////////////
            /////// from here on we have our devices and Miners initialized
            ApplicationStateManager.AfterDeviceQueryInitialization();

            // STEP
            // connect to nhmws
            progress?.Report((Tr("Connecting to nhmws..."), nextProgPerc()));
            // Init ws connection
            NiceHashStats.StartConnection(Nhmws.NhmSocketAddress);

            // STEP
            // disable windows error reporting
            progress?.Report((Tr("Setting Windows error reporting..."), nextProgPerc()));
            Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);

            // STEP
            // Nvidia p0
            progress?.Report((Tr("Changing all supported NVIDIA GPUs to P0 state..."), nextProgPerc()));
            if (ConfigManager.GeneralConfig.NVIDIAP0State && AvailableDevices.HasNvidia)
            {
                Helpers.SetNvidiaP0State();
            }

            // STEP
            // Download open source miners
            // standard miners check scope
            // check if download needed
            if (!MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit)
            {
                loadingControl.LoadTitleTextSecond = Tr("Downloading Open Source Miners");
                loadingControl.ShowSecondProgressBar = true;

                progress?.Report((Tr("Downloading Open Source Miners..."), nextProgPerc()));
                await MinersDownloader.MinersDownloadManager.DownloadAndExtractOpenSourceMinersWithMyDownloaderAsync(progressDownload, ExitApplication.Token);
                loadingControl.ShowSecondProgressBar = false;
                if (ExitApplication.IsCancellationRequested) return;
            }
            // check if files are mising
            if (!MinersExistanceChecker.IsMinersBinsInit())
            {
                var result = MessageBox.Show(Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. NiceHash Miner might not work properly without missing files. Click Yes to reinitialize NiceHash Miner to try to fix this issue."),
                    Tr("Warning!"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    ConfigManager.GeneralConfig.DownloadInit = false;
                    ConfigManager.GeneralConfigFileCommit();
                    ApplicationStateManager.RestartProgram();
                    return;
                }
            }
            else if (!ConfigManager.GeneralConfig.DownloadInit)
            {
                // all good
                ConfigManager.GeneralConfig.DownloadInit = true;
                ConfigManager.GeneralConfigFileCommit();
            }

            // STEP
            // 3rdparty miners check
            // 3rdparty miners check scope #2
            // check if download needed
            if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES)
            {
                if (!MinersExistanceChecker.IsMiners3rdPartyBinsInit() && !ConfigManager.GeneralConfig.DownloadInit3rdParty)
                {
                    loadingControl.LoadTitleTextSecond = Tr("Downloading 3rd party Miners");
                    loadingControl.ShowSecondProgressBar = true;

                    progress?.Report((Tr("Downloading 3rd party Miners..."), nextProgPerc()));
                    await MinersDownloader.MinersDownloadManager.DownloadAndExtractThirdPartyMinersWithMyDownloaderAsync(progressDownload, ExitApplication.Token);
                    loadingControl.ShowSecondProgressBar = false;
                    if (ExitApplication.IsCancellationRequested) return;
                }
                // check if files are mising
                if (!MinersExistanceChecker.IsMiners3rdPartyBinsInit())
                {
                    var result = MessageBox.Show(Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. NiceHash Miner might not work properly without missing files. Click Yes to reinitialize NiceHash Miner to try to fix this issue."),
                        Tr("Warning!"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        ConfigManager.GeneralConfig.DownloadInit3rdParty = false;
                        ConfigManager.GeneralConfigFileCommit();
                        ApplicationStateManager.RestartProgram();
                        return;
                    }
                }
                else if (!ConfigManager.GeneralConfig.DownloadInit3rdParty)
                {
                    // all good
                    ConfigManager.GeneralConfig.DownloadInit3rdParty = true;
                    ConfigManager.GeneralConfigFileCommit();
                }
            }

            // STEP
            // VC_REDIST check
            progress?.Report((Tr("Checking VC_REDIST..."), nextProgPerc()));
            if (runVCRed)
            {
                Helpers.InstallVcRedist();
            }

            // STEP
            progress?.Report((Tr("Checking Firewall Rules..."), nextProgPerc()));
            if (FirewallRules.IsFirewallRulesOutdated())
            {
                // requires UAC
                // TODO show message box
                FirewallRules.UpdateFirewallRules();
            }

            // STEP
            // Cross reference plugin indexes
            progress?.Report((Tr("Cross referencing miner device IDs..."), nextProgPerc()));
            // Detected devices cross reference with miner indexes
            await MinerPluginsManager.DevicesCrossReferenceIDsWithMinerIndexes();

            InitFinished();
        }

        public static void ShowQueryWarnings(QueryResult query)
        {
            if (query.FailedMinNVDriver)
            {
                MessageBox.Show(string.Format(
                        Tr(
                            "We have detected that your system has Nvidia GPUs, but your driver is older than {0}. In order for NiceHash Miner to work correctly you should upgrade your drivers to recommended {1} or newer. If you still see this warning after updating the driver please uninstall all your Nvidia drivers and make a clean install of the latest official driver from http://www.nvidia.com."),
                        query.MinDriverString,
                        query.RecommendedDriverString),
                    Tr("Nvidia Recomended driver"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (query.FailedRecommendedNVDriver)
            {
                MessageBox.Show(string.Format(
                        Tr(
                            "We have detected that your Nvidia Driver is older than {0}{1}. We recommend you to update to {2} or newer."),
                        query.RecommendedDriverString,
                        query.CurrentDriverString,
                        query.RecommendedDriverString),
                    Tr("Nvidia Recomended driver"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (query.NoDevices)
            {
                var result = MessageBox.Show(Tr("No supported devices are found. Select the OK button for help or cancel to continue."),
                    Tr("No Supported Devices"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    Process.Start(Links.NhmNoDevHelp);
                }
            }

            if (query.FailedRamCheck)
            {
                MessageBox.Show(Tr("NiceHash Miner recommends increasing virtual memory size so that all algorithms would work fine."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);
            }

            if (query.FailedVidControllerStatus)
            {
                var msg = Tr("We have detected a Video Controller that is not working properly. NiceHash Miner will not be able to use this Video Controller for mining. We advise you to restart your computer, or reinstall your Video Controller drivers.");
                msg += '\n' + query.FailedVidControllerInfo;
                MessageBox.Show(msg,
                    Tr("Warning! Video Controller not operating correctly"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            //if (query.FailedCpuCount)
            //{
            //    MessageBox.Show(Tr("NiceHash Miner does not support more than 64 virtual cores. CPU mining will be disabled."),
            //        Tr("Warning!"),
            //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }

        private static bool isInitFinished = false;
        public static void InitFinished()
        {
            isInitFinished = true;
            NiceHashStats.StateChanged();
        }
    }
}

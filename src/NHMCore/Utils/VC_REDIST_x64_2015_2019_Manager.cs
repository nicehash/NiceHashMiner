using Microsoft.Win32;
using NHM.Common;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Interfaces;
using NHM.MinersDownloader;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    internal class VC_REDIST_x64_2015_2019_Manager : IBinaryPackageMissingFilesChecker
    {
        VC_REDIST_x64_2015_2019_Manager() { }
        public static VC_REDIST_x64_2015_2019_Manager Instance { get; } = new VC_REDIST_x64_2015_2019_Manager();
        public string Tag => "VC_REDIST_x64_2015_2019_Manager";
        private bool IsVcRedistInstalled()
        {
            // x64 - 14.23.27820
            const int minMajor = 14;
            const int minMinor = 23;
            try
            {
                using var vcredist = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
                var major = Int32.Parse(vcredist.GetValue("Major")?.ToString());
                var minor = Int32.Parse(vcredist.GetValue("Minor")?.ToString());
                if (major < minMajor) return false;
                if (minor < minMinor) return false;
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"IsVcRedistInstalled {e}");
            }
            return false;
        }
        public string VCRedistInstallerPath()
        {
            return Path.Combine(VCRedistFolder(), "VC_redist.x64_2015_2019.exe");
        }
        public string VCRedistFolder()
        {
            return Paths.AppRootPath("common", "VC_redist");
        }
        public async Task InitVCRedist(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            if (IsVcRedistInstalled())
            {
                Logger.Info(Tag, $"Skipping installation minimum version newer already installed");
                return;
            }
            if (MiscSettings.Instance.DisableVisualCRedistributableCheck)
            {
                Logger.Info(Tag, $"Skipping installation MiscSettings.Instance.DisableVisualCRedistributableCheck=true");
                return;
            }
            if (CheckBinaryPackageMissingFiles().Any())
            {
                var downloadProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Downloading {0} %", $"{Tag} {perc}"), perc)));
                var unzipProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Extracting {0} %", $"{Tag} {perc}"), perc)));
                await DownloadVCRedistInstaller(downloadProgress, unzipProgress, stop);
            }
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = VCRedistInstallerPath(),
                    Arguments = "/install /quiet /norestart",
                    UseShellExecute = false,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                };
                using var VCRedistInstallationProcess = new Process { StartInfo = startInfo };
                VCRedistInstallationProcess.Start();
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"InstallVcRedist error: {e.Message}");
            }
        }
        IEnumerable<string> GetBinsUrlsForDependency()
        {
            yield return "https://github.com/nicehash/MinerDownloads/releases/download/v1.0/VC_redist.x64_2015_2019.7z";
        }
        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles("", new List<string> { VCRedistInstallerPath() });
        }
        public async Task DownloadVCRedistInstaller(IProgress<int> downloadProgress, IProgress<int> unzipProgress, CancellationToken stop)
        {
            var installingPluginBinsPath = VCRedistFolder();
            try
            {
                if (Directory.Exists(installingPluginBinsPath)) Directory.Delete(installingPluginBinsPath, true);
                Directory.CreateDirectory(installingPluginBinsPath);
                var installedBins = false;
                var urls = GetBinsUrlsForDependency();
                foreach (var url in urls)
                {
                    var downloadMinerBinsResult = await MinersDownloadManager.DownloadFileAsync(url, installingPluginBinsPath, "vc_bins", downloadProgress, stop);
                    var binsPackageDownloaded = downloadMinerBinsResult.downloadedFilePath;
                    var downloadMinerBinsOK = downloadMinerBinsResult.success;
                    if (!downloadMinerBinsOK || stop.IsCancellationRequested) return;
                    // unzip 
                    var binsUnzipPath = installingPluginBinsPath;
                    var unzipMinerBinsOK = await ArchiveHelpers.ExtractFileAsync(string.Empty, binsPackageDownloaded, binsUnzipPath, unzipProgress, stop);
                    if (stop.IsCancellationRequested) return;
                    if (unzipMinerBinsOK)
                    {
                        installedBins = true;
                        File.Delete(binsPackageDownloaded);
                        break;
                    }
                }
                if (!installedBins)
                {
                    Logger.Error(Tag, $"{Tag} not installed");
                }
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"Installation of {Tag} failed: ${e.Message}");
            }
        }
    }
}

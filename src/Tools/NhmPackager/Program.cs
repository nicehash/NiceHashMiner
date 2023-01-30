using log4net.Core;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static NhmPackager.PackagerFileDirectoryUtils;
using static NhmPackager.PackagerPaths;
using System.Net.Http;

namespace NhmPackager
{
    class Program
    {
        private static bool Exec7ZipCreatePasswordArchive(string password, string archiveName, string releasePath)
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "7z.exe",
                    Arguments = $"-p{password} a {archiveName} {releasePath}\\*",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            var ok = proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                Logger.Info("7z-pwd", line);
            }
            return ok && proc.ExitCode == 0;
        }

        private static async Task EnsurePackerBinsBundle()
        {
            var packerBins = GetRootPath("packer_bins_bundle");
            if (Directory.Exists(packerBins)) return;
            // download and clean scope
            {
                using var client = new HttpClient();
                using var s = await client.GetStreamAsync("https://github.com/nicehash/NHM_MinerPluginsDownloads/releases/download/v18.x/packer_bins_bundle.zip");
                using var fs = new FileStream(GetRootPath("packer_bins_bundle.zip"), FileMode.CreateNew);
                await s.CopyToAsync(fs);
            }
            if (FileHelpers.GetFileSHA256Checksum(GetRootPath("packer_bins_bundle.zip")) != "bd02ea33e8904c9f662dc6176c37badb7ea534cf6ce916be026b50eec498416d") throw new Exception("packer_bins_bundle.zip wrong checksum");
            ZipFile.ExtractToDirectory(GetRootPath("packer_bins_bundle.zip"), GetRootPath());
        }

        private static async Task AddPackerBinsBundleToPath()
        {
            await EnsurePackerBinsBundle();
            // Add common folder to path for launched processes
            var sevenZipPath = GetRootPath("packer_bins_bundle", "7z");
            var nsisPath = GetRootPath("packer_bins_bundle", "NSIS");
            if (!Directory.Exists(sevenZipPath)) throw new Exception($"packer_bins_bundle missing path {sevenZipPath}");
            if (!Directory.Exists(nsisPath)) throw new Exception($"packer_bins_bundle missing path {nsisPath}");
            const string pathKey = "PATH";
            var pathVar = Environment.GetEnvironmentVariable(pathKey);
            pathVar += $";{sevenZipPath};{nsisPath}";
            Environment.SetEnvironmentVariable(pathKey, pathVar);
        }

        private static void CopyInstallerScriptIfAvailable()
        {
            try
            {
                File.Copy(GetRootPath(@"..\", "prepare_installers.bat"), GetTemporaryWorkFolder("prepare_installers.bat"));
            }
            catch (Exception e)
            {
                Logger.Error("Main", $"CopyInstallerScriptIfAvailable error: {e}");
            }
        }

        // #1 copy release folder
        // #2 delete all json settings from release folder
        // #3 get versions from launcher and app (they must be equal)
        // #4 append version to app folder
        // #5 copy assets (EULA,...)
        // #6 create NSIS template '_files_to_pack\nsis'
        static async Task Main(string[] args)
        {
            try
            {
                var randomPart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var tmpWorkFolder = $"tmp_{randomPart}";
                Logger.ConfigureWithFile(GetRootPath($"{tmpWorkFolder}_log.txt"));
                Logger.ConfigureConsoleLogging(Level.Info);
                SetTemporaryWorkFolder(tmpWorkFolder);

                if (args.Contains("-info"))
                {
                    MinerPluginsPacker.CheckOnlinePlugins();
                    return;
                }

                await AddPackerBinsBundleToPath();

                // #1
                // assume we are in installer folder
                Logger.Info("Main", "Preparing Release START");
                ExecXCopy(GetRootPath(@"..\", "Release"), GetTemporaryWorkFolder("Release"));
                Logger.Info("Main", "Preparing Release: Removing redundant files");
                DeletePathIfExists(GetTemporaryWorkFolder("Release", "app", "win10-x64"));
                DeletePathIfExists(GetTemporaryWorkFolder("Release", "build_settings.json"));
                Logger.Info("Main", "Preparing Release Checking version and metadata");
                var (generatedTemplateLauncher, versionLauncher) = VersionInfoHelpers.GenerateVariableTemplate(GetTemporaryWorkFolder("Release", "NiceHashMiner.exe"));
                var (generatedTemplate, version) = VersionInfoHelpers.GenerateVariableTemplate(GetTemporaryWorkFolder("Release", "app", "app_nhm.exe"));
                if (generatedTemplateLauncher != generatedTemplate || versionLauncher != version)
                {
                    throw new Exception($"Launcher and App TAG or Version missmatch!!!\n{generatedTemplateLauncher} != {generatedTemplate} \n{versionLauncher} != {version}");
                }
                Logger.Info("Main", "Versions and metadata OK");
                var appDirOld = GetTemporaryWorkFolder("Release", "app");
                var appDirNew = GetTemporaryWorkFolder("Release", $"app_{version}");
                Logger.Info("Main", $"moving '{appDirOld}' to '{appDirNew}'");
                Directory.Move(appDirOld, appDirNew);
                ZipFile.ExtractToDirectory(GetRootPath("EULA.zip"), GetTemporaryWorkFolder("Release"));
                // TODO HERE EXECUTE CREATE UNINSTALER
                Logger.Info("Main", "Preparing Release DONE. EXECUTE SIG-STAGE #1 NOW. Press any key to continue\n\n");
                Console.ReadKey();

                Logger.Info("Main", "MinerPluginsPacker.Execute START");
                MinerPluginsPacker.Execute(GetTemporaryWorkFolder("Release", "PluginsToSign"));
                Logger.Info("Main", "MinerPluginsPacker.Execute DONE. Press any key to continue\n\n");
                Console.ReadKey();

                // run the plugins packer in the installer
                Logger.Info("Main", "PluginPackageBundler.ExecuteTask START");
                await PluginPackageBundler.ExecuteTask(MinerPluginsPacker.PreInstalledPlugins, NHM.MinerPluginToolkitV1.Checkers.SupportedMajorVersions);
                Logger.Info("Main", "PluginPackageBundler.ExecuteTask DONE. Press any key to continue\n\n");
                Console.ReadKey();

                Logger.Info("Main", "Moving 'miner_plugins' and 'plugins_packages' to 'Release'");
                Directory.Move(GetMinerPluginsPath(), GetTemporaryWorkFolder("Release", "miner_plugins"));
                Directory.Move(GetPluginsPackagesPath(), GetTemporaryWorkFolder("Release", "plugins_packages"));

                // #2 
                DeletePathIfExists(GetTemporaryWorkFolder("Release", "PluginsToSign"));

                // nhm_windows_1.9.2.18_testnetdev.zip
                // nhm_windows_1.9.2.18_testnet.zip
                // TODO create these settings instead of copying them
                var buildSettings = new List<BuildTag> { BuildTag.TESTNETDEV, BuildTag.TESTNET, BuildTag.PRODUCTION };
                foreach (var build in buildSettings)
                {
                    DeletePathIfExists(GetTemporaryWorkFolder("Release", "build_settings.json"));
                    var zipFileName = $"nhm_windows_{version}";
                    if (build != BuildTag.PRODUCTION)
                    {
                        zipFileName += $"_{build.ToString().ToLower()}";
                        File.Copy(GetRootPath("build_settings", $"build_settings_{build}.json"), GetTemporaryWorkFolder("Release", "build_settings.json"), true);
                    }
                    else
                    {
                        var password = $"nhm{version.Replace(".", "")}";
                        var zipFileNamePwd = $"nhm_windows_{version}_pw.zip";
                        Logger.Info("Main", $"Creating password protected {zipFileNamePwd} package password='{password}'");
                        Exec7ZipCreatePasswordArchive(password, GetTemporaryWorkFolder(zipFileNamePwd), GetTemporaryWorkFolder("Release"));
                    }
                    Logger.Info("Main", $"Creating {zipFileName}.zip package");
                    ZipFile.CreateFromDirectory(GetTemporaryWorkFolder("Release"), GetTemporaryWorkFolder($"{zipFileName}.zip"));
                    Logger.Info("Main", $"FINISHED {zipFileName}.zip package");
                }

                // #6 
                var filesToPackPath = GetTemporaryWorkFolder("_files_to_pack");
                RecreateDirectoryIfExists(filesToPackPath);
                // copy template and exe files
                ExecXCopy(GetRootPath("nsis_template"), Path.Combine(filesToPackPath, "nsis"));
                File.WriteAllText(Path.Combine(filesToPackPath, "nsis", "include_common", "packageDefsGenerated.nsh"), generatedTemplate, new UTF8Encoding(true));
                
                NSIS_Helpers.PrepareUninstaller(Path.Combine(filesToPackPath, "nsis"));
                Logger.Info("Main", "Uninstaller generated. EXECUTE SIG-STAGE #2 NOW. Press any key to continue\n\n");
                Console.ReadKey();

                NSIS_Helpers.Exec7ZipPackage(GetTemporaryWorkFolder("Release"), Path.Combine(filesToPackPath, "nsis", "dist"));
                NSIS_Helpers.CallMakeNSIS_Exe(Path.Combine(filesToPackPath, "nsis", "installer.nsi"));
                NSIS_Helpers.CallMakeNSIS_Exe(Path.Combine(filesToPackPath, "nsis", "updater.nsi"));
                File.Move(Path.Combine(filesToPackPath, "nsis", "installer.exe"), GetTemporaryWorkFolder($"nhm_windows_{version}.exe"));
                File.Move(Path.Combine(filesToPackPath, "nsis", "updater.exe"), GetTemporaryWorkFolder($"nhm_windows_updater_{version}.exe"));
                Logger.Info("Main", "'installer.exe' and 'updater.exe' generated. EXECUTE SIG-STAGE #3 NOW. Press any key to continue\n\n");
                Console.ReadKey();

                // move to the temp folder
                Logger.Info("Main", "Clean up temp files...");
                Directory.Delete(GetTemporaryWorkFolder("_files_to_pack"), true);
                Directory.Delete(GetTemporaryWorkFolder("Release"), true);
                Logger.Info("Main", "Finishing...");
                CopyInstallerScriptIfAvailable();
                Directory.Move(GetTemporaryWorkFolder(), GetRootPath($"nhm_windows_{version}_release_files"));
                Logger.Info("Main", "DONE! SUCCESS!!! Press any key to continue\n\n");
                Console.ReadKey();
                return;
            }
            catch (Exception e)
            {
                Logger.Error("Main", $"\t\t PROBLEM: {e.Message} {e.Message}");
                Logger.Error("Main", $"\t\t PROBLEM: {e.StackTrace}");
                Logger.Error("Main", "DONE! FAIL!!! Press any key to continue\n\n");
                Console.ReadKey();
            }
        }
    }
}

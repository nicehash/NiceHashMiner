using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NiceHashMiner.Mining.Plugins
{
    // ALL CAPS
    // This isn't really a plugin it just a hack to piggyback on the miner plugins downloader and file checker
    class VC_REDIST_x64_2015_DEPENDENCY_PLUGIN : IMinerPlugin, IntegratedPlugin, IPluginDependency, IBinaryPackageMissingFilesChecker, IMinerBinsSource
    {
        public static VC_REDIST_x64_2015_DEPENDENCY_PLUGIN Instance { get; } = new VC_REDIST_x64_2015_DEPENDENCY_PLUGIN();
        VC_REDIST_x64_2015_DEPENDENCY_PLUGIN() { }
        public string PluginUUID => "VC_REDIST_x64_2015";

        public Version Version => new Version(1, 0);
        public string Name => "VC_REDIST_x64_2015";

        public string Author => "stanko@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // return empty
            return new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
        }

        public bool IsPluginDependency { get; } = true;

        public bool Is3rdParty => false;

        #region IMinerPlugin stubs
        public IMiner CreateMiner()
        {
            return null;
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return false;
        }
        #endregion IMinerPlugin stubs


        public string VcRedistBinPath()
        {
            var binPath = Path.Combine(Paths.MinerPluginsPath(), PluginUUID, "bins", "vc_redist.x64.exe");
            return binPath;
        }

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles("", new List<string> { VcRedistBinPath() });
        }

        public void InstallVcRedist()
        {
            // TODO check if we need to run the insall
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = VcRedistBinPath(),
                    Arguments = "/q /norestart",
                    UseShellExecute = false,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                };
                using (var cudaDevicesDetection = new Process { StartInfo = startInfo })
                {
                    cudaDevicesDetection.Start();
                }
            }
            catch (Exception e)
            {
                Logger.Error("VC_REDIST_x64_2015_DEPENDENCY_PLUGIN", $"InstallVcRedist error: {e.Message}");
            }
        }

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrlsForPlugin()
        {
            yield return "https://github.com/nicehash/NiceHashMinerTest/releases/download/1.9.1.5/vc_redist.x64.exe.7z";
        }
    }
}

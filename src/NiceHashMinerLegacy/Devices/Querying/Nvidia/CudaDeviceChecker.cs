using NiceHashMiner.Configs;
using NiceHashMiner.Devices.Querying.Nvidia;
using System;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace NiceHashMiner.Devices
{
    public class CudaDeviceChecker
    {
        private const string Tag = "CudaDeviceChecker";

        private Timer _cudaCheckTimer;

        private readonly int _gpuCount;

        private readonly CudaQuery _cudaQuery;

        public CudaDeviceChecker()
        {
            _cudaQuery = new CudaQuery();
            if (_cudaQuery.TryQueryCudaDevices(out var devs))
            {
                _gpuCount = devs.Count;
            }
        }

        public void Start()
        {
            if (!ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost)
                return;

            _cudaCheckTimer = new Timer(60 * 1000);
            _cudaCheckTimer.Elapsed += CudaCheckTimerOnElapsed;
            _cudaCheckTimer.Start();
        }

        public void Stop()
        {
            _cudaCheckTimer.Stop();
        }

        private bool CheckDevicesMistmatch()
        {
            // this function checks if count of CUDA devices is same as it was on application start, reason for that is
            // because of some reason (especially when algo switching occure) CUDA devices are dissapiring from system
            // creating tons of problems e.g. miners stop mining, lower rig hashrate etc.

            if (!_cudaQuery.TryQueryCudaDevices(out var currentDevs))
            {
                Helpers.ConsolePrint(Tag, "Querying CUDA devs failed");
                return true;
            }

            var gpusNew = currentDevs.Count;

            Helpers.ConsolePrint("ComputeDeviceManager.CheckCount",
                "CUDA GPUs count: Old: " + _gpuCount + " / New: " + gpusNew);

            return gpusNew < _gpuCount;
        }

        private void CudaCheckTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!CheckDevicesMistmatch()) return;

            try
            {
                var onGpusLost = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\OnGPUsLost.bat")
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                Process.Start(onGpusLost);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", "OnGPUsMismatch.bat error: " + ex.Message);
            }
        }
    }
}

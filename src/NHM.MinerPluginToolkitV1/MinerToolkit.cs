using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// alias

namespace NHM.MinerPluginToolkitV1
{
    public static class MinerToolkit
    {
        public static bool HideMiningWindows { set; get; } = false;
        public static bool MinimizeMiningWindows { set; get; } = false;
        public static bool EnableSSLMining { set; get; } = false;

        public static (AlgorithmType first, AlgorithmType second, bool ok) GetFirstAndSecondAlgorithmType(this IEnumerable<MiningPair> mps)
        {
            if (mps == null) return (AlgorithmType.NONE, AlgorithmType.NONE, false);

            var firstSet = new HashSet<AlgorithmType>(mps.Select(pair => pair.Algorithm.IDs.First()));
            var secondSet = new HashSet<AlgorithmType>(mps.Select(pair => pair.Algorithm.IDs.Last()));
            var ok = firstSet.Count == 1 && secondSet.Count == 1;
            if (!ok) return (AlgorithmType.NONE, AlgorithmType.NONE, false);

            var first = firstSet.First();
            var second = secondSet.First();
            if (first == second) return (first, AlgorithmType.NONE, true);
            return (first, second, true);
        }

        public static (string name, bool ok) GetAlgorithmSettingsKey(this IEnumerable<MiningPair> mps)
        {
            return AlgorithmIDsToString(mps?.First()?.Algorithm?.IDs?.ToArray() ?? null);
        }

        public static (string name, bool ok) AlgorithmIDsToString(this AlgorithmType[] ids)
        {
            if (ids == null) return ("", false);
            var namesPairs = ids.Select(id => id.GetName());
            var ok = namesPairs.All(p => p.ok);
            var names = namesPairs.Where(p => p.ok).Select(p => p.name);
            var name = string.Join("+", names);
            return (name, ok);
        }

        public static (AlgorithmType[] ids, bool ok) StringToAlgorithmIDs(this string name)
        {
            (bool ok, AlgorithmType id) TryParse(string name) => (Enum.TryParse(name, out AlgorithmType id), id);
            var idPairs = name.Split('+').Select(TryParse);
            var ok = idPairs.All(p => p.ok);
            var ids = idPairs.Where(p => p.ok).Select(p => p.id).ToArray();
            return (ids, ok);
        }

        /// <summary>
        /// GetDevicesIDsInOrder returns ordered device IDs from <paramref name="mps"/>
        /// </summary>
        public static (int id, bool ok) GetOpenCLPlatformID(this IEnumerable<MiningPair> mps)
        {
            if (mps == null || mps.Count() == 0) return (-1, false);

            var openCLPlatformIDGroups = mps.Select(mp => mp.Device)
                .Where(dev => dev is AMDDevice)
                .Cast<AMDDevice>()
                .GroupBy(amd => amd.OpenCLPlatformID);

            var platformID = openCLPlatformIDGroups.Select(group => group.Key).OrderBy(key => key).FirstOrDefault();
            var isUnique = !(openCLPlatformIDGroups.Count() > 1);
            return (platformID, isUnique);
        }

        //shamelessly copied from StringsExt in extensions library/package
        /// <summary>
        /// GetStringAfter returns substring after first occurance of "<paramref name="after"/>" string
        /// </summary>
        public static string GetStringAfter(this string s, string after)
        {
            var index = s.IndexOf(after, StringComparison.Ordinal);
            return s.Substring(index + after.Length);
        }

        public static bool IsNeverHideMiningWindow(Dictionary<string, string> environmentVariables)
        {
            return environmentVariables?.ContainsKey("NEVER_HIDE_MINING_WINDOW") ?? false;
        }

        public static bool IsUseShellExecute(Dictionary<string, string> environmentVariables) => environmentVariables == null;

        /// <summary>
        /// CreateMiningProcess creates a new process used in mining
        /// </summary>
        public static Process CreateMiningProcess(string binPath, string workingDir, string commandLine, Dictionary<string, string> environmentVariables = null)
        {
            var miningHandle = new Process
            {
                StartInfo =
                {
                    // set params
                    FileName = binPath,
                    WorkingDirectory = workingDir,
                    Arguments = commandLine,
                    // common settings
                    UseShellExecute = IsUseShellExecute(environmentVariables),
                },
                EnableRaisingEvents = true,
            };

            // add environment if any
            if (environmentVariables != null)
            {
                foreach (var kvp in environmentVariables)
                {
                    var envName = kvp.Key;
                    var envValue = kvp.Value;
                    miningHandle.StartInfo.EnvironmentVariables[envName] = envValue;
                }
            }

            // WINDOW hiding or minimizing
            var isNeverHideMiningWindow = IsNeverHideMiningWindow(environmentVariables);
            var hideMiningWindow = HideMiningWindows && !isNeverHideMiningWindow;
            var minimizeMiningWindow = MinimizeMiningWindows || (HideMiningWindows && isNeverHideMiningWindow);
            if (minimizeMiningWindow)
            {
                miningHandle.StartInfo.CreateNoWindow = false;
                miningHandle.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                miningHandle.StartInfo.UseShellExecute = true && IsUseShellExecute(environmentVariables);
            }
            else if (hideMiningWindow)
            {
                miningHandle.StartInfo.CreateNoWindow = true;
                miningHandle.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                miningHandle.StartInfo.UseShellExecute = true && IsUseShellExecute(environmentVariables);
            }

            return miningHandle;
        }

        /// <summary>
        /// CreateBenchmarkProcess creates a new process used in benchmarks
        /// </summary>
        public static Process CreateBenchmarkProcess(string binPath, string workingDir, string commandLine, Dictionary<string, string> environmentVariables = null)
        {
            var benchmarkHandle = new Process
            {
                StartInfo =
                {
                    // set params
                    FileName = binPath,
                    WorkingDirectory = workingDir,
                    Arguments = commandLine,
                    // common settings
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true,
            };
            // add environment if any
            if (environmentVariables != null)
            {
                foreach (var kvp in environmentVariables)
                {
                    var envName = kvp.Key;
                    var envValue = kvp.Value;
                    benchmarkHandle.StartInfo.EnvironmentVariables[envName] = envValue;
                }
            }
            return benchmarkHandle;
        }

        /// <summary>
        /// WaitBenchmarkResult returns <see cref="BenchmarkResult"/> after one of 3 conditions is fullfiled.  
        /// Conditions are: Benchmark fails with error message, Benchmarks timeouts, Benchmark returns non-zero speed
        /// </summary>
        /// <param name="benchmarkProcess">Is the running <see cref="BenchmarkProcess"/></param>
        /// <param name="timeoutTime">Is the time after which we get timeout</param>
        /// <param name="delayTime">Is the delay time after which <paramref name="timeoutTime"/> starts counting</param>
        /// <param name="stop">Is the <see cref="CancellationToken"/> for stopping <paramref name="benchmarkProcess"/></param>
        public static async Task<BenchmarkResult> WaitBenchmarkResult(BenchmarkProcess benchmarkProcess, TimeSpan timeoutTime, TimeSpan delayTime, CancellationToken stop, CancellationToken stopAfterTicks)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var timeoutTimerTime = timeoutTime + delayTime;

            using var timeoutSource = new CancellationTokenSource(timeoutTimerTime);
            BenchmarkResult ret = null;
            var timeout = timeoutSource.Token;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeout, stop, stopAfterTicks);
            try
            {
                await Task.Delay(delayTime, linkedCts.Token);
                ret = await benchmarkProcess.Execute(linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Logger.Info("MinerToolkit", $"Error occured while waiting for benchmark result: {e.Message}");
                return new BenchmarkResult { ErrorMessage = e.Message };
            }

            // #1 if canceled return canceled
            if (stop.IsCancellationRequested)
            {
                Logger.Info("MinerToolkit", "Benchmark process was canceled by user");
                return new BenchmarkResult { ErrorMessage = "Cancelling per user request." };
            }
            if (ret == null) return new BenchmarkResult { ErrorMessage = "Benchmark result is null" };
            if (ret.HasNonZeroSpeeds() || !string.IsNullOrEmpty(ret.ErrorMessage)) return ret;
            if (timeout.IsCancellationRequested)
            {
                Logger.Info("MinerToolkit", "Benchmark process timed out");
                return new BenchmarkResult { ErrorMessage = "Operation timed out." };
            }
            return new BenchmarkResult();
        }

        /// <summary>
        /// IsSameAlgorithmType checks if the <see cref="Algorithm"/> <paramref name="a"/> and <see cref="Algorithm"/> <paramref name="b"/> are of same type
        /// </summary>
        public static bool IsSameAlgorithmType(Algorithm a, Algorithm b)
        {
            if (a == null || b == null) return false;
            return a.AlgorithmStringID == b.AlgorithmStringID;
        }


        #region Major version 16
        /// <summary>
        /// GetAlgorithmSingleType returns Tuple of single <see cref="AlgorithmType"/> and success status.
        /// </summary>
        public static Tuple<AlgorithmType, bool> GetAlgorithmSingleType(this IEnumerable<MiningPair> mps)
        {
            var algorithmTypes = mps.Select(pair => pair.Algorithm.IDs.First());
            var mustIncludeSingle = new HashSet<AlgorithmType>(algorithmTypes);
            if (mustIncludeSingle.Count == 1)
            {
                return Tuple.Create(mustIncludeSingle.First(), true);
            }
            return Tuple.Create(AlgorithmType.NONE, false);
        }

        /// <summary>
        /// GetAlgorithmSingleType returns Tuple of dual <see cref="AlgorithmType"/> and success status.
        /// </summary>
        public static Tuple<AlgorithmType, bool> GetAlgorithmDualType(this IEnumerable<MiningPair> mps)
        {
            if (mps.Select(pair => pair.Algorithm.IDs.Count).ToList()[0] == 1) return Tuple.Create(AlgorithmType.NONE, false);

            var algorithmTypes = mps.Select(pair => pair.Algorithm.IDs.Last());
            var mustIncludeDual = new HashSet<AlgorithmType>(algorithmTypes);
            return Tuple.Create(mustIncludeDual.First(), true);
        }

        /// <summary>
        /// GetAlgorithmCustomSettingKey returns first Algorithm name from MiningPairs
        /// </summary>
        public static string GetAlgorithmCustomSettingKey(this IEnumerable<MiningPair> mps)
        {
            if (mps.Count() == 0) return "";
            return mps.First().Algorithm.AlgorithmName;
        }

        /// <summary>
        /// GetAlgorithmPortsKey returns first Algorithm name from MiningPairs
        /// </summary>
        public static string GetAlgorithmPortsKey(this IEnumerable<MiningPair> mps)
        {
            if (mps.Count() == 0) return "";
            return mps.First().Algorithm.AlgorithmName;
        }
        #endregion Major version 16

    }
}

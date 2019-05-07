using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;

// alias
using TimersTimer = System.Timers.Timer;

namespace MinerPluginToolkitV1
{
    public static class MinerToolkit
    {
        /// <summary>
        /// Use DemoUser if the miner requires a network benchmark the plugin will blacklist users
        /// </summary>
        public static string DemoUserBTC => DemoUser.BTC;
        

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

        public static Tuple<AlgorithmType, bool> GetAlgorithmDualType(this IEnumerable<MiningPair> mps)
        {
            if (mps.Select(pair => pair.Algorithm.IDs.Count).ToList()[0] == 1) return Tuple.Create(AlgorithmType.NONE, false);

            var algorithmTypes = mps.Select(pair => pair.Algorithm.IDs.Last());
            var mustIncludeDual = new HashSet<AlgorithmType>(algorithmTypes);
            return Tuple.Create(mustIncludeDual.First(), true);
        }

        public static IEnumerable<string> GetDevicesIDsInOrder(this IEnumerable<MiningPair> mps)
        {
            var deviceIds = mps.Select(pair => pair.Device.ID).OrderBy(id => id).Select(id => id.ToString());
            return deviceIds;
        }

        //shamelessly copied from StringsExt in extensions library/package
        public static string GetStringAfter(this string s, string after)
        {
            var index = s.IndexOf(after, StringComparison.Ordinal);
            return s.Substring(index + after.Length);
        }

        private static int pow10(int power) => (int)Math.Pow(10, power);
        private static readonly Dictionary<char, int> _postfixes = new Dictionary<char, int>
        {
            {'k', pow10(3)},
            {'M', pow10(6)},
            {'G', pow10(9)},
            {'T', pow10(12)},
            {'P', pow10(15)},
            {'E', pow10(15)},
            {'Z', pow10(21)},
            {'Y', pow10(24)},
        };

        // TODO this will work on Sol-rates and G-rates but will skip prefixes
        // Hashrate and found pair
        public static Tuple<double, bool> TryGetHashrateAfter(this string s, string after)
        {
            if (!s.Contains(after))
            {
                return Tuple.Create(0d, false);;
            }

            var afterString = s.GetStringAfter(after).ToLower();
            var numString = new string(afterString
                .ToCharArray()
                .SkipWhile(c => !char.IsDigit(c))
                .TakeWhile(c => char.IsDigit(c) || c == '.')
                .ToArray());

            if (!double.TryParse(numString, NumberStyles.Float, CultureInfo.InvariantCulture, out var hash))
            {
                return Tuple.Create(0d, false);
            }

            var afterNumString = afterString.GetStringAfter(numString);
            for (var i = 0; i < afterNumString.Length - 1; ++i)
            {
                var c = afterNumString[i];
                if (!Char.IsLetter(c)) continue;
                var c2 = afterNumString[i + 1];

                foreach (var kvp in _postfixes)
                {
                    var postfix = Char.ToLower(kvp.Key);
                    var mult = kvp.Value;
                    if (postfix == c && 'h' == c2)
                    {
                        var hashrate = hash * mult;
                        return Tuple.Create(hashrate, true);
                    }
                }
            }
            return Tuple.Create(hash, true);
        }

        // TODO make one with Start NiceHashProcess
        public static Process CreateMiningProcess(string binPath, string workingDir, string commandLine, Dictionary<string, string> environmentVariables = null)
        {
            // TODO no handling of WINDOW hiding or minimizing
            //////if (IsNeverHideMiningWindow)
            //////{
            //////    P.StartInfo.CreateNoWindow = false;
            //////    if (ConfigManager.GeneralConfig.HideMiningWindows || ConfigManager.GeneralConfig.MinimizeMiningWindows)
            //////    {
            //////        P.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            //////        P.StartInfo.UseShellExecute = true;
            //////    }
            //////}
            //////else
            //////{
            //////    P.StartInfo.CreateNoWindow = ConfigManager.GeneralConfig.HideMiningWindows;
            //////}
            var miningHandle = new Process
            {
                StartInfo =
                {
                    // set params
                    FileName = binPath,
                    WorkingDirectory = workingDir,
                    Arguments = commandLine,
                    // common settings
                    UseShellExecute = environmentVariables == null,
                },
                EnableRaisingEvents = true, // TODO check out this one
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
            return miningHandle;
        }

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
                EnableRaisingEvents = true, // TODO check out this one
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

        public static async Task<BenchmarkResult> WaitBenchmarkResult(BenchmarkProcess benchmarkProcess, TimeSpan timeoutTime, TimeSpan delayTime, CancellationToken stop)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var timeoutTimerTime = timeoutTime + delayTime;

            using (var timeoutSource = new CancellationTokenSource(timeoutTimerTime))
            {
                BenchmarkResult ret = null;
                var timeout = timeoutSource.Token;
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeout, stop))
                {
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
                        return new BenchmarkResult{ ErrorMessage = e.Message };
                    }
                }
                // #1 if canceled return canceled
                if (stop.IsCancellationRequested)
                {
                    Logger.Info("MinerToolkit", "Benchmark process was canceled by user");
                    Console.WriteLine("Cancelling per user request.");
                    return new BenchmarkResult { ErrorMessage = "Cancelling per user request." };
                }

                if (ret != null && ret.HasNonZeroSpeeds()) return ret;
                if (timeout.IsCancellationRequested)
                {
                    Logger.Info("MinerToolkit", "Benchmark process timed out");
                    Console.WriteLine("Operation timed out.");
                    return new BenchmarkResult{ ErrorMessage = "Operation timed out." };
                }
            }
            return new BenchmarkResult();
        }
    }
}

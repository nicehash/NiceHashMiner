using System;
using System.Collections.Generic;
using System.Threading;

namespace NHMCore.Mining.Plugins
{
    internal sealed class MinerPluginInstallTask : IDisposable, IProgress<Tuple<PluginInstallProgressState, int>>
    {
        private readonly CancellationTokenSource _cancelInstall = new CancellationTokenSource();
        private readonly object _progressLock = new object();
        private Tuple<PluginInstallProgressState, int> _lastProgressValue = null;
        private List<IProgress<Tuple<PluginInstallProgressState, int>>> _progresses = new List<IProgress<Tuple<PluginInstallProgressState, int>>>();

        internal CancellationToken CancelInstallToken => _cancelInstall.Token;

        internal void TryCancelInstall()
        {
            try
            {
                _cancelInstall.Cancel();
            }
            catch
            { }
        }

        internal void AddProgress(IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            lock (_progressLock)
            {
                if (_lastProgressValue != null) progress?.Report(_lastProgressValue);
                if (!_progresses.Contains(progress)) _progresses.Add(progress);
            }
        }

        internal void RemoveProgress(IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            lock (_progressLock)
            {
                _progresses.Remove(progress);
            }
        }

        public void Dispose()
        {
            _cancelInstall.Dispose();
            lock (_progressLock)
            {
                _progresses.Clear();
            }
        }

        public void Report(Tuple<PluginInstallProgressState, int> value)
        {
            lock (_progressLock)
            {
                _lastProgressValue = value;
                foreach (var progress in _progresses)
                {
                    progress?.Report(value);
                }
            }
        }
    }
}

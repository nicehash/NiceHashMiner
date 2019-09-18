using NHM.Common;
using NHMCore;
using NHMCore.Mining;
using NHMCore.Utils;
using System;
using System.ComponentModel;

namespace NHM.Wpf.ViewModels.Models
{
    /// <summary>
    /// Wraps <see cref="AlgorithmContainer"/> for bench GUI purposes
    /// </summary>
    public class BenchAlgo : NotifyChangedBase, IDisposable
    {
        public AlgorithmContainer Algo { get; }

        /// <summary>
        /// Property that will be shown in speed column. 
        /// </summary>
        public string StatusString
        {
            get
            {
                if (Algo.InBenchmark)
                    return new string('.', _dotCount);
                if (Algo.IsBenchmarkPending)
                    return Translations.Tr("Waiting benchmark");
                if (Algo.BenchmarkErred)
                    return string.IsNullOrWhiteSpace(Algo.ErrorMessage) ? Translations.Tr("Error") : Algo.ErrorMessage;
                if (Algo.BenchmarkSpeed > 0)
                    return Helpers.FormatSpeedOutput(Algo.AnnotatedSpeeds);

                return Translations.Tr("none");
            }
        }

        private int _dotCount;

        public BenchAlgo(AlgorithmContainer algo)
        {
            Algo = algo;
            Algo.PropertyChanged += AlgoOnPropertyChanged;
        }

        private void AlgoOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Algo.InBenchmark) || e.PropertyName == nameof(Algo.IsBenchmarkPending) ||
                e.PropertyName == nameof(Algo.BenchmarkErred) || e.PropertyName == nameof(Algo.Speeds))
                OnPropertyChanged(nameof(StatusString));
        }

        public void IncrementTicker()
        {
            if (++_dotCount > 3) _dotCount = 1;
            OnPropertyChanged(nameof(StatusString));
        }

        public void Dispose()
        {
            Algo.PropertyChanged -= AlgoOnPropertyChanged;
        }
    }
}

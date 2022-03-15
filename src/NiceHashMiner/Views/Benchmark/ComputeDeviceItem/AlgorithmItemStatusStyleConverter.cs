using NHMCore.Mining;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    class AlgorithmItemStatusStyleConverter : IValueConverter
    {
        private static object AlgorithmStatusColor(AlgorithmStatus state)
        {
            switch (state)
            {
                case AlgorithmStatus.Error:
                case AlgorithmStatus.ErrorBenchmark:
                case AlgorithmStatus.ErrorNegativeSMA:
                case AlgorithmStatus.MissingSMA:
                case AlgorithmStatus.Unprofitable:
                case AlgorithmStatus.Unstable:
                    return Application.Current.FindResource("RedDangerColorBrush");
                case AlgorithmStatus.Benchmarked:
                case AlgorithmStatus.Mining:
                    return Application.Current.FindResource("NastyGreenBrush");
                case AlgorithmStatus.BenchmarkPending:
                case AlgorithmStatus.Benchmarking:
                case AlgorithmStatus.ReBenchmark:
                    return Application.Current.FindResource("PrimaryColorBrush");
                case AlgorithmStatus.NoBenchmark:
                    return Application.Current.FindResource("Gray1ColorBrush");
                case AlgorithmStatus.Disabled:
                    return Application.Current.FindResource("Gray2ColorBrush");
                default:
                    return Application.Current.FindResource("Gray1ColorBrush");
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlgorithmStatus state) return AlgorithmStatusColor(state);
            return Application.Current.FindResource("Gray1ColorBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

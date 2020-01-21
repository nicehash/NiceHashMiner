using NHMCore.Mining;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    class AlgorithmItemStatusStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlgorithmStatus state)
            {
                switch (state)
                {
                    case AlgorithmStatus.Error:
                    case AlgorithmStatus.ErrorBenchmark:
                    case AlgorithmStatus.ErrorNegativeSMA:
                    case AlgorithmStatus.MissingSMA:
                    case AlgorithmStatus.Unprofitable:
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
                }
            }
            return Application.Current.FindResource("Gray1ColorBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

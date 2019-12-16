using NHM.Common;
using NHM.Common.Enums;

namespace NHM.Wpf.ViewModels.Models
{
    public class BenchmarkSettings : NotifyChangedBase
    {

        private BenchmarkPerformanceType _selectedBenchmarkType = BenchmarkPerformanceType.Standard;
        public BenchmarkPerformanceType SelectedBenchmarkType {
            get => _selectedBenchmarkType;
            set
            {
                _selectedBenchmarkType = value;
                OnPropertyChanged(nameof(SelectedBenchmarkType));
            }
        }

        public BenchmarkPerformanceType Quick => BenchmarkPerformanceType.Quick;
        public BenchmarkPerformanceType Standard => BenchmarkPerformanceType.Standard;
        public BenchmarkPerformanceType Precise => BenchmarkPerformanceType.Precise;
    }
}

using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Models
{
    public class MinerData : NotifyChangedBase
    {
        public string Name { get; set; }
        public string UUID { get; set; }
        private IEnumerable<AlgoData> _algos;
        public IEnumerable<AlgoData> Algos
        {
            get => _algos;
            set
            {
                _algos = value;
                OnPropertyChanged(nameof(Algos));
            }
        }
    }
}

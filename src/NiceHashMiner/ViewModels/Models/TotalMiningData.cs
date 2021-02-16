using NHM.Common;
using NHMCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NiceHashMiner.ViewModels.Models
{
    /// <summary>
    /// Implementation of <see cref="IMiningData"/> that takes multuple <see cref="MiningData"/> objects
    /// and displays total information (for total rows of mining stats ListView).
    /// </summary>
    public class TotalMiningData : NotifyChangedBase, IMiningData, IDisposable
    {
        private readonly HashSet<MiningData> _datas;

        public string Name { get; } = Translations.Tr("Total");
        public string StateName { get; }

        public double Hashrate => _datas.Sum(d => d.Hashrate);
        public double Payrate => _datas.Sum(d => d.Payrate);
        public double FiatPayrate => _datas.Sum(d => d.FiatPayrate);
        public double PowerUsage => _datas.Sum(d => d.PowerUsage);
        public double PowerCost => _datas.Sum(d => d.PowerCost);
        public double Profit => _datas.Sum(d => d.Profit);

        public IEnumerable<Hashrate> Speeds
        {
            get
            {
                if (_datas.Count <= 0 || _datas.First().Speeds == null) return null;

                var ret = new List<Hashrate>(_datas.First().Speeds);

                foreach (var data in _datas.Skip(1))
                {
                    for (var i = 0; i < ret.Count; i++)
                    {
                        ret[i] += data.Speeds?.ElementAt(i) ?? 0;
                    }
                }

                return ret;
            }
        }

        public TotalMiningData(IEnumerable<MiningData> datas)
        {
            _datas = new HashSet<MiningData>(datas);
            StateName = _datas.FirstOrDefault()?.StateName;

            foreach (var data in _datas)
            {
                data.PropertyChanged += DataOnPropertyChanged;
            }
        }

        public TotalMiningData(params MiningData[] datas)
            : this((IEnumerable<MiningData>)datas)
        { }

        public void AddDevice(MiningData data)
        {
            // Will happen if dev already in hashset
            if (!_datas.Add(data)) return;

            data.PropertyChanged += DataOnPropertyChanged;

            OnPropertyChanged(nameof(Hashrate));
            OnPropertyChanged(nameof(Payrate));
            OnPropertyChanged(nameof(StateName));
            OnPropertyChanged(nameof(Speeds));
            OnPropertyChanged(nameof(FiatPayrate));
            OnPropertyChanged(nameof(PowerUsage));
            OnPropertyChanged(nameof(PowerCost));
            OnPropertyChanged(nameof(Profit));
        }

        private void DataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public void Dispose()
        {
            foreach (var data in _datas)
            {
                data.PropertyChanged -= DataOnPropertyChanged;
            }
        }
    }
}

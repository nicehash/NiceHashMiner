using NHM.Common;
using NHM.MinerPluginToolkitV1.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Models
{
    public class MinerELPData : NotifyChangedBase
    {
        public string Name { get; set; }
        public string UUID { get; set; }
        private List<string> _singleParams { get; set; } = new List<string>();
        public List<string> SingleParams
        {
            get { return _singleParams; }
            set
            {
                _singleParams = value;
                OnPropertyChanged(nameof(SingleParams));
            }
        }
        public void UpdateSingleParams(string singleTxt)
        {
            SingleParams = Regex.Replace(singleTxt, @"\s+", " ").Trim().Split(" ").ToList();
        }
        private List<(string name, string value)> _doubleParams { get; set; } = new List<(string name, string value)>();
        public List<(string name, string value)> DoubleParams
        {
            get { return _doubleParams; }
            set
            {
                _doubleParams = value;
                OnPropertyChanged(nameof(DoubleParams));
            }
        }
        public bool UpdateDoubleParams(string doubleTxt)
        {
            var doubles = Regex.Replace(doubleTxt, @"\s+", " ").Trim().Split(" ").ToList();
            var doubleParams = new List<(string name, string value)>();
            if (doubles.Count % 2 != 0)
            {
                DoubleParams = new List<(string name, string value)>();
                return false;
            }
            for(int i = 0; i < doubles.Count; i++)
            {
                if(i % 2 == 0 || i == 0) continue;
                doubleParams.Add((doubles[i - 1], doubles[i]));
            }
            DoubleParams = doubleParams;
            return true;
        }

        private IEnumerable<AlgoELPData> _algos;
        public IEnumerable<AlgoELPData> Algos
        {
            get => _algos;
            set
            {
                _algos = value;
                OnPropertyChanged(nameof(Algos));
            }
        }
        public string SingleParamString
        {
            get
            {
                return String.Join(' ', SingleParams) ?? "";
            }
        }
        public string DoubleParamString
        {
            get
            {
                return String.Join(' ', DoubleParams.Select(t => $"{t.name} {t.value}")) ?? "";
            }
        }
        public void IterateSubModelsAndSetELPs()
        {
            List<List<string>> minerParams = new List<List<string>>();
            foreach (var single in SingleParams)
            {
                minerParams.Add(new List<string>() { single });
            }
            foreach (var dbl in DoubleParams)
            {
                minerParams.Add(new List<string>() { dbl.name, dbl.value });
            }
            foreach (var algo in Algos)
            {
                List<List<string>> algoParams = new List<List<string>>();
                foreach (var single in algo.SingleParams)
                {
                    algoParams.Add(new List<string>() { single });
                }
                foreach (var dbl in algo.DoubleParams)
                {
                    algoParams.Add(new List<string>() { dbl.name, dbl.value });
                }
                var header = algo.Devices.FirstOrDefault();
                if (header == null || !header.IsDeviceDataHeader) continue;
                List<List<List<string>>> devParams = new List<List<List<string>>>();
                foreach (var dev in algo.Devices)
                {
                    if (dev.IsDeviceDataHeader) continue;
                    List<List<string>> oneDevParams = new List<List<string>>();
                    for(int i = 0; i < dev.ELPs.Count; i++)
                    {
                        var flagAndDelim = header.ELPs[i].ELP.Trim().Split(' ');
                        if(flagAndDelim.Length != 2) continue;
                        oneDevParams.Add(new List<string> { flagAndDelim[0], dev.ELPs[i].ELP, flagAndDelim[1] });
                    }
                    devParams.Add(oneDevParams);
                }
                algo.ParsedString = MinerExtraParameters.Parse(minerParams, algoParams, devParams);
            }
        }
    }
}

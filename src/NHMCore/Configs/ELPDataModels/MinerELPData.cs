using NHM.Common;
using NHM.MinerPluginToolkitV1.CommandLine;
using NHMCore.ApplicationState;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static NHM.MinerPluginToolkitV1.CommandLine.MinerConfigManager;

namespace NHMCore.Configs.ELPDataModels
{
    public class MinerELPData : NotifyChangedBase
    {
        public string Name { get; set; }
        public string UUID { get; set; }
        public string CombinedParams => $"{SingleParamString} {DoubleParamString}";
        public void UpdateProperties()
        {
            OnPropertyChanged(nameof(HasAnyContentSet));
        }
        public bool HasAnyContentSet
        {
            get
            {
                foreach(var algo in Algos)
                {
                    if (algo.AllCMDStrings.Select(str => str.command).Any(cmd => cmd.Trim() != String.Empty)) return true;
                }
                return false;
            }
        }
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
            OnPropertyChanged(nameof(CombinedParams));
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
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
                ELPManager.Instance.IterateSubModelsAndConstructELPs();
                return false;
            }
            for (int i = 0; i < doubles.Count; i++)
            {
                if (i % 2 == 0 || i == 0) continue;
                doubleParams.Add((doubles[i - 1], doubles[i]));
            }
            DoubleParams = doubleParams;
            OnPropertyChanged(nameof(CombinedParams));
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
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
        public string SingleParamString => String.Join(' ', SingleParams) ?? "";
        public string DoubleParamString => String.Join(' ', DoubleParams.Select(t => $"{t.name} {t.value}")) ?? "";

        public void ClearSingleParams()
        {
            SingleParams.Clear();
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
            OnPropertyChanged(nameof(CombinedParams));
        }
        public void ClearDoubleParams()
        {
            DoubleParams.Clear();
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
            OnPropertyChanged(nameof(CombinedParams));
        }
        public MiningState MiningState => MiningState.Instance;
    }
}

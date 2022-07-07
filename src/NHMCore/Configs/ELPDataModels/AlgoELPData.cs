using NHM.Common;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NHMCore.Configs.ELPDataModels
{
    public class AlgoELPData : NotifyChangedBase
    {
        public string Name { get; set; }
        private List<DeviceELPData> _devices = new List<DeviceELPData>();
        public AlgoELPData()
        {
            _devices = new List<DeviceELPData>();
            Devices.Add(new DeviceELPData(true));
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
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
            return true;
        }
        public List<DeviceELPData> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                OnPropertyChanged(nameof(Devices));
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
        private ObservableCollection<(string uuid, string command)> _allCMDStrings { get; set; } = new ObservableCollection<(string uuid, string command)>();
        public ObservableCollection<(string uuid, string command)> AllCMDStrings
        {
            get => _allCMDStrings;
            set
            {
                _allCMDStrings = value;
                OnPropertyChanged(nameof(_allCMDStrings));
                OnPropertyChanged(nameof(UniqueCMDs));
            }
        }
        public List<string> UniqueCMDs
        {
            get => AllCMDStrings.Select(t => t.command).Distinct().ToList();
        }
        public void ClearSingleParams()
        {
            SingleParams.Clear();
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
        }
        public void ClearDoubleParams()
        {
            DoubleParams.Clear();
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
        }
    }
}

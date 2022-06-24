using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Models
{
    public delegate void RescanEventHandler();
    public class AlgoELPData : NotifyChangedBase
    {
        public event RescanEventHandler InfoModified;
        protected virtual void OnModified()
        {
            if (InfoModified != null) InfoModified();
        }
        public string Name { get; set; }
        private List<DeviceELPData> _devices;
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
            for (int i = 0; i < doubles.Count; i++)
            {
                if (i % 2 == 0 || i == 0) continue;
                doubleParams.Add((doubles[i - 1], doubles[i]));
            }
            DoubleParams = doubleParams;
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
        private string _parsedString { get; set; } = string.Empty;
        public string ParsedString
        {
            get => _parsedString;
            set
            {
                _parsedString = value;
                OnPropertyChanged(nameof(ParsedString));
            }
        }
        public void NotifyMinerForELPRescan()
        {
            OnModified();
        }
        public void ClearSingleParams()
        {
            SingleParams.Clear();
        }
        public void ClearDoubleParams()
        {
            DoubleParams.Clear();
        }
    }
}

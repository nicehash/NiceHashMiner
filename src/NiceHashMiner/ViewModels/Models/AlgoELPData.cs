using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Models
{
    public class AlgoELPData : NotifyChangedBase
    {
        public string Name { get; set; }
        private List<DeviceELPData> _devices;
        public AlgoELPData()
        {
            _devices = new List<DeviceELPData>();
            Devices.Add(new DeviceELPData(true));
        }
        private List<string> _singleParams { get; set; }
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
        private List<(string name, string value)> _doubleParams { get; set; }
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
                if (i % 2 == 0 && i == 0) continue;
                doubleParams.Add((doubles[i], doubles[i - 1]));
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
    }
}

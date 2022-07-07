using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NHMCore.Configs.ELPDataModels
{
    public class DeviceELPData : NotifyChangedBase
    {
        public bool IsDeviceDataHeader { get; init; } = false;
        public string DeviceName { get; set; } = string.Empty;
        public string UUID { get; set; } = string.Empty;
        private ObservableCollection<DeviceELPElement> _ELPs = new ObservableCollection<DeviceELPElement>();
        public ObservableCollection<DeviceELPElement> ELPs
        {
            get { return _ELPs; }
            set
            {
                _ELPs = value;
                OnPropertyChanged(nameof(ELPs));
            }
        }

        private List<List<string>> _constructedELPs = new List<List<string>>();
        public List<List<string>> ConstructedELPs
        {
            get { return _constructedELPs; }
            set
            {
                _constructedELPs = value;
                OnPropertyChanged(nameof(ConstructedELPs));
            }
        }


        //public void RemoveELP(int column)
        //{
        //    ELPs.RemoveAt(column);
        //    OnPropertyChanged(nameof(ELPs));
        //}
        public void AddELP(string elp)
        {
            if (ELPs.Count == 0) ELPs.Add(new DeviceELPElement(!IsDeviceDataHeader) { ELP = elp });
            else ELPs[ELPs.Count - 1].ELP = elp;
            ELPs.Add(new DeviceELPElement(!IsDeviceDataHeader) { ELP = String.Empty });
        }
        public DeviceELPData(bool isHeader = false)
        {
            IsDeviceDataHeader = isHeader;
        }
        public DeviceELPData(string name, string uuid)
        {
            DeviceName = name;
            UUID = uuid;
        }
    }
}

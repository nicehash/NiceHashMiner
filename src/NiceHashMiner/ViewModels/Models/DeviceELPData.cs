using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NiceHashMiner.ViewModels.Models
{
    public delegate void EventHandler(object senderInput, EventArgs e, ELPEventActionType action, DeviceELPData data, DeviceELPElement elt);
    public class DeviceELPData : NotifyChangedBase
    {
        public event EventHandler ELPValueChanged;
        public bool IsDeviceDataHeader { get; init; } = false;
        public string DeviceName { get; set; } = string.Empty;
        public string UUID { get; set; } = string.Empty;
        public void OnELPValueChanged(object sender, EventArgs e, ELPEventActionType action, DeviceELPElement elt)
        {
            if (ELPValueChanged != null) ELPValueChanged(sender, e, action, this, elt);
        }
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
        public void RemoveELP(int column)
        {
            ELPs.RemoveAt(column);
            OnPropertyChanged(nameof(ELPs));
        }
        public void AddELP(string elp)
        {
            if(ELPs.Count == 0) ELPs.Add(new DeviceELPElement(!IsDeviceDataHeader) { ELP = elp });
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
        public void InputChanged(object sender, EventArgs e, ELPEventActionType action, DeviceELPElement elpE)
        {
            if (sender is not TextBox tb) return;
            if (elpE.HeaderType == HeaderType.Value)
            {
                var elp = ELPs.Where(elp => elp == elpE).FirstOrDefault();
                elp.ELP = tb.Text;
                OnPropertyChanged(nameof(ELPs));
            }
            OnELPValueChanged(sender, e, action, elpE);
        }
    }
}

using NHM.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NiceHashMiner.ViewModels.Models
{
    public delegate void EventHandler(object senderInput, EventArgs e, int action, DeviceELPData data, DeviceELPElement elt);
    public class DeviceELPData : NotifyChangedBase
    {
        public event EventHandler ELPValueChanged;
        public bool IsDeviceDataHeader { get; init; } = false;
        public string DeviceName { get; set; } = string.Empty;
        public string UUID { get; set; } = string.Empty;
        public void OnELPValueChanged(object sender, EventArgs e, int action, DeviceELPElement elt) // only in header item!!!
        {
            if (ELPValueChanged != null && IsDeviceDataHeader) ELPValueChanged(sender, e, action, this, elt);
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
        public DeviceELPData(bool isHeader = false)
        {
            IsDeviceDataHeader = isHeader;
        }
        public DeviceELPData(string name, string uuid)
        {
            DeviceName = name;
            UUID = uuid;
        }
        public void InputChanged(object sender, EventArgs e, int action, DeviceELPElement elpE)
        {
            if (sender is not TextBox tb) return;
            if (elpE.HeaderType == NHM.Common.Enums.HeaderType.Value)
            {
                var elp = ELPs.Where(elp => elp == elpE).FirstOrDefault();
                elp.ELP = tb.Text;
                return;
            }
            OnELPValueChanged(sender, e, action, elpE);
        }
    }
}

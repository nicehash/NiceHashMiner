using NHM.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Models
{
    public delegate void EventHandler(object senderInput, EventArgs e, string flag, string newText);
    public class DeviceELPData : NotifyChangedBase
    {
        public event EventHandler ELPValueChanged;
        public bool IsDeviceDataHeader { get; init; } = false;
        public string DeviceName { get; set; } = string.Empty;
        public string UUID { get; set; } = string.Empty;
        public void OnELPValueChanged(object sender, EventArgs e, string flag, string newText) // only in header item!!!
        {
            if (ELPValueChanged != null && IsDeviceDataHeader) ELPValueChanged(sender, e, flag, newText);
        }
        private ObservableCollection<string> _ELPs = new ObservableCollection<string>() { "33", "11", "44", "" };
        public ObservableCollection<string> ELPs
        {
            get { return _ELPs; }
            set
            {
                _ELPs = value;
                OnPropertyChanged(nameof(ELPs));
            }
        }

        public void RemoveELP(string flag)
        {
            ELPs.Remove(flag);
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
    }
}

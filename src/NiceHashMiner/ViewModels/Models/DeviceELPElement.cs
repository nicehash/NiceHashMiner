using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Models
{
    public delegate void EventHandlerTB(object senderInput, EventArgs e, int action, DeviceELPElement dataContext);
    public class DeviceELPElement : NotifyChangedBase
    {
        public event EventHandlerTB ELPValueChanged;
        public void OnELPValueChanged(object sender, EventArgs e, int action)
        {
            if (ELPValueChanged != null) ELPValueChanged(sender, e, action, this);
        }
        public DeviceELPElement(bool isValue = true)
        {
            HeaderType = isValue ? HeaderType.Value : HeaderType.FlagAndDelim;
        }
        public HeaderType HeaderType { get; init; } = HeaderType.Value;
        private string _elp { get; set; }
        public string ELP
        {
            get { return _elp; }
            set
            {
                _elp = value;
                OnPropertyChanged(nameof(ELP));
            }
        }
    }
}

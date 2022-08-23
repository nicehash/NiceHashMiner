using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs.ELPDataModels
{
    public class DeviceELPElement : NotifyChangedBase
    {
        public DeviceELPElement(bool isValue = true)
        {
            HeaderType = isValue ? HeaderType.Value : HeaderType.FlagAndDelim;
        }
        public HeaderType HeaderType { get; init; } = HeaderType.Value;
        private string _elp { get; set; } = String.Empty;
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

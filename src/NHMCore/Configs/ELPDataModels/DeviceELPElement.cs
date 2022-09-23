using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
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
        public bool IsHeader => HeaderType == HeaderType.FlagAndDelim;
        public HeaderType HeaderType { get; init; } = HeaderType.Value;
        private string _elp { get; set; } = String.Empty;
        public string ELP
        {
            get { return _elp; }
            set
            {
                _elp = value;
                OnPropertyChanged(nameof(ELP));
                OnPropertyChanged(nameof(FLAG));
                OnPropertyChanged(nameof(DELIM));
            }
        }
        private string _flag { get; set; } = string.Empty;
        public string FLAG
        {
            get { return _flag; }
            set
            {
                _flag = value;
                OnPropertyChanged(nameof(DELIM));
            }
        }
        private string _delim { get; set; } = string.Empty;
        public string DELIM
        {
            get { return _delim; }
            set
            {
                _delim = value;
                OnPropertyChanged(nameof(DELIM));
            }
        }
        public MiningState MiningState => MiningState.Instance;
    }
}

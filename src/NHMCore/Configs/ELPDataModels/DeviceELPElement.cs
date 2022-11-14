using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                _elp = value.Trim();
                var split = Regex.Replace(_elp, @"\s+", " ").Split(' ');
                if (IsHeader && split.Length == 2)
                {
                    _flag = split[0];
                    _delim = split[1];
                    if (_delim == "$ws$") _delim = " ";
                    OnPropertyChanged(nameof(FLAG));
                    OnPropertyChanged(nameof(DELIM));
                }
                OnPropertyChanged(nameof(ELP));
            }
        }
        public void SafeSetELP()
        {
            if(_delim == " ") _delim = "$ws$";
            ELP = $"{_flag} {_delim}";
            OnPropertyChanged(nameof(ELP));
        }
        private string _flag { get; set; } = string.Empty;
        public string FLAG
        {
            get { return _flag; }
            set
            {
                _flag = value.Trim();
                OnPropertyChanged(nameof(FLAG));
            }
        }
        private string _delim { get; set; } = string.Empty;
        public string DELIM
        {
            get
            {
                if (_delim == "$ws$") _delim = " ";
                return _delim; 
            }
            set
            {
                _delim = value;
                OnPropertyChanged(nameof(DELIM));
            }
        }
        public MiningState MiningState => MiningState.Instance;
    }
}

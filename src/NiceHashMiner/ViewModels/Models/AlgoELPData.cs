using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.ViewModels.Models
{
    public class AlgoELPData
    {
        public string Name { get; set; }
        private List<DeviceELPData> _devices;
        public AlgoELPData()
        {
            _devices = new List<DeviceELPData>();
            Devices.Add(new DeviceELPData(true));
        }
        public List<DeviceELPData> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                //todo onpropertychanged
            }
        }
    }
}

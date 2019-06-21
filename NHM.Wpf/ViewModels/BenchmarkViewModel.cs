using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels
{
    internal class BenchmarkViewModel
    {
        internal class FakeDevice
        {
            private bool _enabled;
            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;

                }
            }

            public string Name { get; }

            public FakeDevice(string name)
            {
                Name = name;
            }
        }

        public ObservableCollection<FakeDevice> Devices { get; }

        public BenchmarkViewModel()
        {
            Devices = new ObservableCollection<FakeDevice>();
        }

        public void RefreshData()
        {
            Devices.Add(new FakeDevice("CPU"));
            Devices.Add(new FakeDevice("GPU"));
        }
    }
}

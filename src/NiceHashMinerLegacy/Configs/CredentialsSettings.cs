using NiceHashMiner.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Configs
{
    public class CredentialsSettings : INotifyPropertyChanged
    {
        public static CredentialsSettings Instance { get; } = new CredentialsSettings();

        private CredentialsSettings()
        {
            _stringProps = new NotifyPropertyChangedHelper<string>(NotifyPropertyChanged);
            BitcoinAddress = "";
            WorkerName = "worker1";
        }
        private readonly NotifyPropertyChangedHelper<string> _stringProps;

        public string BitcoinAddress
        {
            get => _stringProps.Get(nameof(BitcoinAddress));
            set => _stringProps.Set(nameof(BitcoinAddress), value);
        }
        public string WorkerName
        {
            get => _stringProps.Get(nameof(WorkerName));
            set => _stringProps.Set(nameof(WorkerName), value);
        }

        public bool IsCredentialsValid
        {
            get => CredentialValidators.ValidateBitcoinAddress(BitcoinAddress);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}

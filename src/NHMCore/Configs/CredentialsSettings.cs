using NHM.Common;
using NHMCore.Utils;

namespace NHMCore.Configs
{
    public class CredentialsSettings : NotifyChangedBase
    {
        public static CredentialsSettings Instance { get; } = new CredentialsSettings();

        private CredentialsSettings()
        {
            _stringProps = new NotifyPropertyChangedHelper<string>(OnPropertyChanged);
            _boolProps = new NotifyPropertyChangedHelper<bool>(OnPropertyChanged);
            BitcoinAddress = "";
            WorkerName = "worker1";
            RigGroup = "";
        }
        // TODO maybe this should be removed 
        private readonly NotifyPropertyChangedHelper<string> _stringProps;
        private readonly NotifyPropertyChangedHelper<bool> _boolProps;

        public void SetBitcoinAddress(string btc)
        {
            BitcoinAddress = btc;
        }

        public string BitcoinAddress
        {
            get => _stringProps.Get(nameof(BitcoinAddress));
            internal set
            {
                _stringProps.Set(nameof(BitcoinAddress), value);
                IsBitcoinAddressValid = CredentialValidators.ValidateBitcoinAddress(BitcoinAddress);
                OnPropertyChanged(nameof(IsCredentialValid));
                OnPropertyChanged(nameof(IsBitcoinAddressValid));
            }
        }
        public string WorkerName
        {
            get => _stringProps.Get(nameof(WorkerName));
            internal set
            {
                _stringProps.Set(nameof(WorkerName), value);
                IsWorkerNameValid = CredentialValidators.ValidateWorkerName(WorkerName);
                OnPropertyChanged(nameof(IsCredentialValid));
            }
        }

        public string RigGroup
        {
            get => _stringProps.Get(nameof(RigGroup));
            internal set => _stringProps.Set(nameof(RigGroup), value);
        }

        public bool _isBitcoinAddressValid = false;
        public bool IsBitcoinAddressValid
        {
            get => _isBitcoinAddressValid;
            private set
            {
                _isBitcoinAddressValid = value;
                OnPropertyChanged(nameof(IsBitcoinAddressValid));
            }
        }

        public bool IsWorkerNameValid
        {
            get => _boolProps.Get(nameof(IsWorkerNameValid));
            private set => _boolProps.Set(nameof(IsWorkerNameValid), value);
        }

        public bool IsCredentialValid => IsBitcoinAddressValid && IsWorkerNameValid;

        //C#7
        public (string btc, string worker, string group) GetCredentials()
        {
            return (BitcoinAddress.Trim(), WorkerName.Trim(), RigGroup.Trim());
        }
    }
}

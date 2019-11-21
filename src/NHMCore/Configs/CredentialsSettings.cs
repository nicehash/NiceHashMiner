using NHM.Common;
using NHMCore.Utils;
using System;
using System.ComponentModel;

namespace NHMCore.Configs
{
    public class CredentialsSettings : NotifyChangedBase
    {
        public static CredentialsSettings Instance { get; } = new CredentialsSettings();

        private CredentialsSettings()
        {
            _stringProps = new NotifyPropertyChangedHelper<string>(OnPropertyChanged);
            BitcoinAddress = "";
            WorkerName = "worker1";
        }
        // TODO maybe this should be removed 
        private readonly NotifyPropertyChangedHelper<string> _stringProps;

        public string BitcoinAddress
        {
            get => _stringProps.Get(nameof(BitcoinAddress));
            internal set => _stringProps.Set(nameof(BitcoinAddress), value);
        }
        public string WorkerName
        {
            get => _stringProps.Get(nameof(WorkerName));
            internal set => _stringProps.Set(nameof(WorkerName), value);
        }

        public string RigGroup
        {
            get => _stringProps.Get(nameof(RigGroup));
            internal set => _stringProps.Set(nameof(RigGroup), value);
        }

        public bool IsCredentialsValid
        {
            get => CredentialValidators.ValidateBitcoinAddress(BitcoinAddress);
        }
    }
}

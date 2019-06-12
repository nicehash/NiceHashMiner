using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Configs
{
    public class CredentialsSettings
    {
        public static CredentialsSettings Instance { get; } = new CredentialsSettings();

        private CredentialsSettings()
        { }


        public string BitcoinAddress { get; set; } = "";
        public string WorkerName { get; set; } = "worker1";

        public bool IsCredentialsValid
        {
            get => CredentialValidators.ValidateBitcoinAddress(BitcoinAddress);
        }
    }
}

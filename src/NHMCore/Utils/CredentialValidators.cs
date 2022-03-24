using NHM.Common;
using NHM.Common.Enums;

namespace NHMCore.Utils
{
    public static class CredentialValidators
    {
        public static bool ValidateBitcoinAddress(string address)
        {
            return NHM.CredentialValidators.CredentialValidators.ValidateBitcoinAddress(address, BuildOptions.BUILD_TAG == BuildTag.PRODUCTION);
        }

        public static bool ValidateWorkerName(string workername) => NHM.CredentialValidators.CredentialValidators.ValidateWorkerName(workername);
    }
}


namespace NHM.CredentialValidators
{
    public static class CredentialValidators
    {
        public static bool ValidateWorkerName(string workername) => WorkernameValidator.ValidateWorkerName(workername);

        public static bool ValidateBitcoinAddress(string address, bool isProduction = true, bool ignoreValidation = false)
        {
            if (ignoreValidation) return !string.IsNullOrEmpty(address) && !string.IsNullOrWhiteSpace(address);
            return ValidateBitcoinAddressBase58(address, isProduction)
                || ValidateBitcoinAddressBech32(address, isProduction)
                || ValidateMiningAddress(address, isProduction);
        }

        private static bool IsInvalidString(string address) => string.IsNullOrEmpty(address) || string.IsNullOrWhiteSpace(address);

        public static bool ValidateMiningAddress(string address, bool isProduction = true)
        {
            bool isProductionValidPrefix() => isProduction && address.StartsWith("NH");
            bool isTestnetValidPrefix() => !isProduction && address.StartsWith("PT");
            if (IsInvalidString(address)) return false;
            if (!isProductionValidPrefix() && !isTestnetValidPrefix()) return false;
            return BTC_Base58.ValidateBitcoinAddress(address.Substring(2));
        }

        public static bool ValidateBitcoinAddressBase58(string address, bool isProduction = true)
        {
            if (IsInvalidString(address)) return false;
            if (!isProduction) return address.StartsWith("2") && BTC_Base58.ValidateBitcoinAddress(address);
            return !address.StartsWith("2") && BTC_Base58.ValidateBitcoinAddress(address);
        }

        public static bool ValidateBitcoinAddressBech32(string address, bool isProduction = true)
        {
            if (IsInvalidString(address)) return false;
            if (!isProduction) return address.StartsWith("tb1") && BTC_Bech32.ValidateBitcoinAddress(address);
            return address.StartsWith("bc1") && BTC_Bech32.ValidateBitcoinAddress(address);
        }
    }
}

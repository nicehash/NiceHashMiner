using System;
using System.Collections.Generic;
using System.Text;

namespace NHM.CredentialValidators
{
    internal static class BTC_Bech32
    {
        internal static bool ValidateBitcoinAddress(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address) || string.IsNullOrWhiteSpace(address)) return false;
                var addressLength = address.Length - 3;
                if (addressLength < 39 || addressLength > 59) return false;
                var a = Bech32_Csharp.Converter.DecodeBech32(address, out var witVer, out var p2pkh, out var isMainnet);
                string b = Bech32_Csharp.Converter.EncodeBech32(witVer, a, true, isMainnet);
                return address == b;
            }
            catch
            {
                return false;
            }
        }
    }
}

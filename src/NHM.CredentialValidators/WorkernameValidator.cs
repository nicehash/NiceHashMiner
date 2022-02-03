using System.Text.RegularExpressions;

namespace NHM.CredentialValidators
{
    internal static class WorkernameValidator
    {
        private static Regex _IsAlphaNumeric = new Regex(@"^[a-zA-Z0-9\s,]*$");
        private static bool IsAlphaNumeric(string strToCheck) => _IsAlphaNumeric.IsMatch(strToCheck);
        private const int MAX_WORKERNAME_LENGTH = 15;

        internal static bool ValidateWorkerName(string workername)
        {
            return !string.IsNullOrEmpty(workername)
                && !string.IsNullOrWhiteSpace(workername)
                && workername.Length <= MAX_WORKERNAME_LENGTH
                && IsAlphaNumeric(workername)
                && !workername.Contains(" ");
        }
    }
}

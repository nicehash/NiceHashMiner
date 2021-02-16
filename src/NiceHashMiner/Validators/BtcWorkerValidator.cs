using NHMCore;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NiceHashMiner.Validators
{
    // These classes are used for the BTC and workername textboxes for validation.

    public class BtcAddressValidator : ValidatorBase<string>
    {
        public override ValidationResult Validate(string value, CultureInfo cultureInfo)
        {
            var result = Task.Run(async () => await ApplicationStateManager.SetBTCIfValidOrDifferent(value)).Result;

            if (result == ApplicationStateManager.SetResult.INVALID)
            {
                return new ValidationResult(false, Translations.Tr("Invalid Bitcoin address! {0} will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!", NHMProductInfo.Name));
            }

            return ValidationResult.ValidResult;
        }
    }

    public class WorkerValidator : ValidatorBase<string>
    {
        public override ValidationResult Validate(string value, CultureInfo cultureInfo)
        {
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(value);

            if (result == ApplicationStateManager.SetResult.INVALID)
            {
                return new ValidationResult(false, Translations.Tr("Invalid workername!\n\nPlease enter a valid workername (Aa-Zz, 0-9, up to 15 character long)."));
            }

            return ValidationResult.ValidResult;
        }
    }
}

using System.Globalization;
using System.Windows.Controls;

namespace NHM.Wpf.ViewModels.Validators
{
    public abstract class ValidatorBase<T> : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is T t))
            {
                return new ValidationResult(false, $"{nameof(value)} must be a {typeof(T)}");
            }

            return Validate(t, cultureInfo);
        }

        public abstract ValidationResult Validate(T value, CultureInfo cultureInfo);
    }
}

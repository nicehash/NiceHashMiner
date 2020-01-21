using System.Globalization;
using System.Windows.Controls;

namespace NiceHashMiner.Validators
{
    /// <summary>
    /// Type-safe helper base class for a <see cref="ValidationRule"/> implementation.
    /// </summary>
    /// <typeparam name="T">The input type for validation.</typeparam>
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

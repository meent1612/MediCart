using System;
using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MinimumFutureDateAttribute : ValidationAttribute
    {
        public int MinimumDays { get; }

        public MinimumFutureDateAttribute(int minimumDays = 30)
            : base($"Expiry date must be at least {minimumDays} days from today.")
        {
            MinimumDays = minimumDays;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            var targetDate = DateTime.Today.AddDays(MinimumDays);

            if (value is DateOnly dateOnlyVal)
            {
                var minDateOnly = DateOnly.FromDateTime(targetDate);
                if (dateOnlyVal < minDateOnly)
                {
                    return new ValidationResult(ErrorMessageString);
                }
            }
            else if (value is DateTime dateTimeVal)
            {
                if (dateTimeVal.Date < targetDate)
                {
                    return new ValidationResult(ErrorMessageString);
                }
            }

            return ValidationResult.Success;
        }
    }
}

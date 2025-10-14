using System.ComponentModel.DataAnnotations;

namespace GeneralizeQueries.Api.Validation;

public class BooleanStringAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is string strValue)
            if (strValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                return ValidationResult.Success!;

        return new ValidationResult(
            ErrorMessage ?? $"The field {validationContext.DisplayName} must be either 'true' or 'false'.");
    }
}
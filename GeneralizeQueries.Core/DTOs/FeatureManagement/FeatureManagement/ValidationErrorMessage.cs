namespace GeneralizeQueries.Core.Models.Validation;

public class ValidationErrorMessage
{
    public string Message { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
}
using GeneralizeQueries.Core.Models.Validation;

namespace GeneralizeQueries.Core.Models.FeatureManagement;

public class FeatureManagementDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ValidationErrorMessage> Errors { get; set; } = new();
}
using GeneralizeQueries.Core.Models.Validation;

namespace GeneralizeQueries.Core.Models.FeatureManagement;

public class FeatureGroup
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<FeatureItem> FeatureItems { get; set; } = new();
    public List<ValidationErrorMessage> Errors { get; set; } = new();
}

public class FeatureItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UniqName { get; set; } = string.Empty;
    public List<string> Commands { get; set; } = new();
    public string CollectionName { get; set; } = string.Empty;
}
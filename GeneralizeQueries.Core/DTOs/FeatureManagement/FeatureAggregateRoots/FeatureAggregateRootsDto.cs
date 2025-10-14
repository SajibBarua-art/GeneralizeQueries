namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

public class FeatureAggregateRootsDto
{
    public string Name { get; set; } = string.Empty;
    public string? UniqName { get; set; }
    public string? TagName { get; set; }
    public List<string> Commands { get; set; } = new();
}
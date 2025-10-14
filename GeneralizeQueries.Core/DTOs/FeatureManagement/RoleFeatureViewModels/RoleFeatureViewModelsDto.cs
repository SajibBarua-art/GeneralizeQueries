namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

public class RoleFeatureViewModelsDto
{
    public string Name { get; set; } = string.Empty;
    public string? TagName { get; set; }
    public List<string> Commands { get; set; } = new();
}
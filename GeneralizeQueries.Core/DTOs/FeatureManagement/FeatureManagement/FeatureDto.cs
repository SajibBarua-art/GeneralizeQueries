namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

public class FeatureDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required List<string> Commands { get; set; }
    public required string TagName { get; set; }
    public required string UniqName { get; set; }
}
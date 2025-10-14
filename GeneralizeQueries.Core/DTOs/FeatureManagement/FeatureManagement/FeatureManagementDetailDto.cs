namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

public class FeatureManagementDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public FeatureAggregateRootsDto? FeatureAggregateRoot { get; set; }
    public FeatureViewModelsDto? FeatureViewModel { get; set; }
    public RoleFeatureViewModelsDto? RoleFeatureViewModel { get; set; }
}
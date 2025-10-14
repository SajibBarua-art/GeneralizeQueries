using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

public class CreateRoleFeatureViewModelsDto
{
    [Required]
    [MaxLength(200)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    [JsonPropertyName("tagName")]
    public string? TagName { get; set; }

    [Required]
    [JsonPropertyName("verticalId")]
    public Guid VerticalId { get; set; }

    [Required]
    [MaxLength(100)]
    [JsonPropertyName("serviceId")]
    public string ServiceId { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "At least one role is required")]
    [JsonPropertyName("rolesAndIdsAllowedToRead")]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = new();

    [Required]
    [MinLength(1, ErrorMessage = "At least one command is required")]
    [JsonPropertyName("commands")]
    public List<string> Commands { get; set; } = new();

    [MaxLength(10)]
    [JsonPropertyName("language")]
    public string Language { get; set; } = "en-US";

    [JsonPropertyName("metadata")] public Dictionary<string, object>? Metadata { get; set; }

    [JsonPropertyName("tags")] public string? Tags { get; set; }

    [JsonPropertyName("tenantId")] public Guid? TenantId { get; set; }
}
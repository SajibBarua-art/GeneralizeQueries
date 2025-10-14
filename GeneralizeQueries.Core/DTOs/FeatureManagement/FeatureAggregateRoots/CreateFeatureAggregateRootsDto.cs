using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

public class CreateFeatureAggregateRootsDto
{
    [Required] [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [Required]
    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; } = null!;

    [Required]
    [JsonPropertyName("language")]
    public string Language { get; set; } = null!;

    [JsonPropertyName("metadata")] public Dictionary<string, object>? Metadata { get; set; }

    [Required]
    [JsonPropertyName("lastUpdatedBy")]
    public string LastUpdatedBy { get; set; } = null!;

    [Required]
    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = null!;

    [JsonPropertyName("tags")] public string? Tags { get; set; }

    [Required]
    [JsonPropertyName("verticalId")]
    public string VerticalId { get; set; } = null!;

    [Required]
    [JsonPropertyName("serviceId")]
    public string ServiceId { get; set; } = null!;

    [JsonPropertyName("isMarkedToDelete")] public bool IsMarkedToDelete { get; set; } = false;

    [JsonPropertyName("version")] public int Version { get; set; } = 0;

    [JsonPropertyName("rolesAndIdsAllowedToRead")]
    public List<string>? RolesAndIdsAllowedToRead { get; set; }

    [JsonPropertyName("readAuthorizations")]
    public List<string>? ReadAuthorizations { get; set; }

    [JsonPropertyName("writeAuthorizations")]
    public List<string>? WriteAuthorizations { get; set; }

    [Required] [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [Required]
    [JsonPropertyName("uniqName")]
    public string UniqName { get; set; } = null!;

    [JsonPropertyName("commands")] public List<string> Commands { get; set; } = [];

    [Required]
    [JsonPropertyName("tagName")]
    public string TagName { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

public class UpdateRoleFeatureViewModelsDto
{
    [Required]
    [JsonPropertyName("language")]
    public string Language { get; set; } = null!;

    [JsonPropertyName("metadata")] public Dictionary<string, object>? Metadata { get; set; }

    [Required]
    [JsonPropertyName("lastUpdatedBy")]
    public string LastUpdatedBy { get; set; } = null!;

    [JsonPropertyName("tags")] public string? Tags { get; set; }

    [JsonPropertyName("isMarkedToDelete")] public bool IsMarkedToDelete { get; set; }

    [JsonPropertyName("rolesAndIdsAllowedToRead")]
    public List<string>? RolesAndIdsAllowedToRead { get; set; }

    [JsonPropertyName("readAuthorizations")]
    public List<string>? ReadAuthorizations { get; set; }

    [JsonPropertyName("writeAuthorizations")]
    public List<string>? WriteAuthorizations { get; set; }

    [Required] [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("tagName")] public string? TagName { get; set; }

    [JsonPropertyName("commands")] public List<string> Commands { get; set; } = [];
}
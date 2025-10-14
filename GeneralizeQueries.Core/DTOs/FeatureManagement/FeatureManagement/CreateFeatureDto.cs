using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

public class CreateFeatureDto
{
    [Required]
    [MaxLength(200)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [JsonPropertyName("uniqName")]
    public string UniqName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [JsonPropertyName("tagName")]
    public string TagName { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "At least one command is required")]
    [JsonPropertyName("commands")]
    public List<string> Commands { get; set; } = new();
}
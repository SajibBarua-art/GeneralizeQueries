using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GeneralizeQueries.Api.Validation;

namespace GeneralizeQueries.Api.DTOs;

// DTO for the POST request body
public class CreateQueryTemplateDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!; // The client provides the string ID on creation

    [Required]
    [JsonPropertyName("template")]
    public TemplateDetailsDto Template { get; set; } = null!;

    [Required]
    [JsonPropertyName("rolesAndIdsAllowedToRead")]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = [];

    [Required]
    [BooleanString]
    [JsonPropertyName("isMarkedToDelete")]
    public string IsMarkedToDelete { get; set; } = "false";
}
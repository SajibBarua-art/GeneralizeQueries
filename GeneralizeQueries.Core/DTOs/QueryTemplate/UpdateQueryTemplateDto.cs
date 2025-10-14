using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GeneralizeQueries.Api.Validation;

namespace GeneralizeQueries.Api.DTOs;

// DTO for the PUT request body. Note the absence of the Id field.
public class UpdateQueryTemplateDto
{
    [Required]
    [JsonPropertyName("template")]
    public TemplateDetailsDto Template { get; set; } = null!;

    [Required]
    [JsonPropertyName("rolesAndIdsAllowedToRead")]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = [];

    [Required]
    [JsonPropertyName("isMarkedToDelete")]
    [BooleanString]
    public string IsMarkedToDelete { get; set; } = null!;
}
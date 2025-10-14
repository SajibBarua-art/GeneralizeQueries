using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GeneralizeQueries.Api.Validation;

namespace GeneralizeQueries.Api.DTOs;

// This is the main DTO for the entire document
public class QueryTemplateDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("template")] public TemplateDetailsDto Template { get; set; } = null!;

    [JsonPropertyName("rolesAndIdsAllowedToRead")]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = [];

    [JsonPropertyName("isMarkedToDelete")]
    [BooleanString]
    public string IsMarkedToDelete { get; set; } = null!; // Note: It's a string "false", not a boolean
}

// This DTO represents the nested "Template" object
public class TemplateDetailsDto : IValidatableObject
{
    [JsonPropertyName("source")] public string Source { get; set; } = null!;

    // Optional: populated when IsDynamicFilter = "true"
    [JsonPropertyName("dynamicFilter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? DynamicFilter { get; set; }

    // Optional: populated when IsDynamicFilter = "false"
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Filter { get; set; }

    [JsonPropertyName("fields")] public List<string> Fields { get; set; } = [];

    [JsonPropertyName("allowedSorts")] public List<string> AllowedSorts { get; set; } = [];

    [JsonPropertyName("locale")] public string Locale { get; set; } = null!;

    // In your JSON, this is a string "true", not a boolean true.
    // If it were a boolean, the type would be `bool`.
    [JsonPropertyName("isDynamicFilter")]
    [BooleanString]
    public string IsDynamicFilter { get; set; } = null!;

    [JsonPropertyName("countOnly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? CountOnly { get; set; }

    // Custom validation logic
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsDynamicFilter == "true")
        {
            if (DynamicFilter == null || DynamicFilter.Count == 0)
                yield return new ValidationResult(
                    "DynamicFilter is required when IsDynamicFilter is 'true'.",
                    new[] { nameof(DynamicFilter) });

            if (Filter != null)
                yield return new ValidationResult(
                    "Filter should not be provided when IsDynamicFilter is 'true'.",
                    new[] { nameof(Filter) });
        }
        else if (IsDynamicFilter == "false")
        {
            if (string.IsNullOrWhiteSpace(Filter))
                yield return new ValidationResult(
                    "Filter is required when IsDynamicFilter is 'false'.",
                    new[] { nameof(Filter) });

            if (DynamicFilter != null && DynamicFilter.Count > 0)
                yield return new ValidationResult(
                    "DynamicFilter should not be provided when IsDynamicFilter is 'false'.",
                    new[] { nameof(DynamicFilter) });
        }
    }
}
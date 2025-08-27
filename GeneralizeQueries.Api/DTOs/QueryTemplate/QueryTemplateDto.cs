using System.Text.Json.Serialization;

namespace GeneralizeQueries.Api.DTOs;

// This is the main DTO for the entire document
public class QueryTemplateDto
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("Template")]
    public TemplateDetailsDto Template { get; set; } = null!;

    [JsonPropertyName("RolesAndIdsAllowedToRead")]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = [];

    [JsonPropertyName("IsMarkedToDelete")]
    public string IsMarkedToDelete { get; set; } = null!; // Note: It's a string "false", not a boolean
}

// This DTO represents the nested "Template" object
public class TemplateDetailsDto
{
    [JsonPropertyName("Source")]
    public string Source { get; set; } = null!;

    [JsonPropertyName("DynamicFilter")]
    public List<string> DynamicFilter { get; set; } = [];

    [JsonPropertyName("Fields")]
    public List<string> Fields { get; set; } = [];

    [JsonPropertyName("AllowedSorts")]
    public List<string> AllowedSorts { get; set; } = [];

    [JsonPropertyName("Locale")]
    public string Locale { get; set; } = null!;
    
    // In your JSON, this is a string "true", not a boolean true.
    // If it were a boolean, the type would be `bool`.
    [JsonPropertyName("IsDynamicFilter")]
    public string IsDynamicFilter { get; set; } = null!; 

    [JsonPropertyName("CountOnly")]
    public bool CountOnly { get; set; }
}
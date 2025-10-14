using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace GeneralizeQueries.Core.Entities;

public class QueryTemplate
{
    // The _id is a string in your example, not an ObjectId.
    // So we just mark the Id property as the BsonId.
    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("template")] public TemplateDetails Template { get; set; } = null!;

    [JsonPropertyName("rolesAndIdsAllowedToRead")]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = [];

    [JsonPropertyName("isMarkedToDelete")] public string IsMarkedToDelete { get; set; } = null!;
}

public class TemplateDetails
{
    [JsonPropertyName("source")] public string Source { get; set; } = null!;

    // This will be used when IsDynamicFilter = "true"
    [JsonPropertyName("dynamicFilter")]
    [BsonIgnoreIfNull]
    public List<string>? DynamicFilter { get; set; }

    // This will be used when IsDynamicFilter = "false"
    [JsonPropertyName("filter")]
    [BsonIgnoreIfNull]
    public string? Filter { get; set; }

    [JsonPropertyName("fields")] public List<string> Fields { get; set; } = [];

    [JsonPropertyName("allowedSorts")] public List<string> AllowedSorts { get; set; } = [];

    [JsonPropertyName("locale")] public string Locale { get; set; } = null!;

    [JsonPropertyName("isDynamicFilter")] public string IsDynamicFilter { get; set; } = null!;

    [JsonPropertyName("countOnly")]
    [BsonIgnoreIfNull] // Won't save to DB if null
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Won't appear in API response if null
    public bool? CountOnly { get; set; }
}
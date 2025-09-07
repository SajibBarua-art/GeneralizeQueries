using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GeneralizeQueries.Core.Entities;

public class QueryTemplate
{
    // The _id is a string in your example, not an ObjectId.
    // So we just mark the Id property as the BsonId.
    [BsonId]
    public string Id { get; set; } = null!;

    [BsonElement("Template")]
    public TemplateDetails Template { get; set; } = null!;

    [BsonElement("RolesAndIdsAllowedToRead")]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = [];

    [BsonElement("IsMarkedToDelete")]
    public string IsMarkedToDelete { get; set; } = null!;
}

[BsonIgnoreExtraElements]
public class TemplateDetails
{
    [BsonElement("Source")]
    public string Source { get; set; } = null!;

    [BsonElement("DynamicFilter")]
    public List<string> DynamicFilter { get; set; } = [];

    [BsonElement("Fields")]
    public List<string> Fields { get; set; } = [];

    [BsonElement("AllowedSorts")]
    public List<string> AllowedSorts { get; set; } = [];

    [BsonElement("Locale")]
    public string Locale { get; set; } = null!;

    [BsonElement("IsDynamicFilter")]
    public string IsDynamicFilter { get; set; } = null!;
}
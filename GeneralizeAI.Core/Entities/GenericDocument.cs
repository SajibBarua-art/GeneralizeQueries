namespace GeneralizeAI.Core.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class GenericDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = null!;

    // This will hold our dynamic, schema-less data
    public BsonDocument Data { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
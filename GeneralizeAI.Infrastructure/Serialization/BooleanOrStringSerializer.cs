using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace GeneralizeAI.Infrastructure.Serialization;

// This serializer can deserialize a property that is either a boolean or a string in MongoDB.
// It will always serialize back to the database as a string.
public class BooleanOrStringSerializer : SerializerBase<string>
{
    public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();
        switch (bsonType)
        {
            case BsonType.Boolean:
                return context.Reader.ReadBoolean().ToString().ToLower();
            case BsonType.String:
                return context.Reader.ReadString();
            default:
                // Handle other types or throw an exception
                throw new BsonSerializationException($"Cannot deserialize BsonType {bsonType} to a string.");
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
    {
        // When writing back to the DB, we always write a string.
        context.Writer.WriteString(value);
    }
}
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace GeneralizeQueries.Infrastructure.Serialization;

// This serializer can deserialize a property that is either a boolean or a string in MongoDB.
// It will always serialize back to the database as a string.
public class BooleanOrStringSerializer : SerializerBase<string>
{
    private static ILogger? _logger;

    // Static method to set logger from DI container
    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    public override string Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();
        _logger?.LogDebug("Deserializing BsonType: {BsonType}", bsonType);

        switch (bsonType)
        {
            case BsonType.Boolean:
                var boolValue = context.Reader.ReadBoolean();
                var result = boolValue.ToString().ToLower();
                _logger?.LogDebug("Deserialized Boolean {BoolValue} to string '{Result}'", boolValue, result);
                return result;
            case BsonType.String:
                var stringValue = context.Reader.ReadString();
                _logger?.LogDebug("Deserialized String value: '{StringValue}'", stringValue);
                return stringValue;
            default:
                // Handle other types or throw an exception
                _logger?.LogError("Cannot deserialize BsonType {BsonType} to a string", bsonType);
                throw new BsonSerializationException($"Cannot deserialize BsonType {bsonType} to a string.");
        }
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        string value)
    {
        _logger?.LogDebug("Serializing string value to MongoDB: '{Value}'", value);
        // When writing back to the DB, we always write a string.
        context.Writer.WriteString(value);
    }
}
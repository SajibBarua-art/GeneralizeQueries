using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

/// <summary>
///     Defines a factory for creating and managing singleton MongoClient instances.
/// </summary>
public interface IMongoClientFactory
{
    /// <summary>
    ///     Gets a singleton MongoClient for the given connection string.
    /// </summary>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <returns>A shared MongoClient instance.</returns>
    MongoClient GetClient(string connectionString);
}
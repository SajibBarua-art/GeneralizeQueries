using System.Collections.Concurrent;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

/// <summary>
///     Manages singleton MongoClient instances based on their connection strings.
///     This class should be registered as a singleton in the DI container.
/// </summary>
public class MongoClientFactory : IMongoClientFactory
{
    private readonly ConcurrentDictionary<string, MongoClient> _clients = new();

    /// <summary>
    ///     Gets a singleton MongoClient for the given connection string. If a client
    ///     for the connection string does not exist, it will be created and stored.
    /// </summary>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <returns>A shared MongoClient instance.</returns>
    public MongoClient GetClient(string connectionString)
    {
        // GetOrAdd is a thread-safe way to ensure only one instance is created per key.
        return _clients.GetOrAdd(connectionString, cs => new MongoClient(cs));
    }

    // Note: We are not disposing of the clients. They are meant to live for the
    // duration of the application. The driver handles connection management internally.
}
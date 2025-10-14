using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

/// <summary>
///     Base class for repository factories that provides MongoClient instances.
/// </summary>
public abstract class BaseRepositoryFactory
{
    private readonly ILogger _logger;
    private readonly IMongoClientFactory _mongoClientFactory;

    // Inject the new factory
    protected BaseRepositoryFactory(
        IMongoClientFactory mongoClientFactory,
        ILogger logger)
    {
        _mongoClientFactory = mongoClientFactory;
        _logger = logger;
    }

    /// <summary>
    ///     Gets a shared, singleton MongoClient for the given connection string.
    /// </summary>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <returns>A shared MongoClient instance.</returns>
    protected MongoClient GetMongoClient(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("MongoDB connection string is null or empty.");
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
        }

        _logger.LogTrace("Getting MongoClient for connection string.");
        return _mongoClientFactory.GetClient(connectionString);
    }
}
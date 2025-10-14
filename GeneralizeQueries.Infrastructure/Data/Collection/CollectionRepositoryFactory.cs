using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Infrastructure.Data;

public class CollectionRepositoryFactory : BaseRepositoryFactory, ICollectionRepositoryFactory
{
    private readonly ILogger<CollectionRepositoryFactory> _logger;
    private readonly IServiceRegistrationService _registrationService;

    public CollectionRepositoryFactory(
        IServiceRegistrationService registrationService,
        ILogger<CollectionRepositoryFactory> logger,
        IMongoClientFactory mongoClientFactory)
        : base(mongoClientFactory, logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    public async Task<ICollectionRepository?> CreateRepositoryAsync(string serviceId)
    {
        _logger.LogInformation("Creating collection repository for service ID: {ServiceId}", serviceId);
        // Step 1: Look up the connection details for the given serviceId.
        var connectionDetails = await _registrationService.GetReadConnectionDetailsByIdAsync(serviceId);

        if (connectionDetails == null)
        {
            _logger.LogWarning(
                "Cannot create collection repository: connection details not found for service ID: {ServiceId}",
                serviceId);
            // If the serviceId is invalid, we cannot create a repository.
            return null;
        }

        var connString = connectionDetails.Value.ConnectionString;
        var dbName = connectionDetails.Value.DatabaseName;

        // Step 2: Get the shared MongoClient.
        var mongoClient = GetMongoClient(connString);

        // Step 3: Get the database and create the repository.
        var database = mongoClient.GetDatabase(dbName);
        _logger.LogInformation(
            "Successfully created collection repository for service ID: {ServiceId}, Database: {DatabaseName}",
            serviceId, dbName);

        return new MongoCollectionRepository(database, _logger);
    }
}
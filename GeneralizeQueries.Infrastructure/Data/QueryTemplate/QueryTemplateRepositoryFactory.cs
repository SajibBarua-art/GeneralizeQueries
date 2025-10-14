using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Infrastructure.Data;

public class QueryTemplateRepositoryFactory : BaseRepositoryFactory, IQueryTemplateRepositoryFactory
{
    private readonly ILogger<QueryTemplateRepositoryFactory> _logger;
    private readonly IServiceRegistrationService _registrationService;

    public QueryTemplateRepositoryFactory(
        IServiceRegistrationService registrationService,
        ILogger<QueryTemplateRepositoryFactory> logger,
        IMongoClientFactory mongoClientFactory)
        : base(mongoClientFactory, logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    public async Task<IQueryTemplateRepository?> CreateRepositoryAsync(string serviceId)
    {
        _logger.LogInformation("Creating QueryTemplate repository for service ID: {ServiceId}", serviceId);
        // Step 1: Use the registration service to look up the connection details.
        var connectionDetails = await _registrationService.GetReadConnectionDetailsByIdAsync(serviceId);

        if (connectionDetails == null)
        {
            _logger.LogWarning(
                "Cannot create QueryTemplate repository: connection details not found for service ID: {ServiceId}",
                serviceId);
            // If the serviceId is invalid, we cannot create a repository. Return null.
            return null;
        }

        var connString = connectionDetails.Value.ConnectionString;
        var dbName = connectionDetails.Value.DatabaseName;

        // Step 2: Get the shared MongoClient.
        var mongoClient = GetMongoClient(connString);

        // Step 3: Get the database and create the repository.
        var database = mongoClient.GetDatabase(dbName);
        _logger.LogInformation(
            "Successfully created QueryTemplate repository for service ID: {ServiceId}, Database: {DatabaseName}",
            serviceId, dbName);

        return new MongoQueryTemplateRepository(database, _logger);
    }
}
using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Infrastructure.Data;

public class FeatureViewModelsRepositoryFactory : BaseRepositoryFactory, IFeatureViewModelsRepositoryFactory
{
    private readonly ILogger<FeatureViewModelsRepositoryFactory> _logger;
    private readonly IServiceRegistrationService _registrationService;

    public FeatureViewModelsRepositoryFactory(
        IServiceRegistrationService registrationService,
        ILogger<FeatureViewModelsRepositoryFactory> logger,
        IMongoClientFactory mongoClientFactory)
        : base(mongoClientFactory, logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    public async Task<IFeatureViewModelsRepository?> CreateRepositoryAsync(string serviceId)
    {
        _logger.LogInformation("Creating FeatureViewModels repository for service ID: {ServiceId}", serviceId);
        // Step 1: Use the registration service to look up the state database connection details.
        var connectionDetails = await _registrationService.GetReadConnectionDetailsByIdAsync(serviceId);

        if (connectionDetails == null)
        {
            _logger.LogWarning(
                "Cannot create FeatureViewModels repository: connection details not found for service ID: {ServiceId}",
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
            "Successfully created FeatureViewModels repository for service ID: {ServiceId}, Database: {DatabaseName}",
            serviceId, dbName);

        return new MongoFeatureViewModelsRepository(database, _logger);
    }
}
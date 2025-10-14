using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Infrastructure.Data.AuditLog;

public class AuditLogViewModelsRepositoryFactory : BaseRepositoryFactory, IAuditLogViewModelsRepositoryFactory
{
    private readonly ILogger<AuditLogViewModelsRepositoryFactory> _logger;
    private readonly IServiceRegistrationService _registrationService;

    public AuditLogViewModelsRepositoryFactory(
        IServiceRegistrationService registrationService,
        ILogger<AuditLogViewModelsRepositoryFactory> logger,
        IMongoClientFactory mongoClientFactory)
        : base(mongoClientFactory, logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    public async Task<IAuditLogViewModelsRepository?> CreateRepositoryAsync(string serviceId)
    {
        _logger.LogInformation("Creating AuditLogViewModels repository for service ID: {ServiceId}", serviceId);

        // Get connection details from State database (where audit logs are stored)
        var connectionDetails = await _registrationService.GetStateConnectionDetailsByIdAsync(serviceId);

        if (connectionDetails == null)
        {
            _logger.LogWarning(
                "Cannot create AuditLogViewModels repository: connection details not found for service ID: {ServiceId}",
                serviceId);
            return null;
        }

        var connString = connectionDetails.Value.ConnectionString;
        var dbName = connectionDetails.Value.DatabaseName;

        // Get the shared MongoClient
        var mongoClient = GetMongoClient(connString);

        // Get the database and create the repository
        var database = mongoClient.GetDatabase(dbName);
        _logger.LogInformation(
            "Successfully created AuditLogViewModels repository for service ID: {ServiceId}, Database: {DatabaseName}",
            serviceId, dbName);

        // Collection name is "AuditLogViewModels" as per the requirement
        return new MongoAuditLogViewModelsRepository(database, "AuditLogViewModels", _logger);
    }
}
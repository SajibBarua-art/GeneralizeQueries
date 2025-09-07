using GeneralizeQueries.Core.Interfaces;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

public class QueryTemplateRepositoryFactory : IQueryTemplateRepositoryFactory
{
    private readonly IServiceRegistrationService _registrationService;
    private readonly IMongoClient _mongoClient;

    public QueryTemplateRepositoryFactory(IServiceRegistrationService registrationService, IMongoClient mongoClient)
    {
        _registrationService = registrationService;
        _mongoClient = mongoClient;
    }

    public async Task<IQueryTemplateRepository?> CreateRepositoryAsync(string serviceId)
    {
        // Step 1: Use the registration service to look up the connection details.
        var connectionDetails = await _registrationService.GetConnectionDetailsByIdAsync(serviceId);

        if (connectionDetails == null)
        {
            // If the serviceId is invalid, we cannot create a repository. Return null.
            return null;
        }

        // Step 2: Assuming all databases are on the same server, get the specific
        // database instance from the shared IMongoClient.
        var database = _mongoClient.GetDatabase(connectionDetails.Value.DatabaseName);

        // Step 3: Create a new instance of our repository, passing in the
        // specific database it should use.
        return new MongoQueryTemplateRepository(database);
    }
}
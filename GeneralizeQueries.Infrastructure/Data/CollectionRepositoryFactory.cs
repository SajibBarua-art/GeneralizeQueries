using GeneralizeQueries.Core.Interfaces;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GeneralizeQueries.Infrastructure.Data;

public class CollectionRepositoryFactory : ICollectionRepositoryFactory
{
    private readonly IServiceRegistrationService _registrationService;
    private readonly IMongoClient _mongoClient;

    public CollectionRepositoryFactory(IServiceRegistrationService registrationService, IMongoClient mongoClient)
    {
        _registrationService = registrationService;
        _mongoClient = mongoClient;
    }

    public async Task<ICollectionRepository?> CreateRepositoryAsync(string serviceId)
    {
        // Step 1: Look up the connection details for the given serviceId.
        var connectionDetails = await _registrationService.GetConnectionDetailsByIdAsync(serviceId);

        if (connectionDetails == null)
        {
            // If the serviceId is invalid, we cannot create a repository.
            return null;
        }

        // Step 2: Get the specific database instance from the shared IMongoClient.
        var database = _mongoClient.GetDatabase(connectionDetails.Value.DatabaseName);

        // Step 3: Create and return a new repository instance configured for that specific database.
        return new MongoCollectionRepository(database);
    }
}
using GeneralizeQueries.Core.Interfaces;
using MongoDB.Bson;
using System.Collections.Generic; // For KeyNotFoundException
using System.Threading.Tasks;

namespace GeneralizeQueries.Application;

public class DynamicQueryService : IDynamicQueryService
{
    private readonly IServiceRegistrationService _registrationService;
    private readonly IDynamicQueryRepository _dynamicRepository;

    public DynamicQueryService(IServiceRegistrationService registrationService, IDynamicQueryRepository dynamicRepository)
    {
        _registrationService = registrationService;
        _dynamicRepository = dynamicRepository;
    }

    public async Task<List<BsonDocument>> GetAllDocumentsFromCollectionAsync(string serviceId, string collectionName)
    {
        // 1. Get the dynamic connection details using the serviceId.
        var connectionDetails = await _registrationService.GetConnectionDetailsByIdAsync(serviceId);
        
        if (connectionDetails == null)
        {
            // Throw an exception that the controller can catch to return a 404 Not Found.
            throw new KeyNotFoundException($"No service registration found for ID '{serviceId}'.");
        }
        
        // 2. Pass the retrieved details to the dynamic repository to execute the query.
        return await _dynamicRepository.GetAllDocumentsAsync(
            connectionDetails.Value.ConnectionString, // Uses "mongodb://localhost:27017"
            connectionDetails.Value.DatabaseName,   // Uses "AuditLogReadDatabase"
            collectionName
        );
    }
}
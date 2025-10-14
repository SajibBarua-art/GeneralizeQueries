using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Application;

/// <summary>
///     This is the Collection Manager. Its job is to manage tasks related to collections.
///     It is stateless and performs its tasks using the worker it is given.
/// </summary>
public class CollectionService : ICollectionService
{
    private readonly ILogger<CollectionService> _logger;

    // The service now depends on a logger, which should be injected.
    public CollectionService(ILogger<CollectionService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetAllCollectionNames(ICollectionRepository repository)
    {
        _logger.LogInformation("Attempting to get all collection names.");
        try
        {
            var collectionNames = await repository.GetCollectionNamesAsync();
            _logger.LogInformation("Successfully retrieved {Count} collection names.", collectionNames.Count());
            return collectionNames;
        }
        catch (Exception ex)
        {
            // Log the error with context before re-throwing it.
            // This ensures the error is captured at the service layer where the business operation was attempted.
            _logger.LogError(ex, "An error occurred while getting all collection names.");
            throw;
        }
    }

    public async Task<List<string>> GetFieldNamesForCollectionAsync(
        ICollectionRepository repository,
        string collectionName)
    {
        _logger.LogInformation("Attempting to get field names for collection: {CollectionName}", collectionName);
        try
        {
            var fieldNames = await repository.GetFieldNamesAsync(collectionName);
            _logger.LogInformation("Successfully retrieved {Count} field names for collection: {CollectionName}",
                fieldNames.Count, collectionName);
            return fieldNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting field names for collection: {CollectionName}",
                collectionName);
            throw;
        }
    }
}
using GeneralizeQueries.Core.Entities.FeatureManagement;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

public class MongoFeatureAggregateRootsRepository : GenericMongoRepository<FeatureAggregateRoot>,
    IFeatureAggregateRootsRepository
{
    public MongoFeatureAggregateRootsRepository(
        IMongoDatabase database,
        ILogger logger)
        : base(database, "FeatureAggregateRoots", logger)
    {
    }

    /// <summary>
    ///     Override GetByIdAsync to use more efficient GUID-based query
    /// </summary>
    public override async Task<FeatureAggregateRoot?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting FeatureAggregateRoot by ID: {Id}", id);
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }
}
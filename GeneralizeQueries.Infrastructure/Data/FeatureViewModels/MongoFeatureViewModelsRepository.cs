using GeneralizeQueries.Core.Entities.FeatureManagement;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

public class MongoFeatureViewModelsRepository : GenericMongoViewModelRepository<FeatureViewModel>,
    IFeatureViewModelsRepository
{
    public MongoFeatureViewModelsRepository(
        IMongoDatabase database,
        ILogger logger)
        : base(database, "FeatureViewModels", logger)
    {
    }

    /// <summary>
    ///     Override GetByIdAsync to use more efficient GUID-based query
    /// </summary>
    public override async Task<FeatureViewModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting FeatureViewModel by ID: {Id}", id);
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }
}
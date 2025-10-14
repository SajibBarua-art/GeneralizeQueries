using GeneralizeQueries.Core.Entities.FeatureManagement;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

public class MongoRoleFeatureViewModelsRepository : GenericMongoViewModelRepository<RoleFeatureViewModel>,
    IRoleFeatureViewModelsRepository
{
    public MongoRoleFeatureViewModelsRepository(
        IMongoDatabase database,
        ILogger logger)
        : base(database, "RoleFeatureViewModels", logger)
    {
    }

    /// <summary>
    ///     Override GetByIdAsync to use more efficient GUID-based query
    /// </summary>
    public override async Task<RoleFeatureViewModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting RoleFeatureViewModel by ID: {Id}", id);
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }
}
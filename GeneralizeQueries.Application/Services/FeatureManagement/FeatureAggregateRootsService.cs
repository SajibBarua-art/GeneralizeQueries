using GeneralizeQueries.Core.Entities.FeatureManagement;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Application;

public class FeatureAggregateRootsService : GenericService<FeatureAggregateRoot>, IFeatureAggregateRootsService
{
    public FeatureAggregateRootsService(ILogger<GenericService<FeatureAggregateRoot>> logger) : base(logger)
    {
    }

    // Inherits all generic CRUD operations from GenericService<FeatureAggregateRoots>
    // Add any specific business logic for FeatureAggregateRoots here if needed in the future
}
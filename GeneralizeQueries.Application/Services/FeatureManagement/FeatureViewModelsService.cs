using GeneralizeQueries.Core.Entities.FeatureManagement;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Application;

public class FeatureViewModelsService : GenericService<FeatureViewModel>, IFeatureViewModelsService
{
    public FeatureViewModelsService(ILogger<GenericService<FeatureViewModel>> logger) : base(logger)
    {
    }

    // Inherits all generic CRUD operations from GenericService<FeatureViewModel>
    // Add any specific business logic for FeatureViewModels here if needed in the future
}
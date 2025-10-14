using GeneralizeQueries.Core.Entities.FeatureManagement;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Application;

public class RoleFeatureViewModelsService : GenericService<RoleFeatureViewModel>, IRoleFeatureViewModelsService
{
    public RoleFeatureViewModelsService(ILogger<GenericService<RoleFeatureViewModel>> logger) : base(logger)
    {
    }

    // Inherits all generic CRUD operations from GenericService<RoleFeatureViewModel>
    // Add any specific business logic for RoleFeatureViewModels here if needed in the future
}
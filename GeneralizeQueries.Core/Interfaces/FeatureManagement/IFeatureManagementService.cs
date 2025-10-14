using GeneralizeQueries.Api.DTOs.FeatureManagement;
using GeneralizeQueries.Core.Models;
using GeneralizeQueries.Core.Models.FeatureManagement;

namespace GeneralizeQueries.Core.Interfaces;

public interface IFeatureManagementService
{
    Task<PagedResult<FeatureManagementDto>> GetPagedFeaturesAsync(
        string serviceId,
        PaginationParameters parameters,
        string? searchTerm = null);

    Task<FeatureManagementDetailDto> GetByIdAsync(
        string serviceId,
        Guid id);

    Task DeleteFeatureAtomicAsync(
        string serviceId,
        Guid guidId);

    Task ValidateUniqNameAsync(
        string serviceId,
        string uniqName,
        Guid? excludeId = null);
}
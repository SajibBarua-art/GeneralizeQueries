using GeneralizeQueries.Core.DTOs.AuditLog;

namespace GeneralizeQueries.Core.Interfaces;

public interface IAuditLogViewModelsRepository
{
    Task<(List<AuditLogViewModel> Items, long TotalCount)> GetAllPagedAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDirection,
        string? serviceIdFilter);

    Task<AuditLogDetailViewModel?> GetByIdAsync(Guid id);
}
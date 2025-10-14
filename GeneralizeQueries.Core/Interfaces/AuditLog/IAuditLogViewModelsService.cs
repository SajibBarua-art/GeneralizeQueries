using GeneralizeQueries.Core.DTOs.AuditLog;
using GeneralizeQueries.Core.Models;

namespace GeneralizeQueries.Core.Interfaces;

public interface IAuditLogViewModelsService
{
    Task<PagedResult<AuditLogViewModel>> GetPagedAuditLogsAsync(
        IAuditLogViewModelsRepository repository,
        PaginationParameters parameters,
        string? serviceIdFilter);

    Task<AuditLogDetailViewModel?> GetByIdAsync(
        IAuditLogViewModelsRepository repository,
        Guid id);
}
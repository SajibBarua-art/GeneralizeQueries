using GeneralizeQueries.Core.DTOs.AuditLog;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Application.Services.AuditLog;

public class AuditLogViewModelsService : IAuditLogViewModelsService
{
    private readonly ILogger<AuditLogViewModelsService> _logger;

    public AuditLogViewModelsService(ILogger<AuditLogViewModelsService> logger)
    {
        _logger = logger;
    }

    public async Task<PagedResult<AuditLogViewModel>> GetPagedAuditLogsAsync(
        IAuditLogViewModelsRepository repository,
        PaginationParameters parameters,
        string? serviceIdFilter)
    {
        _logger.LogInformation(
            "Getting paged audit logs with Page: {Page}, PageSize: {PageSize}, ServiceIdFilter: {ServiceIdFilter}",
            parameters.Page, parameters.PageSize, serviceIdFilter);

        parameters.ValidateAndSetDefaults();

        var (items, totalCount) = await repository.GetAllPagedAsync(
            parameters.Page,
            parameters.PageSize,
            parameters.SortBy,
            parameters.SortDirection,
            serviceIdFilter);

        _logger.LogInformation("Retrieved {Count} audit logs out of {TotalCount} total", items.Count, totalCount);

        return PagedResult<AuditLogViewModel>.Create(items, parameters.Page, parameters.PageSize, totalCount);
    }

    public async Task<AuditLogDetailViewModel?> GetByIdAsync(
        IAuditLogViewModelsRepository repository,
        Guid id)
    {
        _logger.LogInformation("Getting audit log by ID: {Id}", id);

        var result = await repository.GetByIdAsync(id);

        if (result == null)
            _logger.LogWarning("Audit log with ID {Id} not found", id);
        else
            _logger.LogInformation("Successfully retrieved audit log with ID: {Id}", id);

        return result;
    }
}
using GeneralizeQueries.Api.Authorization;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneralizeQueries.Api.Controllers;

[Authorize]
[RoleAuthorization]
[ApiController]
[Route("auditlogs")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogViewModelsService _auditLogService;
    private readonly ILogger<AuditLogController> _logger;
    private readonly IAuditLogViewModelsRepositoryFactory _repositoryFactory;

    public AuditLogController(
        IAuditLogViewModelsService auditLogService,
        IAuditLogViewModelsRepositoryFactory repositoryFactory,
        ILogger<AuditLogController> logger)
    {
        _auditLogService = auditLogService;
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    private async Task<IAuditLogViewModelsRepository?> GetRepositoryFromFactory(string serviceId)
    {
        var repo = await _repositoryFactory.CreateRepositoryAsync(serviceId);
        return repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? serviceId = null)
    {
        const string auditLogServiceId = "auditlog";

        _logger.LogInformation("Attempting to get all audit logs, ServiceId Filter: {ServiceIdFilter}", serviceId);

        try
        {
            var repo = await GetRepositoryFromFactory(auditLogServiceId);
            if (repo == null)
            {
                _logger.LogWarning("Service with ID '{ServiceId}' not found in configuration.", auditLogServiceId);
                return NotFound(new { message = $"Service with ID '{auditLogServiceId}' not found in configuration." });
            }

            var paginationParams = new PaginationParameters
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var pagedResult = await _auditLogService.GetPagedAuditLogsAsync(repo, paginationParams, serviceId);

            var response = new
            {
                items = pagedResult.Items,
                page = pagedResult.Page,
                pageSize = pagedResult.PageSize,
                totalCount = pagedResult.TotalCount,
                totalPages = pagedResult.TotalPages,
                hasPreviousPage = pagedResult.HasPreviousPage,
                hasNextPage = pagedResult.HasNextPage
            };

            _logger.LogInformation("Successfully retrieved {Count} audit logs for Service ID: {ServiceId}",
                pagedResult.Items.Count(), auditLogServiceId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while getting audit logs for Service ID: {ServiceId}",
                auditLogServiceId);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        const string auditLogServiceId = "auditlog";

        _logger.LogInformation("Attempting to get audit log by ID: {Id}", id);

        if (!Guid.TryParse(id, out var guidId))
        {
            _logger.LogWarning("Invalid GUID format provided for audit log ID: {Id}", id);
            return BadRequest(new { message = "The provided ID is not in a valid GUID format." });
        }

        try
        {
            var repo = await GetRepositoryFromFactory(auditLogServiceId);
            if (repo == null)
            {
                _logger.LogWarning("Service with ID '{ServiceId}' not found in configuration.", auditLogServiceId);
                return NotFound(new { message = $"Service with ID '{auditLogServiceId}' not found in configuration." });
            }

            var auditLog = await _auditLogService.GetByIdAsync(repo, guidId);

            if (auditLog == null)
            {
                _logger.LogWarning("Audit log with ID {Id} not found", guidId);
                return NotFound(new { message = $"Audit log with ID '{id}' not found." });
            }

            _logger.LogInformation("Successfully retrieved audit log with ID: {Id}", guidId);
            return Ok(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while getting audit log by ID: {Id}", id);
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}
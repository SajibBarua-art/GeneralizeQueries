using GeneralizeQueries.Api.Authorization;
using GeneralizeQueries.Api.DTOs;
using GeneralizeQueries.Api.DTOs.QueryTemplate;
using GeneralizeQueries.Core.Entities;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneralizeQueries.Api.Controllers;

[Authorize]
[RoleAuthorization]
[ApiController]
[Route("templates")]
public class QueryTemplatesController : ControllerBase
{
    private readonly ILogger<QueryTemplatesController> _logger;
    private readonly IQueryTemplateRepositoryFactory _repositoryFactory;
    private readonly IQueryTemplateService _templateService;

    public QueryTemplatesController(
        IQueryTemplateService templateService,
        IQueryTemplateRepositoryFactory repositoryFactory,
        ILogger<QueryTemplatesController> logger) // Inject ILogger
    {
        _templateService = templateService;
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    private async Task<IQueryTemplateRepository?> GetRepositoryFromFactory(string serviceId)
    {
        var repo = await _repositoryFactory.CreateRepositoryAsync(serviceId);
        return repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? search = null)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to get all templates for Service ID: {ServiceId}", serviceId);
        try
        {
            var repo = await GetRepositoryFromFactory(serviceId);
            if (repo == null)
            {
                _logger.LogWarning("Service with ID '{ServiceId}' not found in configuration.", serviceId);
                return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
            }

            var paginationParams = new PaginationParameters
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var pagedResult = await _templateService.GetPagedTemplatesAsync(repo, paginationParams);

            var filteredItems = pagedResult.Items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                filteredItems = filteredItems.Where(template =>
                    template.Template.Source.Contains(search, StringComparison.OrdinalIgnoreCase)
                );

            var mappedItems = filteredItems.Select(MapToDto);
            var responseDto = PagedQueryTemplateResponseDto<QueryTemplateDto>.FromPagedResult(
                PagedResult<QueryTemplateDto>.Create(mappedItems, pagedResult.Page, pagedResult.PageSize,
                    pagedResult.TotalCount));

            _logger.LogInformation("Successfully retrieved {Count} templates for Service ID: {ServiceId}",
                responseDto.Items.Count(), serviceId);
            return Ok(responseDto);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database connection was disposed while getting templates for Service ID: {ServiceId}",
                serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while getting all templates for Service ID: {ServiceId}",
                serviceId);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("{id}", Name = "GetTemplateById")]
    public async Task<IActionResult> GetById(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        string id)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to get template by ID: {TemplateId} for Service ID: {ServiceId}", id,
            serviceId);
        try
        {
            var repo = await GetRepositoryFromFactory(serviceId);
            if (repo == null)
            {
                _logger.LogWarning(
                    "Service with ID '{ServiceId}' not found in configuration. Cannot get template '{TemplateId}'.",
                    serviceId, id);
                return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
            }

            var template = await _templateService.GetTemplateByIdAsync(repo, id);
            if (template == null)
            {
                _logger.LogWarning("Template with ID '{TemplateId}' not found for Service ID: {ServiceId}", id,
                    serviceId);
                return NotFound(new { message = $"Template with ID '{id}' not found in service '{serviceId}'." });
            }

            _logger.LogInformation("Successfully retrieved template with ID: {TemplateId}", id);
            return Ok(MapToDto(template));
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "Database connection was disposed while getting template ID: {TemplateId} for Service ID: {ServiceId}",
                id, serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An unexpected error occurred while getting template ID: {TemplateId} for Service ID: {ServiceId}", id,
                serviceId);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        CreateQueryTemplateDto createDto)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to create a new template with ID: {TemplateId} for Service ID: {ServiceId}",
            createDto.Id, serviceId);
        if (!ModelState.IsValid)
            // Logging validation errors can be noisy, but useful for debugging client issues.
            // _logger.LogWarning("Invalid model state for creating template for Service ID: {ServiceId}. Errors: {Errors}", serviceId, ModelState.Values.SelectMany(v => v.Errors));
            return BadRequest(ModelState);

        try
        {
            var repo = await GetRepositoryFromFactory(serviceId);
            if (repo == null)
            {
                _logger.LogWarning(
                    "Service with ID '{ServiceId}' not found in configuration. Cannot create template '{TemplateId}'.",
                    serviceId, createDto.Id);
                return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
            }

            var existingTemplate = await _templateService.GetTemplateByIdAsync(repo, createDto.Id);
            if (existingTemplate != null)
            {
                _logger.LogWarning(
                    "Attempt to create a template with an existing ID '{TemplateId}' for Service ID '{ServiceId}' was blocked.",
                    createDto.Id, serviceId);
                return Conflict($"A template with ID '{createDto.Id}' already exists in service '{serviceId}'.");
            }

            var template = MapFromCreateDtoToEntity(createDto);
            await _templateService.CreateTemplateAsync(repo, template);

            _logger.LogInformation("Successfully created template with ID: {TemplateId} for Service ID: {ServiceId}",
                template.Id, serviceId);
            var responseDto = MapToDto(template);
            return CreatedAtRoute("GetTemplateById", new { id = responseDto.Id }, responseDto);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "Database connection was disposed while creating template with ID: {TemplateId} for Service ID: {ServiceId}",
                createDto.Id, serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An unexpected error occurred while creating template with ID: {TemplateId} for Service ID: {ServiceId}",
                createDto.Id, serviceId);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        string id,
        UpdateQueryTemplateDto updateDto)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to update template with ID: {TemplateId} for Service ID: {ServiceId}", id,
            serviceId);
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var repo = await GetRepositoryFromFactory(serviceId);
            if (repo == null)
            {
                _logger.LogWarning(
                    "Service with ID '{ServiceId}' not found in configuration. Cannot update template '{TemplateId}'.",
                    serviceId, id);
                return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
            }

            var existingTemplate = await _templateService.GetTemplateByIdAsync(repo, id);
            if (existingTemplate is null)
            {
                _logger.LogWarning(
                    "Attempted to update a non-existent template with ID '{TemplateId}' for Service ID '{ServiceId}'.",
                    id, serviceId);
                return NotFound(new { message = $"Template with ID '{id}' not found in service '{serviceId}'." });
            }

            var templateToUpdate = MapFromUpdateDtoToEntity(id, updateDto);
            await _templateService.UpdateTemplateAsync(repo, id, templateToUpdate);

            _logger.LogInformation("Successfully updated template with ID: {TemplateId} for Service ID: {ServiceId}",
                id, serviceId);
            return NoContent();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "Database connection was disposed while updating template with ID: {TemplateId} for Service ID: {ServiceId}",
                id, serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An unexpected error occurred while updating template with ID: {TemplateId} for Service ID: {ServiceId}",
                id, serviceId);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        string id)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to delete template with ID: {TemplateId} for Service ID: {ServiceId}", id,
            serviceId);
        try
        {
            var repo = await GetRepositoryFromFactory(serviceId);
            if (repo == null)
            {
                _logger.LogWarning(
                    "Service with ID '{ServiceId}' not found in configuration. Cannot delete template '{TemplateId}'.",
                    serviceId, id);
                return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
            }

            var existingTemplate = await _templateService.GetTemplateByIdAsync(repo, id);
            if (existingTemplate is null)
            {
                _logger.LogWarning(
                    "Attempted to delete a non-existent template with ID '{TemplateId}' for Service ID '{ServiceId}'.",
                    id, serviceId);
                return NotFound(new { message = $"Template with ID '{id}' not found in service '{serviceId}'." });
            }

            await _templateService.DeleteTemplateAsync(repo, id);

            _logger.LogInformation("Successfully deleted template with ID: {TemplateId} for Service ID: {ServiceId}",
                id, serviceId);
            return NoContent();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "Database connection was disposed while deleting template with ID: {TemplateId} for Service ID: {ServiceId}",
                id, serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An unexpected error occurred while deleting template with ID: {TemplateId} for Service ID: {ServiceId}",
                id, serviceId);
            return StatusCode(500, "An internal server error occurred.");
        }
    }


    // --- v-- MAPPING HELPERS --v ---
    // No logging needed in pure mapping functions.
    private QueryTemplate MapFromCreateDtoToEntity(CreateQueryTemplateDto dto)
    {
        var template = new TemplateDetails
        {
            Source = dto.Template.Source,
            Fields = dto.Template.Fields,
            AllowedSorts = dto.Template.AllowedSorts,
            Locale = dto.Template.Locale,
            IsDynamicFilter = dto.Template.IsDynamicFilter,
            CountOnly = dto.Template.CountOnly
        };

        if (dto.Template.IsDynamicFilter == "true")
            template.DynamicFilter = dto.Template.DynamicFilter;
        else if (dto.Template.IsDynamicFilter == "false") template.Filter = dto.Template.Filter;

        return new QueryTemplate
        {
            Id = dto.Id,
            RolesAndIdsAllowedToRead = dto.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = dto.IsMarkedToDelete,
            Template = template
        };
    }

    private QueryTemplate MapFromUpdateDtoToEntity(
        string id,
        UpdateQueryTemplateDto dto)
    {
        var template = new TemplateDetails
        {
            Source = dto.Template.Source,
            Fields = dto.Template.Fields,
            AllowedSorts = dto.Template.AllowedSorts,
            Locale = dto.Template.Locale,
            IsDynamicFilter = dto.Template.IsDynamicFilter,
            CountOnly = dto.Template.CountOnly
        };

        if (dto.Template.IsDynamicFilter == "true")
            template.DynamicFilter = dto.Template.DynamicFilter;
        else if (dto.Template.IsDynamicFilter == "false") template.Filter = dto.Template.Filter;

        return new QueryTemplate
        {
            Id = id,
            RolesAndIdsAllowedToRead = dto.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = dto.IsMarkedToDelete,
            Template = template
        };
    }

    private QueryTemplateDto MapToDto(QueryTemplate template)
    {
        return new QueryTemplateDto
        {
            Id = template.Id,
            RolesAndIdsAllowedToRead = template.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = template.IsMarkedToDelete,
            Template = new TemplateDetailsDto
            {
                Source = template.Template.Source,
                Fields = template.Template.Fields,
                AllowedSorts = template.Template.AllowedSorts,
                Locale = template.Template.Locale,
                IsDynamicFilter = template.Template.IsDynamicFilter,
                CountOnly = template.Template.CountOnly,
                DynamicFilter = template.Template.DynamicFilter,
                Filter = template.Template.Filter
            }
        };
    }
}
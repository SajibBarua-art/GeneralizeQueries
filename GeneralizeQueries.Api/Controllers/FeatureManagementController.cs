using GeneralizeQueries.Api.Authorization;
using GeneralizeQueries.Api.DTOs.FeatureManagement;
using GeneralizeQueries.Core.Exceptions;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using GeneralizeQueries.Core.Models.FeatureManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Infrastructure.Common.Security;
using Platform.Infrastructure.Core.Bus;
using Platform.Uam.Commands;

namespace GeneralizeQueries.Api.Controllers;

[Authorize]
[RoleAuthorization]
[ApiController]
[Route("features")]
public class FeatureManagementController : ControllerBase
{
    private readonly IBusMessageDispatcher _dispatcher;
    private readonly IFeatureManagementService _featureManagementService;
    private readonly ILogger<FeatureManagementController> _logger; // Added ILogger
    private readonly IUserContextProvider _userContextProvider;

    public FeatureManagementController(
        IFeatureManagementService featureManagementService,
        IBusMessageDispatcher dispatcher,
        IUserContextProvider userContextProvider,
        ILogger<FeatureManagementController> logger) // Injected ILogger
    {
        _featureManagementService = featureManagementService;
        _dispatcher = dispatcher;
        _userContextProvider = userContextProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFeatures(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? search = null)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation(
            "Attempting to get all features for Service ID: {ServiceId} with Page: {Page}, PageSize: {PageSize}",
            serviceId, page, pageSize);

        try
        {
            var paginationParams = new PaginationParameters
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            paginationParams.ValidateAndSetDefaults();

            var pagedResult =
                await _featureManagementService.GetPagedFeaturesAsync(serviceId, paginationParams, search);
            var responseDto = PagedFeatureManagementResponseDto<FeatureManagementDto>.FromPagedResult(pagedResult);

            _logger.LogInformation("Successfully retrieved {Count} features for Service ID: {ServiceId}",
                pagedResult.Items.Count(), serviceId);
            return Ok(responseDto);
        }
        catch (InvalidOperationException ex)
        {
            // Check if this is a database connection error or service not found
            if (ex.InnerException is ObjectDisposedException)
            {
                _logger.LogError(ex, "Database connection error for Service ID: {ServiceId}", serviceId);
                return StatusCode(503, new { message = ex.Message });
            }

            // This often means the serviceId configuration was not found. This is a client-side error.
            _logger.LogWarning(ex, "Could not find service configuration for Service ID: {ServiceId}", serviceId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // This is an unexpected error.
            _logger.LogError(ex, "An unexpected error occurred while retrieving features for Service ID: {ServiceId}",
                serviceId);
            return StatusCode(500,
                new { message = "An error occurred while retrieving features.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        string id)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to get feature with ID: {FeatureId} for Service ID: {ServiceId}", id,
            serviceId);

        if (!Guid.TryParse(id, out var guidId))
        {
            _logger.LogWarning("Invalid GUID format provided for feature ID: {FeatureId}", id);
            return BadRequest(new { message = "The provided ID is not in a valid GUID format." });
        }

        try
        {
            var feature = await _featureManagementService.GetByIdAsync(serviceId, guidId);
            _logger.LogInformation("Successfully retrieved feature with ID: {FeatureId} for Service ID: {ServiceId}",
                id, serviceId);
            return Ok(feature);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Feature with ID: {FeatureId} not found for Service ID: {ServiceId}", id, serviceId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Check if this is a database connection error
            if (ex.InnerException is ObjectDisposedException)
            {
                _logger.LogError(ex, "Database connection error for Service ID: {ServiceId}", serviceId);
                return StatusCode(503, new { message = ex.Message });
            }

            _logger.LogWarning(ex,
                "Invalid operation while getting feature ID: {FeatureId} for Service ID: {ServiceId}", id, serviceId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An unexpected error occurred while retrieving feature with ID: {FeatureId} for Service ID: {ServiceId}",
                id, serviceId);
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving the feature.",
                error = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        [FromBody] CreateFeatureDto createDto)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to create a new feature for Service ID: {ServiceId}", serviceId);

        if (!ModelState.IsValid)
            // No need to log here, as ASP.NET Core framework logging often covers this.
            // If you need more detail, you could log the validation errors.
            return BadRequest(ModelState);

        try
        {
            // The validation logic inside the try-catch is fine, but exceptions are better for flow control.
            // For this exercise, we'll log the specific exceptions caught below.

            // Validate and normalize UniqName
            string? normalizedUniqName = null;
            if (!string.IsNullOrEmpty(createDto.UniqName))
            {
                normalizedUniqName = createDto.UniqName.ToLower().Trim();

                if (!normalizedUniqName.All(c => char.IsLetter(c) || c == '_'))
                {
                    _logger.LogWarning(
                        "Validation failed for creating feature in Service ID {ServiceId}: UniqName '{UniqName}' contains invalid characters.",
                        serviceId, createDto.UniqName);
                    return BadRequest(new { message = "Feature UniqName can only contain letters and underscores." });
                }

                createDto.UniqName = normalizedUniqName;
                await _featureManagementService.ValidateUniqNameAsync(serviceId, normalizedUniqName);
            }

            var userContext = _userContextProvider.GetUserContext();
            userContext.ServiceId = serviceId;

            var newFeatureId = Guid.NewGuid();
            var command = new CreateFeaturesCommand
            {
                IsInMemoryCommand = false,
                CorrelationId = Guid.NewGuid(),
                UserContext = userContext,
                QueueName = typeof(CreateFeaturesCommand).FullName ?? string.Empty,
                Features = new[]
                {
                    new FeatureDto
                    {
                        Id = newFeatureId,
                        Name = createDto.Name,
                        Commands = createDto.Commands,
                        TagName = createDto.TagName,
                        UniqName = normalizedUniqName ?? createDto.UniqName
                    }
                }
            };

            await _dispatcher.SendAsync(command);

            _logger.LogInformation(
                "Successfully dispatched command to create feature with new ID: {FeatureId} for Service ID: {ServiceId}",
                newFeatureId, serviceId);
            return CreatedAtAction(nameof(GetById), new { serviceId, id = newFeatureId },
                new { message = "Feature creation initiated successfully.", id = newFeatureId });
        }
        catch (DuplicateUniqNameException ex)
        {
            _logger.LogWarning(
                "Failed to create feature for Service ID {ServiceId} due to duplicate UniqName: {UniqName}", serviceId,
                createDto.UniqName);
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex,
                "Failed to create feature for Service ID {ServiceId} because a related entity was not found.",
                serviceId);
            return NotFound(new { message = ex.Message });
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database connection was disposed while creating feature for Service ID: {ServiceId}",
                serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a feature for Service ID: {ServiceId}",
                serviceId);
            return StatusCode(500,
                new { message = "An error occurred while creating the feature.", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFeature(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        string id)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to delete feature with ID: {FeatureId} from Service ID: {ServiceId}", id,
            serviceId);

        if (!Guid.TryParse(id, out var guidId))
        {
            _logger.LogWarning("Invalid GUID format provided for feature ID to delete: {FeatureId}", id);
            return BadRequest(new { message = "The provided ID is not in a valid GUID format." });
        }

        try
        {
            await _featureManagementService.DeleteFeatureAtomicAsync(serviceId, guidId);
            _logger.LogInformation("Successfully deleted feature with ID: {FeatureId} from Service ID: {ServiceId}",
                guidId, serviceId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(
                "Could not delete feature with ID: {FeatureId} from Service ID: {ServiceId} because it was not found.",
                guidId, serviceId);
            return NotFound(new { message = ex.Message });
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex,
                "Bad request while deleting feature with ID: {FeatureId} from Service ID: {ServiceId}", guidId,
                serviceId);
            return BadRequest(new { message = ex.Message });
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "Database connection was disposed while deleting feature with ID: {FeatureId} from Service ID: {ServiceId}",
                guidId, serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An unexpected error occurred while deleting feature with ID: {FeatureId} from Service ID: {ServiceId}",
                guidId, serviceId);
            return StatusCode(500,
                new { message = "An error occurred while deleting the feature.", details = ex.Message });
        }
    }
}
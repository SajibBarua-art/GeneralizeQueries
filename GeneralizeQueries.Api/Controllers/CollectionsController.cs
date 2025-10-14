using GeneralizeQueries.Api.Authorization;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneralizeQueries.Api.Controllers;

[Authorize]
[RoleAuthorization]
[ApiController]
[Route("collections")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICollectionRepositoryFactory _repositoryFactory;

    public CollectionsController(
        ICollectionService collectionService,
        ICollectionRepositoryFactory repositoryFactory,
        ILogger<CollectionsController> logger) // Inject ILogger
    {
        _collectionService = collectionService;
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    private async Task<ICollectionRepository?> GetRepository(string serviceId)
    {
        // Logging the creation attempt itself can be noisy; it's better to log the outcome in the public methods.
        return await _repositoryFactory.CreateRepositoryAsync(serviceId);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetCollections(
        [FromHeader(Name = "x-service-id")] string? serviceId)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to get collections for Service ID: {ServiceId}", serviceId);

        try
        {
            var repo = await GetRepository(serviceId);
            if (repo == null)
            {
                // Log a warning for a known but handled failure condition.
                _logger.LogWarning("Service with ID '{ServiceId}' not found in configuration.", serviceId);
                return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
            }

            var collectionNames = await _collectionService.GetAllCollectionNames(repo);
            _logger.LogInformation("Successfully retrieved {Count} collections for Service ID: {ServiceId}",
                collectionNames.Count(), serviceId);

            return Ok(collectionNames);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "Database connection was disposed while getting collections for Service ID: {ServiceId}", serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while getting collections for Service ID: {ServiceId}",
                serviceId);
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }

    [HttpGet("{collectionName}/fields")]
    public async Task<ActionResult<List<string>>> GetFields(
        [FromHeader(Name = "x-service-id")] string? serviceId,
        string collectionName)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            return BadRequest(new { message = "x-service-id header is required" });

        _logger.LogInformation("Attempting to get fields for Collection: {CollectionName} in Service ID: {ServiceId}",
            collectionName, serviceId);

        try
        {
            var repo = await GetRepository(serviceId);
            if (repo == null)
            {
                _logger.LogWarning(
                    "Service with ID '{ServiceId}' not found in configuration. Cannot get fields for collection '{CollectionName}'.",
                    serviceId, collectionName);
                return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
            }

            var fieldNames = await _collectionService.GetFieldNamesForCollectionAsync(repo, collectionName);
            _logger.LogInformation(
                "Successfully retrieved {Count} fields for Collection: {CollectionName} in Service ID: {ServiceId}",
                fieldNames.Count, collectionName, serviceId);

            return Ok(fieldNames);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "Database connection was disposed while getting fields for Collection: {CollectionName} in Service ID: {ServiceId}",
                collectionName, serviceId);
            return StatusCode(503, new { message = "Database connection error. Please retry the request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An unexpected error occurred while getting fields for Collection: {CollectionName} in Service ID: {ServiceId}",
                collectionName, serviceId);
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }
}
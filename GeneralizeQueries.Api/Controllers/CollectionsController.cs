using GeneralizeQueries.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GeneralizeQueries.Api.Controllers;

[ApiController]
[Route("api/service/{serviceId}/collections")] // The route is now dynamic
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;
    private readonly ICollectionRepositoryFactory _repositoryFactory;

    // The front desk (Controller) has a direct line to the manager (Service)
    // and also knows how to contact the factory to hire a temporary worker.
    public CollectionsController(ICollectionService collectionService, ICollectionRepositoryFactory repositoryFactory)
    {
        _collectionService = collectionService;
        _repositoryFactory = repositoryFactory;
    }

    // A private helper to get the correctly configured repository for a request.
    private async Task<ICollectionRepository?> GetRepository(string serviceId)
    {
        return await _repositoryFactory.CreateRepositoryAsync(serviceId);
    }

    [HttpGet]
    // [Authorize]
    public async Task<ActionResult<IEnumerable<string>>> GetCollections(string serviceId)
    {
        var repo = await GetRepository(serviceId);
        if (repo == null)
        {
            return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
        }

        var collectionNames = await _collectionService.GetAllCollectionNames(repo);
        return Ok(collectionNames);
    }

    [HttpGet("{collectionName}/fields")]
    public async Task<ActionResult<List<string>>> GetFields(string serviceId, string collectionName)
    {
        var repo = await GetRepository(serviceId);
        if (repo == null)
        {
            return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
        }

        var fieldNames = await _collectionService.GetFieldNamesForCollectionAsync(repo, collectionName);
        return Ok(fieldNames);
    }
}
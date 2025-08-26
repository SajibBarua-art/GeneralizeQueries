using GeneralizeAI.Application;
using Microsoft.AspNetCore.Mvc;

namespace GeneralizeAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly CollectionService _collectionService;

    // The front desk talks to the manager (the service).
    public CollectionsController(CollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // It takes the user's request and asks the manager to handle it.
        var names = await _collectionService.GetAllCollectionNames();
        return Ok(names);
    }
}
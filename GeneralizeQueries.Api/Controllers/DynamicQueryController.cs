using GeneralizeQueries.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;

namespace GeneralizeQueries.Api.Controllers;

[ApiController]
[Route("api/query")]
public class DynamicQueryController : ControllerBase
{
    private readonly IDynamicQueryService _queryService;

    public DynamicQueryController(IDynamicQueryService queryService)
    {
        _queryService = queryService;
    }

    /// <summary>
    /// Gets all documents from a collection by dynamically resolving the database from a service ID.
    /// </summary>
    /// <param name="serviceId">The ID from ServiceRegistrations.json (e.g., 'storage', 'reservation').</param>
    /// <param name="collectionName">The name of the collection to query.</param>
    // [HttpGet("{serviceId}/{collectionName}")]
    // public async Task<IActionResult> GetDocuments(string serviceId, string collectionName)
    // {
    //     try
    //     {
    //         // 1. Get the raw BSON documents from the service
    //         var bsonDocuments = await _queryService.GetAllDocumentsFromCollectionAsync(serviceId, collectionName);
    //         
    //         // 2. Convert the list of BsonDocument into a list of Dictionary<string, object>.
    //         //    This is a format that System.Text.Json understands perfectly.
    //         //    We use LINQ's .Select() to transform each item in the list.
    //         var jsonFriendlyDocuments = bsonDocuments.Select(doc =>
    //             BsonSerializer.Deserialize<Dictionary<string, object>>(doc)
    //         ).ToList();
    //
    //         // 3. Return the clean, JSON-friendly list.
    //         return Ok(jsonFriendlyDocuments);
    //     }
    //     catch (KeyNotFoundException ex)
    //     {
    //         return NotFound(new { message = ex.Message });
    //     }
    //     catch (Exception ex)
    //     {
    //         // Catch other potential errors (e.g., connection timeout)
    //         return StatusCode(500, new { message = "An internal error occurred.", details = ex.Message });
    //     }
    // }
}
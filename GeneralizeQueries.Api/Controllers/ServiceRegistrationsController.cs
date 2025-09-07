using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeneralizeQueries.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRegistrationsController : ControllerBase
{
    private readonly IServiceRegistrationService _service;

    public ServiceRegistrationsController(IServiceRegistrationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRegistrations()
    {
        try
        {
            // 1. Get the full, detailed list of registrations from the service.
            var fullRegistrations = await _service.GetAllAsync();

            // --- v-- THE TRANSFORMATION --v ---
            // 2. Use LINQ's .Select() to transform each full object
            //    into our new, simple DTO.
            var simpleInfoList = fullRegistrations.Select(reg => new SimpleServiceInfoDto()
            {
                Id = reg.Id,
                ServiceName = reg.ServiceName
            });
            // --- ^-- END OF TRANSFORMATION --^ ---

            // 3. Return the new list of simple objects.
            return Ok(simpleInfoList);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // [HttpGet("{serviceId}/collections")]
    // public async Task<IActionResult> GetCollectionsByServiceId(string serviceId)
    // {
    //     try
    //     {
    //         var collections = await _service.GetCollectionsForServiceAsync(serviceId);
    //         if (collections.Count == 0)
    //         {
    //             return NotFound(new { message = $"Service with ID '{serviceId}' not found or has no collections." });
    //         }
    //         return Ok(collections);
    //     }
    //     catch (InvalidOperationException ex)
    //     {
    //         return BadRequest(new { message = ex.Message });
    //     }
    // }
}
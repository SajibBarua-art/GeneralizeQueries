using GeneralizeQueries.Api.Authorization;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneralizeQueries.Api.Controllers;

[Authorize]
[RoleAuthorization]
[ApiController]
[Route("services")]
public class ServiceRegistrationsController : ControllerBase
{
    private readonly ILogger<ServiceRegistrationsController> _logger;
    private readonly IServiceRegistrationService _service;

    public ServiceRegistrationsController(
        IServiceRegistrationService service,
        ILogger<ServiceRegistrationsController> logger) // Inject ILogger
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRegistrations()
    {
        _logger.LogInformation("Attempting to get all service registrations.");
        try
        {
            var fullRegistrations = await _service.GetAllAsync();

            var simpleInfoList = fullRegistrations.Select(reg => new SimpleServiceInfoDto
            {
                Id = reg.Id,
                ServiceName = reg.ServiceName
            }).ToList(); // Use ToList() to easily get the count

            _logger.LogInformation("Successfully retrieved {Count} service registrations.", simpleInfoList.Count);

            return Ok(simpleInfoList);
        }
        catch (FileNotFoundException ex)
        {
            // This is a configuration-level error, significant enough for a warning.
            _logger.LogWarning(ex, "Service registrations configuration file was not found.");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Catch any other unexpected errors.
            _logger.LogError(ex, "An unexpected error occurred while retrieving service registrations.");
            return StatusCode(500,
                new { message = "An internal server error occurred while retrieving service registrations." });
        }
    }
}
namespace GeneralizeQueries.Core.Models;

/// <summary>
///     A lightweight DTO representing the basic information for a service registration.
///     Used for API endpoints that return a list of available services.
/// </summary>
public class SimpleServiceInfoDto
{
    public string Id { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
}
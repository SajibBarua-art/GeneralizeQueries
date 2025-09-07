using System.Text.Json.Serialization;

namespace GeneralizeQueries.Core.Models;

public class ServiceRegistrationDto
{
    public string Id { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public List<TenantDto> Tenants { get; set; } = [];
}

public class TenantDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<VerticalDto> Verticals { get; set; } = [];
}

public class VerticalDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ReadDatabaseName { get; set; } = null!;
    public string StateDatabaseName { get; set; } = null!;
    public string EventDatabaseName { get; set; } = null!;
    public string DefaultDatabaseName { get; set; } = null!;
    public string? DefaultServerConnectionString { get; set; }
    public string? ReadServerConnectionString { get; set; }
    public List<object> Apps { get; set; } = []; // Using 'object' for simplicity
}
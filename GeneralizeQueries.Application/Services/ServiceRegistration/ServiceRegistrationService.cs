using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.Extensions.Logging;
using Platform.Infrastructure.ServiceRegistry;

namespace GeneralizeQueries.Application;

public class ServiceRegistrationService : IServiceRegistrationService
{
    private readonly ILogger<ServiceRegistrationService> _logger;
    private readonly IServiceRegistryProvider _serviceRegistryProvider;
    private readonly IEnumerable<Vertical> _verticals;

    public ServiceRegistrationService(
        ILogger<ServiceRegistrationService> logger,
        IServiceRegistryProvider serviceRegistryProvider)
    {
        _logger = logger;
        _serviceRegistryProvider = serviceRegistryProvider;

        IEnumerable<Vertical> verticals =
            _serviceRegistryProvider.GetAllServices().SelectMany(service => service.Verticals);
        _verticals = verticals;
    }

    public async Task<List<ServiceRegistrationDto>> GetAllAsync()
    {
        _logger.LogInformation("Getting all service registrations from verticals");

        // Group verticals by ServiceId and TenantId to create the hierarchical structure
        var registrations = _verticals
            .GroupBy(v => v.ServiceId)
            .Select(serviceGroup => new ServiceRegistrationDto
            {
                Id = serviceGroup.First().ServiceId,
                ServiceName = serviceGroup.First().Name,
                Tenants = serviceGroup
                    .GroupBy(v => v.TenantId)
                    .Select(tenantGroup => new TenantDto
                    {
                        Id = tenantGroup.Key.ToString(),
                        Name = tenantGroup.First().Name,
                        Verticals = tenantGroup.Select(v => new VerticalDto
                        {
                            Id = v.Id.ToString(),
                            Name = v.Name,
                            ReadDatabaseName = v.ReadDatabaseName,
                            StateDatabaseName = v.StateDatabaseName,
                            EventDatabaseName = v.EventDatabaseName,
                            DefaultDatabaseName = v.DefaultDatabaseName,
                            DefaultServerConnectionString = v.DefaultServerConnectionString,
                            ReadServerConnectionString = v.ReadServerConnectionString,
                            StateServerConnectionString = v.StateServerConnectionString,
                            EventServerConnectionString = v.EventServerConnectionString,
                            Apps = v.Apps?.ToList<object>() ?? new List<object>()
                        }).ToList()
                    }).ToList()
            }).ToList();

        _logger.LogInformation("Retrieved {Count} service registrations from verticals", registrations.Count);
        return await Task.FromResult(registrations);
    }

    public async Task<(string ConnectionString, string DatabaseName)?> GetStateConnectionDetailsByIdAsync(
        string serviceId)
    {
        _logger.LogInformation("Getting state connection details for service ID: {ServiceId}", serviceId);
        var registrations = await GetAllAsync();

        var registration =
            registrations.FirstOrDefault(r => r.Id.Equals(serviceId, StringComparison.OrdinalIgnoreCase));

        if (registration == null)
        {
            _logger.LogWarning("Service registration not found for ID: {ServiceId}", serviceId);
            // If the ID isn't found in the file, return null.
            return null;
        }

        var vertical = registration.Tenants?.FirstOrDefault()?.Verticals?.FirstOrDefault();
        if (vertical == null)
        {
            _logger.LogWarning("No vertical configuration found for service ID: {ServiceId}", serviceId);
            return null;
        }

        // Logic to select the connection string for state database
        var connectionString = !string.IsNullOrEmpty(vertical.StateServerConnectionString)
            ? vertical.StateServerConnectionString
            : vertical.DefaultServerConnectionString;

        // Get the StateDatabaseName for FeatureAggregateRoots
        var databaseName = vertical.StateDatabaseName;

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
        {
            _logger.LogWarning("Incomplete state connection details for service ID: {ServiceId}", serviceId);
            // Not enough info in the file to connect.
            return null;
        }

        _logger.LogInformation(
            "Successfully retrieved state connection details for service ID: {ServiceId}, Database: {DatabaseName}",
            serviceId, databaseName);
        // It returns the connection string and the state database name it found.
        return (connectionString, databaseName);
    }

    public async Task<(string ConnectionString, string DatabaseName)?> GetReadConnectionDetailsByIdAsync(
        string serviceId)
    {
        _logger.LogInformation("Getting read connection details for service ID: {ServiceId}", serviceId);
        var registrations = await GetAllAsync();

        var registration =
            registrations.FirstOrDefault(r => r.Id.Equals(serviceId, StringComparison.OrdinalIgnoreCase));

        if (registration == null)
        {
            _logger.LogWarning("Service registration not found for ID: {ServiceId}", serviceId);
            // If the ID isn't found in the file, return null.
            return null;
        }

        var vertical = registration.Tenants?.FirstOrDefault()?.Verticals?.FirstOrDefault();
        if (vertical == null)
        {
            _logger.LogWarning("No vertical configuration found for service ID: {ServiceId}", serviceId);
            return null;
        }

        // Logic to select the connection string for state database
        var connectionString = !string.IsNullOrEmpty(vertical.ReadServerConnectionString)
            ? vertical.ReadServerConnectionString
            : vertical.DefaultServerConnectionString;

        // Get the StateDatabaseName for FeatureAggregateRoots
        var databaseName = vertical.ReadDatabaseName;

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
        {
            _logger.LogWarning("Incomplete read connection details for service ID: {ServiceId}", serviceId);
            // Not enough info in the file to connect.
            return null;
        }

        _logger.LogInformation(
            "Successfully retrieved read connection details for service ID: {ServiceId}, Database: {DatabaseName}",
            serviceId, databaseName);
        // It returns the connection string and the state database name it found.
        return (connectionString, databaseName);
    }
}
using GeneralizeQueries.Core.Models;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Application.Configuration; 
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.IO;

namespace GeneralizeQueries.Application;

public class ServiceRegistrationService : IServiceRegistrationService
{
    private readonly string _filePath;
    private readonly IDynamicMongoRepository _dynamicMongoRepository;

    public ServiceRegistrationService(IOptions<FileSettings> fileSettings, IDynamicMongoRepository dynamicMongoRepository)
    {
        _filePath = fileSettings.Value.ServiceRegistrationsFilePath;
        _dynamicMongoRepository = dynamicMongoRepository;
    }

    public async Task<List<ServiceRegistrationDto>> GetAllAsync()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException("ServiceRegistrations.json file not found.", _filePath);
        }

        var json = await File.ReadAllTextAsync(_filePath);
        var registrations = JsonSerializer.Deserialize<List<ServiceRegistrationDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        return registrations ?? new List<ServiceRegistrationDto>();
    }
    
    public async Task<(string ConnectionString, string DatabaseName)?> GetConnectionDetailsByIdAsync(string serviceId)
    {
        var registrations = await GetAllAsync();

        var registration = registrations.FirstOrDefault(r => r.Id.Equals(serviceId, StringComparison.OrdinalIgnoreCase));

        if (registration == null)
        {
            // If the ID isn't found in the file, return null.
            return null;
        }
        
        var vertical = registration.Tenants?.FirstOrDefault()?.Verticals?.FirstOrDefault();
        if (vertical == null)
        {
            return null;
        }
            
        // Logic to select the connection string
        var connectionString = !string.IsNullOrEmpty(vertical.ReadServerConnectionString)
            ? vertical.ReadServerConnectionString
            : vertical.DefaultServerConnectionString;
            
        // *** THIS IS THE KEY LINE ***
        // It specifically gets the value from the "readDatabaseName" field.
        var databaseName = vertical.ReadDatabaseName;

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
        {
            // Not enough info in the file to connect.
            return null;
        }

        // It returns the connection string and the specific database name it found.
        return (connectionString, databaseName);
    }

    public async Task<List<string>> GetCollectionsForServiceAsync(string serviceId)
    {
        var registrations = await GetAllAsync();

        // Find the registration matching the ID from the frontend.
        var registration = registrations.FirstOrDefault(r => r.Id.Equals(serviceId, StringComparison.OrdinalIgnoreCase));

        if (registration == null)
        {
            // Service ID not found in the file
            return new List<string>(); 
        }

        // Simple logic: Use the first Tenant and first Vertical.
        // You can add more complex logic here if needed.
        var vertical = registration.Tenants?.FirstOrDefault()?.Verticals?.FirstOrDefault();
        if (vertical == null)
        {
            return new List<string>();
        }

        // Determine the connection string and database name to use.
        // Prioritize Read connection, fall back to Default.
        var connectionString = !string.IsNullOrEmpty(vertical.ReadServerConnectionString)
            ? vertical.ReadServerConnectionString
            : vertical.DefaultServerConnectionString;
        
        var databaseName = vertical.ReadDatabaseName;

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
        {
            // Not enough information in the file to connect.
            throw new InvalidOperationException($"Configuration for service '{serviceId}' is missing a connection string or database name.");
        }

        // Use the dynamic repository to connect and get the data.
        return await _dynamicMongoRepository.ListCollectionNamesAsync(connectionString, databaseName);
    }
}
using GeneralizeQueries.Core.Models;

// We can reference DTOs here for convenience

namespace GeneralizeQueries.Core.Interfaces;

public interface IServiceRegistrationService
{
    Task<List<ServiceRegistrationDto>> GetAllAsync();
    Task<(string ConnectionString, string DatabaseName)?> GetStateConnectionDetailsByIdAsync(string serviceId);

    Task<(string ConnectionString, string DatabaseName)?> GetReadConnectionDetailsByIdAsync(string serviceId);
    // Task<List<string>> GetCollectionsForServiceAsync(string serviceId);
}
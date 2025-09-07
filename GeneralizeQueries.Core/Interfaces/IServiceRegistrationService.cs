using GeneralizeQueries.Core.Models; // We can reference DTOs here for convenience

namespace GeneralizeQueries.Core.Interfaces;

public interface IServiceRegistrationService
{
    Task<List<ServiceRegistrationDto>> GetAllAsync();
    Task<(string ConnectionString, string DatabaseName)?> GetConnectionDetailsByIdAsync(string serviceId);
    Task<List<string>> GetCollectionsForServiceAsync(string serviceId);
}
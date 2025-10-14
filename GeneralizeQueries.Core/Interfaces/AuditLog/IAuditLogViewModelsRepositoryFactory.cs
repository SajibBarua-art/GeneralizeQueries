namespace GeneralizeQueries.Core.Interfaces;

public interface IAuditLogViewModelsRepositoryFactory
{
    Task<IAuditLogViewModelsRepository?> CreateRepositoryAsync(string serviceId);
}
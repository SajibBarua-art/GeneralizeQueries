namespace GeneralizeQueries.Core.Interfaces;

public interface IQueryTemplateRepositoryFactory
{
    Task<IQueryTemplateRepository?> CreateRepositoryAsync(string serviceId);
}
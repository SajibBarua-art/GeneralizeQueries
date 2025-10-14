namespace GeneralizeQueries.Core.Interfaces;

public interface IRoleFeatureViewModelsRepositoryFactory
{
    Task<IRoleFeatureViewModelsRepository?> CreateRepositoryAsync(string serviceId);
}
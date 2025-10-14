namespace GeneralizeQueries.Core.Interfaces;

public interface IFeatureViewModelsRepositoryFactory
{
    Task<IFeatureViewModelsRepository?> CreateRepositoryAsync(string serviceId);
}
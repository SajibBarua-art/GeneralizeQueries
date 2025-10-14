namespace GeneralizeQueries.Core.Interfaces;

public interface IFeatureAggregateRootsRepositoryFactory
{
    Task<IFeatureAggregateRootsRepository?> CreateRepositoryAsync(string serviceId);
}
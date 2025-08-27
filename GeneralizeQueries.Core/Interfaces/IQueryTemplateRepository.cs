using GeneralizeQueries.Core.Entities;

namespace GeneralizeQueries.Core.Interfaces;
public interface IQueryTemplateRepository
{
    Task<QueryTemplate?> GetByIdAsync(string id);
    Task<IEnumerable<QueryTemplate>> GetAllAsync();
    Task CreateAsync(QueryTemplate template);
    Task<bool> UpdateAsync(QueryTemplate template);
    Task<bool> DeleteAsync(string id);
}
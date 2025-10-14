using GeneralizeQueries.Core.Entities;
using GeneralizeQueries.Core.Models;

namespace GeneralizeQueries.Core.Interfaces;

public interface IQueryTemplateRepository
{
    Task<QueryTemplate?> GetByIdAsync(string id);
    Task<IEnumerable<QueryTemplate>> GetAllAsync();
    Task<PagedResult<QueryTemplate>> GetPagedAsync(PaginationParameters parameters);
    Task<long> GetCountAsync();
    Task CreateAsync(QueryTemplate template);
    Task<bool> UpdateAsync(QueryTemplate template);
    Task<bool> DeleteAsync(string id);
}
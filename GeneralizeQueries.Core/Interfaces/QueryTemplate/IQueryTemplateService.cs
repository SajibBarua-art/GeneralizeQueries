using GeneralizeQueries.Core.Entities;
using GeneralizeQueries.Core.Models;

namespace GeneralizeQueries.Core.Interfaces;

public interface IQueryTemplateService
{
    Task<IEnumerable<QueryTemplate>> GetAllTemplatesAsync(IQueryTemplateRepository repository);

    Task<PagedResult<QueryTemplate>> GetPagedTemplatesAsync(
        IQueryTemplateRepository repository,
        PaginationParameters parameters);

    Task<long> GetTemplatesCountAsync(IQueryTemplateRepository repository);

    Task<QueryTemplate?> GetTemplateByIdAsync(
        IQueryTemplateRepository repository,
        string id);

    Task CreateTemplateAsync(
        IQueryTemplateRepository repository,
        QueryTemplate template);

    Task<bool> UpdateTemplateAsync(
        IQueryTemplateRepository repository,
        string id,
        QueryTemplate template);

    Task<bool> DeleteTemplateAsync(
        IQueryTemplateRepository repository,
        string id);
}
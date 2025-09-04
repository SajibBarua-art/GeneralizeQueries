using GeneralizeQueries.Core.Entities;
using GeneralizeQueries.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneralizeQueries.Application;

public class QueryTemplateService : IQueryTemplateService
{
    // The service is now stateless and has no dependencies in its constructor.
    // It just contains the business logic, which is to delegate calls.
    public QueryTemplateService() { }

    public async Task<IEnumerable<QueryTemplate>> GetAllTemplatesAsync(IQueryTemplateRepository repository)
    {
        return await repository.GetAllAsync();
    }

    public async Task<QueryTemplate?> GetTemplateByIdAsync(IQueryTemplateRepository repository, string id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task CreateTemplateAsync(IQueryTemplateRepository repository, QueryTemplate template)
    {
        await repository.CreateAsync(template);
    }

    public async Task<bool> UpdateTemplateAsync(IQueryTemplateRepository repository, string id, QueryTemplate template)
    {
        // Business logic: Ensure the ID from the URL is always used.
        template.Id = id;
        return await repository.UpdateAsync(template);
    }

    public async Task<bool> DeleteTemplateAsync(IQueryTemplateRepository repository, string id)
    {
        return await repository.DeleteAsync(id);
    }
}
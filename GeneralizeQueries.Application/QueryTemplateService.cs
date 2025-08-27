using GeneralizeQueries.Core.Entities;
using GeneralizeQueries.Core.Interfaces;

namespace GeneralizeQueries.Application;

public class QueryTemplateService : IQueryTemplateService
{
    private readonly IQueryTemplateRepository _repository;
    public QueryTemplateService(IQueryTemplateRepository repository)
    {
        _repository = repository;
    }

    public Task<QueryTemplate?> GetTemplateByIdAsync(string id) => _repository.GetByIdAsync(id);
    
    public Task<IEnumerable<QueryTemplate>> GetAllTemplatesAsync() => _repository.GetAllAsync();
    
    public async Task CreateTemplateAsync(QueryTemplate template)
    {
        // Here you could add business logic, e.g., validation, checking for duplicates.
        await _repository.CreateAsync(template);
    }
    
    public async Task<bool> UpdateTemplateAsync(string id, QueryTemplate template)
    {
        // Ensure the ID from the URL is used, not one from a potential request body.
        template.Id = id;
        return await _repository.UpdateAsync(template);
    }

    public async Task<bool> DeleteTemplateAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
using GeneralizeAI.Core.Entities;
using GeneralizeAI.Core.Interfaces;

namespace GeneralizeAI.Application;

public class QueryTemplateService : IQueryTemplateService
{
    private readonly IQueryTemplateRepository _repository;
    public QueryTemplateService(IQueryTemplateRepository repository)
    {
        _repository = repository;
    }

    public Task<QueryTemplate?> GetTemplateByIdAsync(string id) => _repository.GetByIdAsync(id);
    
    public Task<IEnumerable<QueryTemplate>> GetAllTemplatesAsync() => _repository.GetAllAsync();
}
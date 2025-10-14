using GeneralizeQueries.Core.Entities;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Application;

public class QueryTemplateService : IQueryTemplateService
{
    private readonly ILogger<QueryTemplateService> _logger;

    public QueryTemplateService(ILogger<QueryTemplateService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<QueryTemplate>> GetAllTemplatesAsync(IQueryTemplateRepository repository)
    {
        _logger.LogInformation("Getting all query templates");
        return await repository.GetAllAsync();
    }

    public async Task<PagedResult<QueryTemplate>> GetPagedTemplatesAsync(
        IQueryTemplateRepository repository,
        PaginationParameters parameters)
    {
        _logger.LogInformation("Getting paged query templates, Page: {Page}, PageSize: {PageSize}", parameters.Page,
            parameters.PageSize);
        return await repository.GetPagedAsync(parameters);
    }

    public async Task<long> GetTemplatesCountAsync(IQueryTemplateRepository repository)
    {
        _logger.LogInformation("Getting query templates count");
        return await repository.GetCountAsync();
    }

    public async Task<QueryTemplate?> GetTemplateByIdAsync(
        IQueryTemplateRepository repository,
        string id)
    {
        _logger.LogInformation("Getting query template by ID: {TemplateId}", id);
        return await repository.GetByIdAsync(id);
    }

    public async Task CreateTemplateAsync(
        IQueryTemplateRepository repository,
        QueryTemplate template)
    {
        _logger.LogInformation("Creating query template with ID: {TemplateId}", template.Id);
        await repository.CreateAsync(template);
    }

    public async Task<bool> UpdateTemplateAsync(
        IQueryTemplateRepository repository,
        string id,
        QueryTemplate template)
    {
        _logger.LogInformation("Updating query template with ID: {TemplateId}", id);
        // Business logic: Ensure the ID from the URL is always used.
        template.Id = id;
        return await repository.UpdateAsync(template);
    }

    public async Task<bool> DeleteTemplateAsync(
        IQueryTemplateRepository repository,
        string id)
    {
        _logger.LogInformation("Deleting query template with ID: {TemplateId}", id);
        return await repository.DeleteAsync(id);
    }
}
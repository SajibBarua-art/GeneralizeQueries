using GeneralizeAI.Api.DTOs;
using GeneralizeAI.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using GeneralizeAI.Core.Entities;

namespace GeneralizeAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryTemplatesController : ControllerBase
{
    private readonly IQueryTemplateService _templateService;

    public QueryTemplatesController(IQueryTemplateService templateService)
    {
        _templateService = templateService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QueryTemplateDto>>> GetAll()
    {
        var templates = await _templateService.GetAllTemplatesAsync();
        
        // Use LINQ's Select to map each entity to its DTO using our helper method
        var dtos = templates.Select(MapToDto);
        
        return Ok(dtos);
    }

    [HttpGet("{id}", Name = "GetTemplateById")]
    public async Task<ActionResult<QueryTemplateDto>> GetById(string id)
    {
        var template = await _templateService.GetTemplateByIdAsync(id);

        if (template is null)
        {
            return NotFound();
        }

        // Manually map from the entity to the DTO
        var dto = new QueryTemplateDto
        {
            Id = template.Id,
            RolesAndIdsAllowedToRead = template.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = template.IsMarkedToDelete,
            Template = new TemplateDetailsDto
            {
                Source = template.Template.Source,
                DynamicFilter = template.Template.DynamicFilter,
                Fields = template.Template.Fields,
                AllowedSorts = template.Template.AllowedSorts,
                Locale = template.Template.Locale,
                IsDynamicFilter = template.Template.IsDynamicFilter,
                CountOnly = template.Template.CountOnly
            }
        };

        return Ok(dto);
    }
    
    private QueryTemplateDto MapToDto(QueryTemplate template)
    {
        return new QueryTemplateDto
        {
            Id = template.Id,
            RolesAndIdsAllowedToRead = template.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = template.IsMarkedToDelete,
            Template = new TemplateDetailsDto
            {
                Source = template.Template.Source,
                DynamicFilter = template.Template.DynamicFilter,
                Fields = template.Template.Fields,
                AllowedSorts = template.Template.AllowedSorts,
                Locale = template.Template.Locale,
                IsDynamicFilter = template.Template.IsDynamicFilter,
                CountOnly = template.Template.CountOnly
            }
        };
    }
}